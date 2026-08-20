using Arch.Common.Utils;
using log4net;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Dao.Crm;
using Lvy.Trip.Dao.Order;
using Lvy.Trip.Dao.Product;
using Lvy.VModels.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Text.Json;

namespace Lvy.Trip.Biz.Booking
{
    /// <summary>
    ///  预定业务模块
    /// </summary>
    public partial class BookingBiz : BaseBiz
    {
        private ILog _logger = LogManager.GetLogger(typeof(BookingBiz));

        private TpQuotaBiz _quotaBiz = new TpQuotaBiz();
        private OrderBiz _orderBiz = new OrderBiz();
        private CustomerBiz _customerBiz = new CustomerBiz();
        private TpLineBiz _lineBiz = new TpLineBiz();

        private TpTourPlanDao _dao = new TpTourPlanDao();
        private TpLineBusPointDao _busPointDao = new TpLineBusPointDao();
        private TpOrderDao ordersDao = new TpOrderDao();
        private TpTravellerDao travellerDao = new TpTravellerDao();
        private TpLineDao _tpLineDao = new TpLineDao();
        private TpLineRouteDao _tpLineRouteDao = new TpLineRouteDao();
        private CustomerDao _customerDao = new CustomerDao();

        /// <summary>
        /// 获取该团信息
        /// </summary>
        /// <returns></returns>
        public TpTourPlanModel GetTourById(int tourId)
        {
            string sql = "SELECT a.*, b.* FROM TpTourPlan a LEFT JOIN TpLine b ON a.LineId=b.LineId WHERE a.Id=@0";
            return _dao.Query<TpTourPlanModel, TpLineModel>(sql, tourId).FirstOrDefault();
        }

        /// <summary>
        /// 获取上车地点
        /// </summary>
        /// <returns></returns>
        public List<TpLineBusPointModel> GetBusPoints(string lineId)
        {
            string sql = "SELECT * FROM TpLineBusPoint WHERE LineId=@0";
            return _busPointDao.Fetch(sql, lineId);
        }

        /// <summary>
        /// 获取该线路的出发日期
        /// </summary>
        /// <returns></returns>
        public List<KeyValueBean> GetOutDateBeansByLineId(int lineId)
        {
            // 状态=上线 的 线路出发日期
            string sql = "SELECT Id AS `Key`, OutDate AS Value FROM TpTourPlan WHERE TourState=3 AND LineId=@0 ";
            return _dao.Query<KeyValueBean>(sql, lineId).ToList();
        }

        /// <summary>
        /// 通过tourId获取该团的价格类型
        /// </summary>
        /// <returns></returns>
        public List<TpPriceModel> GetPricesByTourId(int tourId)
        {
            return new TpPriceDao().GetValidByTourId(tourId);
        }

        public TpTourPlanModel GetPlan(int tourId)
        {
            return _dao.GetById(tourId);
        }

        public TpTourPlanModel GetFirstPlan(string lineId)
        {
            string sql = @"SELECT ttp.* FROM tptourplan ttp
 INNER JOIN tpline tl ON tl.LineId = ttp.lineId
 INNER JOIN tptourquotamap tqm ON tqm.TourId=ttp.Id
 INNER JOIN tpquota tq ON tq.Id=tqm.QuotaId
 WHERE ttp.lineId=@0 AND tl.IsValid=1 AND tl.lineState=3
    AND ttp.TourState=3
    AND tq.UseQuota > 0 ";
            return _dao.FirstOrDefault(sql, lineId);
        }

        /// <summary>
        /// 获取座位分布数据
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public TpBusSeatModel GetSeatDetails(int tourId)
        {
            string sql = @"SELECT c.* FROM TpTourQuotaMap a INNER JOIN TpQuota b ON a.QuotaId=b.Id
INNER JOIN TpBusSeat c ON b.Id=c.QuotaId WHERE a.TourId=@0";

            return _dao.Query<TpBusSeatModel>(sql, tourId).FirstOrDefault();
        }

        #region 预定业务逻辑

        /// <summary>
        /// 下单保存
        /// </summary>
        /// <param name="orderCode"> </param>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public OrderResultState BookingTrans(ref string orderCode, BookingVModel vModel, CrmAccountModel currentUser)
        {
            OrderResultState ERROR_STATE;
            // 关联的上车点
            TpLineBusPointModel busPoint = null;
            if (vModel.BusPointId == 0)//没有上车点的场合
                busPoint = new TpLineBusPointModel();
            else
                busPoint = new TpLineBusPointDao().GetById(vModel.BusPointId);

            vModel.Tour = _dao.GetById(vModel.TourId);
            vModel.Tour.Line = _tpLineDao.GetByLineId(vModel.Tour.LineId);

            TpOrderModel order = CreateOrder(vModel, busPoint, currentUser);
            var travellers = GetTravellers(vModel, order, busPoint, currentUser);
            //-----订单价格------------------------------
            order.TolYsPrice = travellers.Sum(a => a.YsPrice);
            order.JieSuanState = 1;// 初始未支付
            order.SettleCustomer = vModel.SettleCustomer;
            order.SettlePlatForm = vModel.SettlePlatForm;
            order.ContactCode = vModel.ContactCode;
            order.TravellerCount = vModel.Travellers.Count;
            order.RebateInBill = _customerBiz.GetById(vModel.SettleCustomer).RebateInBill;
            //------------------------------------------

            using (var ts = new TransactionScope())
            {
                // 再次判断是否有名额 设定库存+-
                ERROR_STATE = SetQuota(vModel);
                if (ERROR_STATE != OrderResultState.Code100)
                    return ERROR_STATE;

                //if (vModel.Tour.Line.TrafficType == 1)
                //{
                //    // 如果是汽车班
                //    // 设定座位
                //    ERROR_STATE = CheckSeatDetails(vModel);
                //    if (ERROR_STATE != OrderState.Code100)
                //        return ERROR_STATE;
                //}

                // 插入订单
                ordersDao.Insert(order);
                // 插入游客信息
                foreach (var item in travellers)
                {
                    travellerDao.Insert(item);
                }

                // 更新库存
                _orderBiz.FreeQuota(vModel.TourId, order.OrderCode, currentUser.Code);

                ts.Complete();
            }
            orderCode = order.OrderCode;

            return OrderResultState.Code100;
        }

        /// <summary>
        ///  检查是否有库存
        /// </summary>
        /// <returns></returns>
        public OrderResultState SetQuota(BookingVModel vModel)
        {
            var quota = _quotaBiz.GetQuotaByTour(vModel.TourId);
            if (quota == null)
                throw new Exception("团号：{0}库存对象为空！".With(vModel.TourId));

            vModel.Quota = quota;
            int cnt = vModel.Travellers.Count;
            if (quota.UseQuota < cnt)
                // 可用名额小于预定名额
                return OrderResultState.Code110;

            return OrderResultState.Code100;
        }

        /// <summary>
        ///  检查位置状态。未占的场合 设置  已占
        /// </summary>
        public OrderResultState CheckSeatDetails(BookingVModel vModel)
        {
            var seats = GetSeatDetails(vModel.TourId).SeatModels;
            foreach (var traveller in vModel.Travellers)
            {
                var obj = seats.FirstOrDefault(a => a.No == traveller.SeatNum);

                if (obj != null)
                {
                    switch (obj.State)
                    {
                        case 1:  // 未占
                            obj.State = 2;
                            break;

                        case 2:  //已占
                            return OrderResultState.Code101;

                        case 3:  //锁定
                            return OrderResultState.Code102;

                        default:
                            throw new Exception("无此座位状态！");
                    }
                }
            }
            TpBusSeatDao dao = new TpBusSeatDao();
            var jsonSeats = JsonSerializer.Serialize(seats);

            dao.Update(" SET SeatDetail=@0 WHERE QuotaId=@1", Ansi(jsonSeats), vModel.Quota.Id);

            return OrderResultState.Code100;
        }

        /// <summary>
        ///  获取游客集合
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="order"></param>
        /// <param name="busPoint"></param>
        /// <returns></returns>
        private List<TpTravellerModel> GetTravellers(BookingVModel vModel, TpOrderModel order,
            TpLineBusPointModel busPoint, CrmAccountModel currentUser)
        {
            List<TpTravellerModel> travellers = new List<TpTravellerModel>(vModel.Travellers.Count);
            TpTravellerModel traveller = null;
            var placelist = GetPricesByTourId(vModel.TourId);
            var pp = GetPlan(vModel.TourId);
            foreach (var item in vModel.Travellers)
            {
                var tp = placelist.FirstOrDefault(a => a.Id == item.PriceId.ToInt());

                traveller = new TpTravellerModel();
                traveller.OrderCode = order.OrderCode;
                traveller.TourId = order.TourId;
                traveller.Name = item.Name;
                traveller.Phone = item.Phone;
                traveller.PassType = item.PassType;
                traveller.PassNo = item.PassNo;
                traveller.PinYin = item.PinYin;
                traveller.Sex = item.Sex;
                traveller.DateOfBirth = item.DateOfBirth;
                traveller.PlaceOfBirth = item.PlaceOfBirth;
                traveller.DateOfIssue = item.DateOfIssue;
                traveller.PlaceOfIssue = item.PlaceOfIssue;
                traveller.DateOfExpiry = item.DateOfExpiry;
                traveller.SeatNum = item.SeatNum;
                traveller.PriceId = item.PriceId.ToInt();
                traveller.PriceContent = tp.PriceRemark;
                traveller.IsMianPiao = item.IsMianPiao;
                if (item.IsMianPiao == 1)//如果 买一送X的场合
                {
                    traveller.Price = 0;
                    traveller.TeJiaFanLi = 0;
                }
                else
                {
                    traveller.Price = tp.SettlePrice;    // 结算
                    traveller.TeJiaFanLi = tp.TeJiaFanLi;
                }
                traveller.IsOccupiedQuota = tp.SuitNum > 0 ? 1 : 0;

                traveller.SingleRoom = item.IsSingleRoom ? pp.SingleRoom : 0;
                traveller.JiePrice = busPoint.JiePrice;
                traveller.SongPrice = busPoint.SongPrice;
                traveller.ZiFei = item.IsZiFei ? pp.ZiFei : 0;
                traveller.Tax = item.IsTax ? pp.Tax : 0;
                traveller.VisaPrice = item.IsVisaPrice ? pp.VisaPrice : 0;
                traveller.FanLi = item.FanLi.ToDecimal();

                traveller.State = 2;   // 有效
                traveller.Remark = item.Remark;
                traveller.CreatedBy = currentUser.Code;
                traveller.CreatedTime = DateTime.Now;
                traveller.ModifiedBy = currentUser.Code;
                traveller.ModifiedTime = DateTime.Now;
                traveller.YsPrice = traveller.Price + traveller.SingleRoom
                                                    + traveller.JiePrice + traveller.SongPrice
                                                    + traveller.ZiFei
                                                    + traveller.VisaPrice + traveller.Tax
                                                    - traveller.TeJiaFanLi + traveller.FanLi;

                travellers.Add(traveller);
            }
            return travellers;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="vModel"></param>
        /// <param name="busPoint"></param>
        /// <returns></returns>
        private TpOrderModel CreateOrder(BookingVModel vModel, TpLineBusPointModel busPoint, CrmAccountModel currentUser)
        {
            var order = new TpOrderModel();
            order.OrderCode = DBTools.GetSeqNo("TpOrder");
            order.TourId = vModel.TourId;
            order.LineName = vModel.Tour.Line.LineName;
            order.LineId = vModel.Tour.LineId;
            order.LineBusPointId = vModel.BusPointId.ToInt();// todo 废除
            order.LineBusPoint = "";// busPoint.ToJsonSerialize(); // 序列化上车点
            order.SalesTeamId = vModel.SalesTeamId;
            order.SalerCode = vModel.SalerCode;
            var linkInfo = GetLinkInfo(vModel.Travellers);
            order.LinkMan = linkInfo.Name;
            order.LinkPhone = linkInfo.Phone;
            order.OutDate = vModel.Tour.OutDate;

            order.ContactCode = vModel.ContactCode;
            order.Managers = vModel.Managers;
            order.ManagerPhone = vModel.ManagerPhone;
            order.SupplierCode = vModel.Tour.Line.CustomerCode;
            order.BookingAccount = vModel.BookingCustomer.IsNullOrEmpty()
                                       ? currentUser.Code
                                       : null;
            order.BookingCustomer = vModel.BookingCustomer.IsNullOrEmpty()
                                        ? currentUser.CustomerCode
                                        : vModel.BookingCustomer;
            order.OrderState = vModel.OrderState;
            order.TraceState = vModel.TraceState;
            order.OrderSource = vModel.OrderSource == string.Empty ? 1 : vModel.OrderSource.ToInt(); // 默认值 同行=1
            order.JoinOrderCode = vModel.JoinOrderCode;
            if (vModel.BusPointId == 0)
                order.IsJieSong = 0;
            else
                order.IsJieSong = 1;
            order.Remark = vModel.Remark;
            order.Remark2 = vModel.Remark2;
            order.CreatedBy = currentUser.Code;
            order.CreatedTime = DateTime.Now;
            order.ModifiedBy = currentUser.Code;
            order.ModifiedTime = DateTime.Now;
            order.OwnerCode = currentUser.OwnerCode;
            return order;
        }

        /// <summary>
        /// 获取联系人姓名
        /// </summary>
        /// <param name="travellers"></param>
        /// <returns></returns>
        public BookingPostVModel GetLinkInfo(List<BookingPostVModel> travellers)
        {
            if (travellers[0].Name.IsNullOrEmpty())
            {
                return travellers.FirstOrDefault(a => !a.Name.IsNullOrEmpty());
            }
            return travellers[0];
        }

        #endregion 预定业务逻辑

        #region 莫名

        /// <summary>
        /// 获取线路行程对象
        /// </summary>
        /// <param name="lineId"></param>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public RouteVModel GetLineRoute(string lineId, int tourId)
        {
            RouteVModel routeVModel = new RouteVModel();
            routeVModel.LineModel = _tpLineDao.GetByLineId(lineId);
            if (tourId != 0)
                routeVModel.TpTourPlanModel = GetTourById(tourId);
            routeVModel.PlanList = _dao.TourPlanId(lineId);
            routeVModel.FileList = _lineBiz.GetLineFileList(lineId);
            routeVModel.LogoPath = _customerDao.FirstOrDefault(@" SELECT LogoPath FROM CrmCustomer WHERE OwnerCode=@0 ", routeVModel.LineModel.OwnerCode).LogoPath;
            routeVModel.TpLineRoutes = _tpLineRouteDao.Fetch(@"SELECT * FROM TpLineRoute WHERE LineId=@0", lineId);
            if (routeVModel.LineModel.LineSpecial.IsNullOrEmpty())
                routeVModel.LineModel.LineSpecial = "";
            else
                routeVModel.LineModel.LineSpecial = routeVModel.LineModel.LineSpecial.Replace("\r\n", "</br>");
            return routeVModel;
        }

        #endregion 莫名
    }
}