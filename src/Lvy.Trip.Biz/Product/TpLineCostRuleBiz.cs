using Lvy.Models.ProductDB;
using Lvy.Trip.Dao.Product;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Product
{
    /// <summary>
    /// 线路成本规则
    /// </summary>
    public class TpLineCostRuleBiz : BaseBiz
    {
        private readonly TpLineCostRuleDao _dao = new TpLineCostRuleDao();

        public List<TpLineCostRuleModel> GetByLineId(string lineId)
        {
            string sql = " select * from TpLineCostRules where LineId=@0";
            return _dao.Fetch(sql, lineId);
        }


        /// <summary>
        /// 获取成本项目
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public List<TpLineCostRuleModel> GetCostByLineId(string lineId)
        {
            return GetByLineId(lineId);
        }

        public int DeleteCostByLineId(string lineId)
        {
            return _dao.Delete("where lineId=@0", lineId);
        }

        public int Insert(TpLineCostRuleModel model)
        {
            return _dao.Insert(model).ToInt();
        }

        public int InsertBatch(List<TpLineCostRuleModel> models)
        {
            foreach (var model in models)
            {
                Insert(model);
            }
            return 1;
        }
    }
}