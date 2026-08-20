using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.OrderDB;
using Lvy.Models.TourDB;
using Lvy.Trip.Biz;
using Lvy.Trip.Dao.Order;
using Lvy.Trip.Dao.Tour;
using Lvy.Visa.Dao;
using Lvy.Visa.Models;
using Lvy.Visa.VModels;
using Lvy.VModels.Tour;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Visa.Biz
{
    public class VisaOrderBiz : BaseBiz
    {
        //private ApplicanterDao applicanterDao = new ApplicanterDao();
        private OperationHistoryDao historyDao = new OperationHistoryDao();
        private VisaOrderDao visaOrderDao = new VisaOrderDao();
        private TpChildOrderDao _childDao = new TpChildOrderDao();
        private TpTourBalanceDao _balanceDao = new TpTourBalanceDao();

        public PagedList<TpTourBalanceModel> SearchOrderList(VisaOrderQModel qModel)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT ttb.*, vo.*, cc.Name VisaIssuePlaceName, bdd1.Value OrderStatusName,
    bdd2.Value PaymentStatusName, bdd3.Value OrderSourceValue, bdd4.Value statusName, vi.PManageUserName PManageName
FROM TpTourBalance ttb
INNER JOIN Visa_Order vo ON vo.OrderCode = ttb.MasterOrderCode
INNER JOIN Visa_Information vi ON vo.ProductCode = vi.InformationCode
inner join BaseDestination cc on cc.ParentStr = vi.VisaIssuePlace
inner join BaseDictionaryDetail bdd1 on ttb.OrderState=bdd1.`Key` and bdd1.Name='OrderStateEnum'
inner join BaseDictionaryDetail bdd2 on ttb.PaymentStatus=bdd2.`Key` and bdd2.Name='PayStatusEnum'
inner join BaseDictionaryDetail bdd3 on ttb.OrderSource=bdd3.`Key` and bdd3.Name='OrderSourceEnum'
inner join BaseDictionaryDetail bdd4 on vo.TraceState=bdd4.`Key` and bdd4.Name='VisaOrderStatusEnum'
where ttb.OwnerCode=@0 AND vo.TraceState<>7 ", qModel.OwnerCode);

            if (null != qModel && null != qModel.orderQueryModel)
            {
                if (!string.IsNullOrEmpty(qModel.orderQueryModel.TeamID))
                    sql.Append(" AND ttb.TeamID=@0 ", qModel.orderQueryModel.TeamID.Trim());
                if (!qModel.orderQueryModel.OrderCode.IsNullOrEmpty())
                    sql.Append(" AND ttb.MasterOrderCode=@0 ", qModel.orderQueryModel.OrderCode.Trim());
                if (!qModel.orderQueryModel.ProductName.IsNullOrEmpty())
                    sql.Append(" and vo.ProductName LIKE @0 ", AnsiLike(qModel.orderQueryModel.ProductName.Trim()));
                if (!qModel.orderQueryModel.ProduceManager.IsNullOrEmpty())
                    sql.Append(" and vo.PManageName=@0 ", qModel.orderQueryModel.ProduceManager.Trim());
                if (!qModel.orderQueryModel.TourNo.IsNullOrEmpty())
                    sql.Append(" and ttb.TourNo=@0 ", qModel.orderQueryModel.TourNo.Trim());
                if (!qModel.orderQueryModel.BookMan.IsNullOrEmpty())
                    sql.Append(" and vo.BookMan=@0 ", qModel.orderQueryModel.BookMan);
                if (qModel.orderQueryModel.BookDate != null)
                {
                    var t = qModel.orderQueryModel.BookDate.Split('-');
                    sql.Append(" and vo.BookDate>=@0 and vo.BookDate<@1 ", t[0].ToDateTime(), t[1].ToDateTime());
                }

                if (qModel.orderQueryModel.OrderSource != 0)
                    sql.Append(" and vo.OrderSource=@0 ", qModel.orderQueryModel.OrderSource);
                if (!qModel.orderQueryModel.ContactName.IsNullOrEmpty())
                    sql.Append(" and vo.ContactName=@0 ", qModel.orderQueryModel.ContactName);
                if (!qModel.orderQueryModel.ContactTel.IsNullOrEmpty())
                    sql.Append(" and vo.ContactTel=@0 ", qModel.orderQueryModel.ContactTel);
                //申请人
                //if (!qModel.orderQueryModel.ApplicantName.IsNullOrEmpty())
                //{
                //    var guestList = from a in _session.GetAll<view_GetAllGuest>()
                //                        // where a.Name.Equals(qModel.orderQueryModel.ApplicantName)
                //                    where a.Name.Contains(qModel.orderQueryModel.ApplicantName)
                //                    select a.OrderCode;
                //    var tempOrderList = guestList.ToList();
                //    query = query.Where(a => tempOrderList.Contains(a.OrderCode));
                //}
                if (qModel.orderQueryModel.PaymentType != 0)
                    sql.Append(" and vo.PaymentType=@0 ", qModel.orderQueryModel.PaymentType);
                if (qModel.orderQueryModel.PaymentStatus != 0)
                    sql.Append(" and vo.PaymentStatus=@0 ", qModel.orderQueryModel.PaymentStatus);
                if (qModel.orderQueryModel.OrderStatus != 0)
                    sql.Append(" and ttb.OrderState=@0 ", qModel.orderQueryModel.OrderStatus);
                if (qModel.orderQueryModel.TraceState != 0)
                    sql.Append(" and vo.TraceState=@0 ", qModel.orderQueryModel.TraceState);
                if (qModel.orderQueryModel.SendVisaDate != null) //送签时间
                {
                    var t = qModel.orderQueryModel.SendVisaDate.Split('-');
                    sql.Append(" AND vo.SendVisaDate>=@0 AND vo.SendVisaDate<@0 ", t[0].ToDateTime(), t[1].ToDateTime());
                }
                if (qModel.orderQueryModel.MaterialDeadline != null) //材料截止收取日期
                {
                    var t = qModel.orderQueryModel.MaterialDeadline.Split('-');
                    sql.Append(" and vo.MaterialDeadline>=@0 and vo.MaterialDeadline<@0", t[0].ToDateTime(), t[1].ToDateTime());
                }

                if (!qModel.orderQueryModel.OperateName.IsNullOrEmpty())//操作员
                    sql.Append(" and vo.OperateName like @0 ", AnsiLike(qModel.orderQueryModel.OperateName));
                if (qModel.orderQueryModel.InterviewDate != null)//面试日期
                {
                    var t = qModel.orderQueryModel.InterviewDate.Split('-');
                    sql.Append(" and vo.InterviewDate>=@0 and vo.InterviewDate<@0 ", t[0].ToDateTime(), t[1].ToDateTime());
                }

                if (qModel.orderQueryModel.FollowupDate != null)//跟进日期
                {
                    var t = qModel.orderQueryModel.FollowupDate.Split('-');
                    sql.Append(" and vo.FollowupDate>=@0 and vo.FollowupDate<@0 ", t[0].ToDateTime(), t[1].ToDateTime());
                }
            }
            sql.Append(" Order By ttb.CreatedTime DESC ");

            return visaOrderDao.Pager<TpTourBalanceModel>(qModel.visaOrderModelsList.PageIndex, qModel.visaOrderModelsList.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 取得父订单
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public TpTourBalanceModel GetMasterOrder(string orderCode)
        {
            return _balanceDao.FirstOrDefault(@"SELECT ttb.*, bdd1.Value OrderStatusName,
    bdd2.Value PaymentStatusName, bdd3.Value OrderSourceValue
FROM TpTourBalance ttb
inner join BaseDictionaryDetail bdd1 on ttb.OrderState = bdd1.`Key` and bdd1.Name = 'VisaOrderStatusEnum'
inner join BaseDictionaryDetail bdd2 on ttb.PaymentStatus = bdd2.`Key` and bdd2.Name = 'PayStatusEnum'
inner join BaseDictionaryDetail bdd3 on ttb.OrderSource = bdd3.`Key` and bdd3.Name = 'OrderSourceEnum'
where ttb.MasterOrderCode = @0 ", Ansi(orderCode));
        }

        /// <summary>
        /// 取得签证订单
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public VisaOrderModel GetVisaOrderByCode(string orderCode)
        {
            return visaOrderDao.FirstOrDefault(@"SELECT vo.*, cc.Name VisaIssuePlaceName,
    bdd4.Value TraceStateName, vi.PManageUserName PManageName
FROM  Visa_Order vo
INNER JOIN Visa_Information vi ON vo.ProductCode = vi.InformationCode
inner join BaseDestination cc on cc.ParentStr = vi.VisaIssuePlace
inner join BaseDictionaryDetail bdd4 on vo.TraceState = bdd4.`Key` and bdd4.Name = 'VisaOrderStatusEnum'
where vo.OrderCode = @0", Ansi(orderCode));
        }

        public List<CommonOrderModel> GetCommonOrderByCode(string orderCode)
        {
            return visaOrderDao.Query<CommonOrderModel>(@"SELECT t.OrderCode, t.ProductCode AS ProductId, t.TotalNum AS TravellerCount,
vi.InformationName AS ProductName, c.Name AS AgentName, ca.Name AS SalerName,
t.Price TolYsPrice, ttb.AgentCode,  ttb.SalesTeamId, ttb.SalerCode
FROM  Visa_Order t
INNER JOIN Visa_Information vi ON t.ProductCode = vi.InformationCode
INNER JOIN tptourbalance ttb ON ttb.MasterOrderCode = t.OrderCode
LEFT JOIN CrmCustomer c ON ttb.AgentCode = c.Code
LEFT JOIN crmaccount ca ON ttb.SalerCode = ca.Code
WHERE t.OrderCode=@0
union
SELECT t.ChildOrderCode AS OrderCode, t.ProductID, t.Quantity AS TravellerCount,
vi.ProductName, c.Name AS AgentName, ca.Name AS SalerName,
 t.Amount TolYsPrice, ttb.AgentCode, ttb.SalesTeamId, ttb.SalerCode
FROM  tpchildorders t
INNER JOIN tpproducts vi ON t.ProductID = vi.ProductID
INNER JOIN tptourbalance ttb ON ttb.MasterOrderCode = t.OrderCode
LEFT JOIN CrmCustomer c ON ttb.AgentCode = c.Code
LEFT JOIN crmaccount ca ON ttb.SalerCode = ca.Code
WHERE t.OrderCode=@0", Ansi(orderCode)).ToList();
        }

        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="qModel"></param>
        public void SaveVisaOrder(BookingQModel qModel, CrmAccountModel currentUser)
        {
            qModel.TotAmount = qModel.ProductModel.SellPrice * (qModel.TotPeopleNum);
            qModel.ProductCode = qModel.ProductModel.InformationCode;
            /// 总订单
            SaveVisaOrder1(qModel, currentUser);

            #region 操作历史

            VisaOperationHistoryModel visaOrderHisModel = new VisaOperationHistoryModel();
            visaOrderHisModel.Ip = qModel.ClientIP;
            visaOrderHisModel.Name =currentUser.Name;
            visaOrderHisModel.OperateId = currentUser.Code;
            visaOrderHisModel.OperateContent = "直客预定";
            visaOrderHisModel.OperateDate = DateTime.Now;
            visaOrderHisModel.OperateType = 1;
            visaOrderHisModel.OrderCode = qModel.OrderCode;
            visaOrderHisModel.Role = "";
            historyDao.Insert(visaOrderHisModel);

            #endregion 操作历史
        }

        private void SaveVisaOrder1(BookingQModel qModel, CrmAccountModel currentUser)
        {
            var entity = new TpTourBalanceModel();
            entity.OwnerCode = qModel.OwnerCode;
            entity.MasterOrderCode = DBTools.GetSeqNo("TourBalance");
            entity.Type = 3;        // 固定签证
            entity.IsPackage = 1;   // 非报团
            entity.ProductType = 3;
            entity.TeamId = qModel.ProductModel.TeamID;
            entity.ProductName = qModel.ProductModel.InformationName;
            entity.ContactName = qModel.LinkName;
            entity.ContactEmail = qModel.LinkEmail;
            entity.ContactPhone = qModel.LinkPhone;
            entity.YingShou = qModel.TotAmount;//应收
            entity.YiShou = 0;         //实收
            entity.TotalCost = 0;      //应付
            entity.TotalRealpay = 0;   //实付
            entity.MaoLi = 0;          //利润
            entity.Remarks = qModel.CustMessage;//备注
            entity.OutDate = qModel.ReadyDate;
            entity.CreatedBy = currentUser.Code;
            entity.CreatedTime = DateTime.Now;
            entity.AuditPax = qModel.AudltNum;              //成人人数
            entity.ChildPax = qModel.ChildNum;              //儿童人数
            entity.Num = qModel.AudltNum + qModel.ChildNum; //总人数
            entity.OrderSource = qModel.OrderSource;
            entity.LaterPayDate = DateTime.Now.AddHours(qModel.ProductModel.PayTimeLimit);    //最晚支付时间
            entity.ContractType = qModel.ContractType.IsNullOrEmpty() ? 1 : Convert.ToInt32(qModel.ContractType);
            entity.OrderState = 1;
            entity.PaymentStatus = 1;//未支付
            entity.IsCancel = 0;

            // 发票部分
            entity.IsneedInvoice = qModel.IsneedInvoice;//是否需要发票
            entity.PostCode = qModel.PostCode;
            _balanceDao.Insert(entity);

            var orderObj = new VisaOrderModel();
            orderObj.OrderCode = entity.MasterOrderCode;
            orderObj.TraceState = 1;//已预定
            orderObj.ProductCode = qModel.ProductCode;
            orderObj.ProductName = qModel.ProductModel.InformationName;
            orderObj.Price = qModel.ProductModel.SellPrice;//单价
            orderObj.TotalNum = qModel.AudltNum + qModel.ChildNum;
            orderObj.SupplierCode = "";
            orderObj.SupplierName = "";
            orderObj.MaterialDeadline = qModel.ReadyDate.Value.AddDays(-7);//出行日期-7天-办理工作日

            visaOrderDao.Insert(orderObj);
        }

        public void SaveOperateName(VisaOrderModel model, CrmAccountModel currentUser, string ip)
        {
            visaOrderDao.Update("SET OperateName=@1 WHERE OrderCode=@0  ", model.OrderCode, model.OperateName);
            //添加到操作历史
            OperationHistoryAdd(model.OrderCode, "修改了操作员", currentUser, ip);
        }

        public void SaveMaterialDeadline(VisaOrderModel model, CrmAccountModel currentUser, string ip)
        {
            visaOrderDao.Update("SET MaterialDeadline=@1 WHERE OrderCode=@0  ", model.OrderCode, model.MaterialDeadline);
            //添加到操作历史
            OperationHistoryAdd(model.OrderCode, "修改了材料截止收取日期", currentUser, ip);
        }

        public void SaveOrderReadyDate(TpTourBalanceModel model,CrmAccountModel currentUser, string ip)
        {
            _balanceDao.Update("SET OutDate=@1 WHERE MasterOrderCode=@0  ", model.MasterOrderCode, model.OutDate);
            //添加到操作历史
            OperationHistoryAdd(model.MasterOrderCode, "修改了预约出发日期", currentUser, ip);
        }

        public void SaveFollowData(VisaOrderModel model)
        {
            visaOrderDao.Update("SET FollowupDate=@1 WHERE OrderCode=@0  ", model.OrderCode, model.FollowupDate);
            //添加到操作历史
            //OperationHistoryAdd(model.OrderCode, "修改了材料截止收取日期");
        }


        /// <summary>
        /// 修改订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int OrderModify(TpTourBalanceModel model, CrmAccountModel currentUser, string ip)
        {
            int i = 0;
            var entity = GetVisaOrderByCode(model.MasterOrderCode);
            if (entity.TraceState != model.VisaOrder.TraceState)   // 检验状态是否改变
            {
                var masterOrder = GetMasterOrder(model.MasterOrderCode);
                // 主表
                if (model.ContactEmail != null) { masterOrder.ContactEmail = model.ContactEmail; }
                if (model.ContactName != null) { masterOrder.ContactName = model.ContactName; }
                if (model.ContactPhone != null) { masterOrder.ContactPhone = model.ContactPhone; }
                if (model.PostCode != null) { masterOrder.PostCode = model.PostCode; }
                if (model.DeliveryAddress != null) { masterOrder.DeliveryAddress = model.DeliveryAddress; }

                _balanceDao.Update(masterOrder);

                // 签证表
                //if (model.VisaOrder.TraceState != null) { entity.TraceState = model.VisaOrder.TraceState; }
                //if (model.VisaOrder.OrderState != null) { entity.OrderState = model.VisaOrder.OrderStatus; }
                if (model.VisaOrder.SendVisaDate != null) { entity.SendVisaDate = model.VisaOrder.SendVisaDate; }
                if (model.VisaOrder.FinishVisaDate != null) { entity.FinishVisaDate = model.VisaOrder.FinishVisaDate; }

                // 记录操作日志
                if (!string.IsNullOrEmpty(model.ContactName)) { OperationHistoryAdd(model.MasterOrderCode, currentUser.Name + "修改订单联系人的信息。", currentUser, ip); }
                if (model.VisaOrder.TraceState == 2) { OperationHistoryAdd(model.MasterOrderCode, currentUser.Name + "外乎确认订单的信息。", currentUser, ip); }
                if (model.VisaOrder.TraceState == 5) { OperationHistoryAdd(model.MasterOrderCode, currentUser.Name + "设置订单送签。", currentUser, ip); }
                if (model.VisaOrder.TraceState == 6) { OperationHistoryAdd(model.MasterOrderCode, currentUser.Name + "设置出签的订单信息。", currentUser, ip); }
            }
            else
            { i = 1; }
            return i;
        }

        public void ModifyOrderContactInfo(TpTourBalanceModel model, CrmAccountModel currentUser, string ip)
        {
            var entity = GetMasterOrder(model.MasterOrderCode);

            entity.AgentCode = model.AgentCode;
            entity.ContactName = model.ContactName;
            entity.ContactPhone = model.ContactPhone;
            entity.ContactEmail = model.ContactEmail;
            entity.PostCode = model.PostCode;
            entity.DeliveryAddress = model.DeliveryAddress;
            entity.SalesTeamId = model.SalesTeamId;
            entity.SalerCode = model.SalerCode;

            _balanceDao.Update(entity);

            OperationHistoryAdd(model.MasterOrderCode, currentUser.Name + "修改订单联系人的信息。", currentUser, ip);
        }


        /// <summary>
        /// 签证材料状态
        /// </summary>
        /// <param name="val"></param>
        /// <param name="code"></param>
        public void SetModelStatus(string val, string code)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 取得订单对应产品的签证人群类型列表
        /// </summary>
        /// <param name="ordercode"></param>
        /// <returns></returns>
        public List<VisaCategoryModel> GetApplicanterCategory(string ordercode)
        {
            var entity = GetVisaOrderByCode(ordercode);
            return new ProductBiz().GetCategroyList(entity.ProductCode);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public void CancelOrder(string orderCode, string v1, string v2)
        {
            throw new NotImplementedException();
        }


        public void SaveAuditOrderMaterials(OrderQModel model)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 外呼确认 改变状态
        /// </summary>
        /// <param name="orderCode"></param>
        public void ConfirmSaveVisaOrder(string orderCode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 更新付款状态
        /// </summary>
        /// <param name="orderCode"></param>
        public void UpdatePayState(string orderCode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 是否全额付清
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public bool IsFullPayMent(string orderCode)
        {
            var master = ReCount(orderCode);
            if (master.YingShou <= master.YiShou)
                return true;

            return false;
        }

        /// <summary>
        /// 重新计算 应收
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public TpTourBalanceModel ReCount(string orderCode)
        {
            var master = GetMasterOrder(orderCode);
            var visa = GetVisaOrderByCode(orderCode);
            var child = SearchChildOrderList(orderCode);  //子订单
            var c = child.Where(m => m.IsCancel == 0).Sum(m => m.Amount);
            var v = visa.Price * visa.TotalNum;
            if (master.YingShou != c + v)
            {
                master.YingShou = c + v;
                _balanceDao.Update(master);
            }

            return master;
        }

        #region 操作历史

        /// <summary>
        /// 取得订单操作历史
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public List<VisaOperationHistoryModel> SearchOrderHistoryList(string orderCode)
        {
            return historyDao.Fetch("select * from Visa_OperationHistory where OrderCode=@0", orderCode);
        }


        public void SaveOrderOperateHistory(VisaOperationHistoryModel model, CrmAccountModel currentUser)
        {
            model.OperateDate = System.DateTime.Now;
            model.Name = currentUser.Name;
            model.OperateId = currentUser.Code;
            historyDao.Insert(model);
        }

        public void OperationHistoryAdd(string orderCode, string content, CrmAccountModel currentUser, string ip)
        {
            VisaOperationHistoryModel models = new VisaOperationHistoryModel();
            models.OrderCode = orderCode;
            models.OperateDate = DateTime.Now;
            models.OperateContent = content;
            models.Ip = ip;
            models.Name = currentUser.Name;
            models.OperateId = currentUser.Code;
            historyDao.Insert(models);
        }

        #endregion

        #region 申请人
        //public List<VisaApplicanterModel> SearchOrderTrallerList(string orderCode)
        //{
        //    return applicanterDao.Fetch("select * from Visa_Applicanter where OrderCode=@0", orderCode);
        //}

        //public VisaApplicanterModel GetTravellerDetail(string orderCode, int applicanterID)
        //{
        //    return applicanterDao.FirstOrDefault("select * from Visa_Applicanter where OrderCode=@0 and ID=@1", orderCode, applicanterID);
        //}

        //public void UpdateTravellerInfo(VisaApplicanterModel model)
        //{
        //    var entity = GetTravellerDetail(model.OrderCode, model.Id);
        //    if (null != entity)
        //    {
        //        entity.Name = model.Name;
        //        entity.Pinyin = OperateCommon.ConvertHanZiToPinYin(entity.Name);
        //        entity.Sex = model.Sex;
        //        entity.Birthday = model.Birthday;
        //        entity.Phone = model.Phone;
        //        entity.Categorycode = model.Categorycode;
        //        entity.Status = model.Status;
        //        entity.MidifiedTime = DateTime.Now;
        //        entity.CardType = model.CardType;
        //        entity.CardNo = model.CardNo;

        //        applicanterDao.Update(entity);
        //    }
        //}

        //public VisaApplicanterModel QueryApplicanterDataSetById(int id)
        //{
        //    return applicanterDao.GetById(id);
        //}

        //public void ApplicanterUpdate(VisaApplicanterModel model)
        //{
        //    applicanterDao.Update(model);
        //}

        //public void ApplicanterDelete(int id)
        //{
        //    var entity = applicanterDao.GetById(id);
        //    if (entity.IsValid == 1)
        //        entity.IsValid = 0;
        //    else
        //        entity.IsValid = 1;

        //    applicanterDao.Update(entity);
        //}

        //public void ApplicanterAdd(VisaApplicanterModel model)
        //{
        //    #region 订单修改
        //    var ordermodel = GetVisaOrderByCode(model.OrderCode);
        //    var masterOrder = GetMasterOrder(model.OrderCode);

        //    masterOrder.YingShou = masterOrder.YingShou + ordermodel.Price;
        //    ordermodel.TotalNum = ordermodel.TotalNum + 1;
        //    if (model.Type == 0)
        //    {
        //        masterOrder.ChildPax = masterOrder.ChildPax + 1;
        //        //ordermodel.TotalPay = masterOrder.TotalPay + rcmodel.ChildPrice;
        //    }
        //    else
        //    {
        //        masterOrder.AuditPax = masterOrder.AuditPax + 1;
        //        //ordermodel.TotalPay = masterOrder.TotalPay + rcmodel.AdultManPrice;
        //    }
        //    if (masterOrder.YiShou < masterOrder.YingShou) { masterOrder.PaymentStatus = 1; }

        //    _balanceDao.Update(masterOrder);
        //    #endregion


        //    applicanterDao.Insert(model);
        //    #region 添加操作历史
        //    OperationHistoryAdd(model.OrderCode, GlobalContext.Current.UserInfo.Name + "添加申请人：" + model.Name);
        //    OperationHistoryAdd(model.OrderCode, GlobalContext.Current.UserInfo.Name + "添加申请人后订单状态改变为【材料收取中 】");
        //    #endregion
        //}


        //private void AddBathApplyGuests(IList<VisaApplicanterModel> triplist, String orderCode)
        //{
        //    if (null != triplist && triplist.Count > 0)
        //    {
        //        foreach (var trip in triplist)
        //        {
        //            //申请人
        //            var tripObj = new VisaApplicanterModel();

        //            tripObj.ApplicanterCode = "V" + DBTools.GetSeqNo("12");
        //            trip.ApplicanterCode = tripObj.ApplicanterCode;
        //            tripObj.OrderCode = orderCode;
        //            tripObj.Pinyin = ""; // OperateCommon.ConvertHanZiToPinYin(tripObj.Name);//获取拼音的方法
        //            tripObj.Status = 0;
        //            tripObj.Isvalid = 1;
        //            tripObj.CreatDate = DateTime.Now;
        //            tripObj.MidifyDate = DateTime.Now;
        //            applicanterDao.Insert(tripObj);
        //        }
        //    }
        //}
        #endregion

        #region 子订单

        /// <summary>
        /// 取得子订单列表
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public List<TpChildOrderModel> SearchChildOrderList(string orderCode)
        {
            return _childDao.Fetch("SELECT * FROM tpchildorders WHERE OrderCode=@0", orderCode);
        }

        /// <summary>
        /// 添加子订单
        /// </summary>
        /// <param name="model"></param>
        public void OrderdetailAdd(TpChildOrderModel model, CrmAccountModel currentUser, string ip)
        {
            model.ChildOrderCode = "Z" + DBTools.GetSeqNo("ChildOrder");
            _childDao.Insert(model);
            OperationHistoryAdd(model.OrderCode, currentUser.Name + "添加了子订单{" + model.Id + "}", currentUser, ip);
        }

        /// <summary>
        /// 更新子订单
        /// </summary>
        /// <param name="model"></param>
        public void OrderdetailUpdate(TpChildOrderModel model, CrmAccountModel currentUser, string ip)
        {
            _childDao.Update(model);
            OperationHistoryAdd(model.OrderCode, currentUser.Name + "修改了子订单信息。", currentUser, ip);
        }

        #endregion

    }
}