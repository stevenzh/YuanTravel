using Lvy.Models.CrmDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Lvy.Trip.Biz.Product
{
    /// <summary>
    /// 线路行程
    /// </summary>
    public class TpLineRouteBiz : BaseBiz
    {
        private readonly TpLineRouteDao _bpDao = new TpLineRouteDao();
        private readonly TpLineTrafficDao _trafficDao = new TpLineTrafficDao();

        /// <summary>
        /// 根据行程Id获取行程信息
        /// </summary>
        /// <param name="id">行程Id </param>
        public TpLineRouteModel GetRouteById(int id)
        {
            return _bpDao.FirstOrDefault(@"SELECT * FROM TpLineRoute WHERE Id=@0", id);
        }

        /// <summary>
        /// 根据线路Id与第几天获取行程信息
        /// </summary>
        /// <param name="lineId">线路Id </param>
        /// <param name="day">第几天 </param>
        public TpLineRouteModel GetRouteByLineIdAndDay(int lineId, int day)
        {
            return _bpDao.FirstOrDefault(@"SELECT * FROM TpLineRoute WHERE LineId=@0 AND Days=@1", lineId, day);
        }

        /// <summary>
        /// 根据线路Id获取行程列表
        /// </summary>
        /// <param name="lineId">线路Id </param>
        public List<TpLineRouteModel> GetRouteListByLineId(string lineId)
        {
            return _bpDao.Fetch(@"SELECT * FROM TpLineRoute WHERE LineId=@0", lineId);
        }

        /// <summary>
        /// 批量新增行程
        /// </summary>
        /// <param name="models"></param>
        public void AddRoutes(List<TpLineRouteModel> models)
        {
            using (var tran = _bpDao.GetTransaction())
            {
                foreach (var record in models)
                {
                    _bpDao.Insert(record);
                }
                tran.Complete();
            }
        }

        /// <summary>
        /// 更新行程
        /// </summary>
        /// <param name="model"></param>
        public int UpdateRoute(TpLineRouteModel model, CrmAccountModel userInfo)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    if (model.LineTrafficList == null || model.LineTrafficList.Count == 0)
                    {
                        _trafficDao.Delete(@"WHERE LineRouteId=@0", model.Id);
                    }
                    else
                    {
                        _trafficDao.Delete(@"WHERE LineRouteId=@0", model.Id);
                        foreach (var item in model.LineTrafficList)
                        {
                            item.LineRouteId = model.Id;
                            _trafficDao.Insert(item);
                        }
                    }

                    _bpDao.Update(model);
                    scope.Complete();
                }
            }
            catch (Exception)
            {
                return 0;
            }
            return 1;
        }

        public List<TpLineTrafficModel> GetLineTrafficList(int lineRouteId)
        {
            Sql sql = new Sql();
            sql.Append(" select * from TpLineTraffics where LineRouteId=@0 ", lineRouteId);

            return _bpDao.Query<TpLineTrafficModel>(sql.SQL, sql.Arguments).ToList<TpLineTrafficModel>();
        }

        public List<TpLineTrafficModel> GetTrafficListByLineId(string lineId)
        {
            Sql sql = new Sql();
            sql.Append("SELECT tt.* FROM TpLineTraffics tt INNER JOIN TpLineRoute tr ON tr.Id=tt.LineRouteId WHERE tr.LineId=@0 ", lineId);

            return _bpDao.Query<TpLineTrafficModel>(sql.SQL, sql.Arguments).ToList<TpLineTrafficModel>();
        }

        /// <summary>
        /// 更新行程
        /// </summary>
        /// <param name="model"></param>
        public int UpdateRoute(List<TpLineRouteModel> list, CrmAccountModel userInfo)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    foreach (var model in list)
                    {
                        var currentModel = GetRouteById(model.Id);
                        currentModel.Title = model.Title;
                        currentModel.Catering = model.Catering;
                        currentModel.Breakfast = model.Breakfast;
                        currentModel.Lunch = model.Lunch;
                        currentModel.Supper = model.Supper;
                        currentModel.Hotel = model.Hotel;
                        currentModel.Contents = model.Contents;
                        currentModel.ModifiedBy = userInfo.Code;
                        currentModel.ModifiedTime = DateTime.Now;
                        if (model.LineTrafficList == null || model.LineTrafficList.Count == 0)
                        {
                            _trafficDao.Delete(@"WHERE LineRouteId=@0", model.Id);
                        }
                        else
                        {
                            _trafficDao.Delete(@"WHERE LineRouteId=@0", model.Id);
                            foreach (var item in model.LineTrafficList)
                            {
                                item.LineRouteId = model.Id;
                                _trafficDao.Insert(item);
                            }
                        }
                        _bpDao.Update(currentModel);
                    }

                    scope.Complete();
                }
            }
            catch (Exception)
            {
                return 0;
            }
            return 1;
        }
    }
}