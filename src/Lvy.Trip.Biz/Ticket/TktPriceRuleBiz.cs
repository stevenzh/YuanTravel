using Lvy.Models.CrmDB;
using Lvy.Models.TicketDB;
using Lvy.Trip.Dao.Ticket;
using Lvy.VModels.Ticket;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Lvy.Trip.Biz.Ticket
{
    public class TktPriceRuleBiz : BaseBiz
    {
        private readonly TktPriceRuleDao _dao = new TktPriceRuleDao();
        private readonly TktPriceDao _priceDao = new TktPriceDao();
        private readonly TktRulePriceMapDao _mapDao = new TktRulePriceMapDao();
        private readonly TktProductDao _productDao = new TktProductDao();


        #region Price

        /// <summary>
        /// 取得该时间段规则的 所有价格
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public List<TktPriceModel> GetPriceList(int ruleId)
        {
            return _priceDao.Fetch(@"SELECT * FROM TktPrice WHERE RuleId = @0", ruleId);
        }

        public List<TktPriceModel> GetPriceListByProduct(string productId)
        {
            return _priceDao.Fetch(@"SELECT A.* FROM TktPrice A
LEFT JOIN TktPriceRule B ON B.Id = A.RuleId
WHERE B.ProductId = @0", productId);
        }

        #endregion Price

        #region PriceRule

        /// <summary>
        /// 获取报价规则
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public TktPriceRuleModel GetModel(int ruleId)
        {
            return _dao.GetById(ruleId);
        }


        /// <summary>
        /// 根据产品Id、年份 获取报价规则
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="year"></param>
        /// <param name="isGeneral"></param>
        /// <returns></returns>
        public TktPriceRuleModel GetModel(string productId, int isGeneral)
        {
            return _dao.SingleOrDefault(@"SELECT * FROM TktPriceRule WHERE ProductId = @0 AND IsGeneral = @1", productId, isGeneral);
        }

        /// <summary>
        /// 根据产品Id、年份与是否常规 获取报价规则
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="isGeneral"></param>
        /// <returns></returns>
        public List<TktPriceRuleModel> GetModels(string productId, int isGeneral)
        {
            return _dao.Fetch(@"SELECT * FROM TktPriceRule WHERE ProductId = @0 AND IsGeneral = @1 ", productId, isGeneral);
        }

        /// <summary>
        /// 获取报价规则列表
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<TktPriceRuleModel> GetModels(string productId)
        {
            return _dao.Fetch(@"SELECT * FROM TktPriceRule WHERE ProductId = @0", productId);
        }

        ///// <summary>
        ///// 获取报价规则列表(startDate与endDate范围内)
        ///// </summary>
        ///// <param name="productId"></param>
        ///// <param name="startDate"></param>
        ///// <param name="endDate"></param>
        ///// <returns></returns>
        //public List<TktPriceRuleModel> GetModels(int productId, int startDate, int endDate)
        //{
        //    return _dao.Fetch(@"SELECT * FROM TktPriceRule WHERE ProductId = @0 AND StartDate >= @1 AND EndDate <= @2 ", productId, startDate, endDate);
        //}

        ///// <summary>
        ///// 获取报价规则列表(startDate与endDate范围内)
        ///// </summary>
        ///// <param name="productId"></param>
        ///// <param name="startDate"></param>
        ///// <param name="endDate"></param>
        ///// <param name="isGeneral"></param>
        ///// <returns></returns>
        //public List<TktPriceRuleModel> GetModels(int productId, int startDate, int endDate, int isGeneral)
        //{
        //    return _dao.Fetch(@"SELECT * FROM TktPriceRule WHERE ProductId = @0 AND StartDate >= @1 AND EndDate <= @2 AND IsGeneral = @3 ", productId, startDate, endDate, isGeneral);
        //}

        #region General

        /// <summary>
        /// 新增常规报价
        /// </summary>
        /// <param name="vModel"></param>
        public void AddGeneral(EditPriceVModel vModel, CrmAccountModel userInfo)
        {
            var time = DateTime.Now;
            var priceRule = vModel.PriceRule;
            var priceList = vModel.PriceList;
            var ticket = _productDao.GetByProductId(vModel.TkcketProduct.ProductId);

            priceRule.ProductId = vModel.TkcketProduct.ProductId;
            priceRule.RuleName = "常规报价";
            priceRule.IsGeneral = 1;
            priceRule.IsValid = 1;
            var standard = priceList.SingleOrDefault(p => p.IsStandard == 1);
            if (null == standard)
                throw new NullReferenceException("不存在标准价。");
            priceRule.PriceType = standard.PriceType;
            priceRule.MarketPrice = standard.MarketPrice;
            priceRule.SettlePrice = standard.SettlePrice;
            priceRule.SysPrice = standard.SysPrice;
            priceRule.TktType = ticket.TktType;

            foreach (var price in priceList)
            {
                price.TktType = ticket.TktType;
                price.ModifiedBy = userInfo.Code;
                price.ModifiedTime = time;
                price.OwnerCode = userInfo.OwnerCode;
                price.PriceDesc = price.PriceDesc ?? string.Empty;
            }
            var map = new TktRulePriceMapModel { ProductId = vModel.TkcketProduct.ProductId };

            using (var scope = new TransactionScope())
            {
                var ruleId = Convert.ToInt32(_dao.Insert(priceRule));
                foreach (var price in priceList)
                {
                    price.RuleId = ruleId;
                    _priceDao.Insert(price);
                }

                scope.Complete();
            }
        }

        /// <summary>
        /// 编辑常规报价
        /// </summary>
        /// <param name="vModel"></param>
        public void UpdateGeneral(EditPriceVModel vModel, CrmAccountModel userInfo)
        {
            var time = DateTime.Now;
            var generalRule = GetModel(vModel.PriceRule.Id);
            var priceList = vModel.PriceList;  // 页面提交价格列表

            #region 常规规则可修改部分

            var standard = priceList.SingleOrDefault(p => p.IsStandard == 1);
            if (null == standard)
                throw new NullReferenceException("不存在标准价。");
            generalRule.PriceType = standard.PriceType;
            generalRule.MarketPrice = standard.MarketPrice;
            generalRule.SettlePrice = standard.SettlePrice;
            generalRule.SysPrice = standard.SysPrice;
            //generalRule.TktType = standard.TktType;

            #endregion 常规规则可修改部分

            #region 价格（新增、更新）

            var ticket = _productDao.GetByProductId(vModel.TkcketProduct.ProductId);
            var addPriceList = new List<TktPriceModel>();
            var updatePriceList = new List<TktPriceModel>();
            var dbPriceList = GetPriceList(generalRule.Id);  // 原有价格记录
            foreach (var price in priceList)
            {
                if (price.Id == 0)
                {
                    //price.RuleId = priceRule.Id;
                    price.TktType = ticket.TktType;
                    price.ModifiedBy = userInfo.Code;
                    price.ModifiedTime = time;
                    price.OwnerCode = userInfo.OwnerCode;
                    price.PriceDesc = price.PriceDesc ?? string.Empty;
                    addPriceList.Add(price);
                }
                else if (price.Id > 0)
                {
                    var update = dbPriceList.Find(p => p.Id == price.Id);
                    //update.PriceType = price.PriceType;
                    update.MarketPrice = price.MarketPrice;
                    update.SettlePrice = price.SettlePrice;
                    update.SysPrice = price.SysPrice;
                    update.IsValid = price.IsValid;
                    update.IsStandard = price.IsStandard;
                    update.PriceDesc = price.PriceDesc;
                    updatePriceList.Add(update);
                    //deletePriceList.Remove(update);
                }
            }

            #endregion 价格（新增、更新）

            //该常规报价规则下所有报价规则（包含本身）
            var rules = GetModels(vModel.TkcketProduct.ProductId);

            #region 执行数据操作

            using (var scope = new TransactionScope())
            {
                //更新常规报价规则
                _dao.Update(generalRule);

                /*
                 * 对于新增的价格类型，
                 * 默认为当前常规报价规则下所有报价规则添加该新增价格类型
                 */
                foreach (var price in addPriceList)
                {
                    foreach (var rule in rules)
                    {
                        price.RuleId = rule.Id;
                        _priceDao.Insert(price);
                    }
                }
                /*
                 * 对于变更的价格类型，
                 * 仅为当前常规报价规则进行更新
                 */
                foreach (var price in updatePriceList)
                {
                    _priceDao.Update(price);
                }

                scope.Complete();
            }

            #endregion 执行数据操作


            #region 价格规则变更
            if (vModel.PriceMode == 1 && ticket.PriceMode == 2)
            {
                ticket.PriceMode = 1;
                // 日期模式变为固定价格
                // 其他价格失效  应用这个价格的 表更新成标准价
                _productDao.Update(ticket);

                _dao.Update("SET IsValid=0 WHERE IsGeneral=0 AND ProductID=@0", ticket.ProductId);

                _mapDao.Update("SET RuleId=@1 WHERE to_days(CurrentDate)>=to_days(now()) AND ProductID=@0", ticket.ProductId, generalRule.Id);
            }
            else if (vModel.PriceMode == 2 && ticket.PriceMode == 1)
            {
                ticket.PriceMode = 2;
                // 日期模式变为固定价格
                // 其他价格失效  应用这个价格的 表更新成标准价
                _productDao.Update(ticket);
            }


            #endregion
        }

        #endregion General

        /// <summary>
        /// 新增其他报价
        /// </summary>
        /// <param name="vModel"></param>
        public void AddOther(OtherPriceVModel vModel, CrmAccountModel userInfo)
        {
            var time = DateTime.Now;
            var priceRule = vModel.PriceRule;
            var priceList = vModel.PriceList;

            priceRule.IsGeneral = 0;
            priceRule.IsValid = 1;
            var standard = priceList.SingleOrDefault(p => p.IsStandard == 1);
            if (null == standard)
                throw new NullReferenceException("不存在标准价。");
            //var generalRule = GetModel(vModel.PriceRule.Id);
            priceRule.PriceType = standard.PriceType;
            priceRule.MarketPrice = standard.MarketPrice;
            priceRule.SettlePrice = standard.SettlePrice;
            priceRule.SysPrice = standard.SysPrice;
            priceRule.TktType = vModel.TkcketProduct.TktType;

            foreach (var price in priceList)
            {
                price.TktType = vModel.TkcketProduct.TktType;
                price.ModifiedBy = userInfo.Code;
                price.ModifiedTime = time;
                price.OwnerCode = userInfo.OwnerCode;
            }

            using (var scope = new TransactionScope())
            {
                var ruleId = Convert.ToInt32(_dao.Insert(priceRule));
                foreach (var price in priceList)
                {
                    price.RuleId = ruleId;
                    _priceDao.Insert(price);
                }

                scope.Complete();
            }
        }

        /// <summary>
        /// 删除其他报价规则
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="ruleId"></param>
        /// <param name="year"></param>
        public void DeleteOtherRule(string productId, int ruleId)
        {
            /*
             * 操作内容：
             * 1.将该报价规则所关联的map表重新与常规报价规则关联
             * 2.删除该报价规则对应的报价
             * 3.删除该报价规则
             */
            var generalRule = GetModel(productId, 1);
            var generalRuleId = generalRule.Id;
            //var otherPrices = GetPriceList(ruleId);
            var maps = GetMaps(productId, ruleId);
            using (var scope = new TransactionScope())
            {
                foreach (var map in maps)
                {
                    map.RuleId = generalRuleId;   // 替换为常规价格
                    _mapDao.Update(map);
                }
                //foreach (var price in otherPrices)
                //{
                //    _priceDao.Delete(price);
                //}
                _dao.Update("SET IsValid=0 WHERE ID=@0 ", ruleId);

                scope.Complete();
            }
        }

        public void RestoreOtherRule(string productId, int ruleId)
        {
            _dao.Update("SET IsValid=1 WHERE ID=@0 ", ruleId);
        }

        /// <summary>
        /// 更新其他报价
        /// </summary>
        /// <param name="vModel"></param>
        public void UpdateOtherPrice(OtherPriceVModel vModel, CrmAccountModel userInfo)
        {
            /*
             * 对于规则的更新，仅仅能够修改以下内容：
             * 1.规则的名称与标准价
             * 2.价格内容（价格类型无法修改）
             */
            var priceList = vModel.PriceList;
            var standard = priceList.Find(p => p.IsStandard == 1);
            if (standard == null)
                throw new NullReferenceException("无标准价。");
            var rule = GetModel(vModel.PriceRule.Id);
            rule.RuleName = vModel.PriceRule.RuleName;
            rule.MarketPrice = standard.MarketPrice;
            rule.SettlePrice = standard.SettlePrice;
            rule.SysPrice = standard.SysPrice;
            //rule.TktType = vModel.TkcketProduct.TktType;//无需更新

            var dbPriceList = GetPriceList(rule.Id);
            var time = DateTime.Now;
            foreach (var price in dbPriceList)
            {
                var viewPrice = priceList.SingleOrDefault(p => p.Id == price.Id);
                if (null == viewPrice) continue;
                price.MarketPrice = viewPrice.MarketPrice;
                price.SettlePrice = viewPrice.SettlePrice;
                price.SysPrice = viewPrice.SysPrice;
                price.PriceDesc = viewPrice.PriceDesc;
                price.IsValid = viewPrice.IsValid;
                price.IsStandard = viewPrice.IsStandard;
                price.ModifiedBy = userInfo.Code;
                price.ModifiedTime = time;
            }

            using (var scope = new TransactionScope())
            {
                _dao.Update(rule);
                foreach (var price in dbPriceList)
                {
                    _priceDao.Update(price);
                }
                scope.Complete();
            }
        }

        public void BatchPrice(BatchPriceVModel model)
        {
            var d = model.DateRange.Split('-');
            var start = d[0].ToDateTime();
            var end = d[1].ToDateTime();
            var days = GetDays(start, end, model.SelectedDays);
            var maps = GetMaps(model.ProductID, days);  // 已经包含的日期

            using (var scope = new TransactionScope())
            {
                if (model.Operation == 1)
                {
                    foreach (var map in maps)
                    {
                        if (model.RuleID != 0)   // 不调整价格
                            map.RuleId = model.RuleID;
                        if (model.PlanQuota != 0)
                            map.PlanQuota = model.PlanQuota;
                        _mapDao.Update(map);
                    }

                    // 以前不含
                    foreach (var item in days.Except(maps.Select(t => t.CurrentDate)))
                    {
                        _mapDao.Insert(new TktRulePriceMapModel
                        {
                            CurrentDate = item.Date,
                            PlanQuota = model.PlanQuota,
                            ProductId = model.ProductID,
                            RuleId = model.RuleID
                        });
                    }

                }
                else   // 批量清除
                {

                    foreach (var map in maps)
                    {
                        _mapDao.Delete(map);
                    }
                }

                scope.Complete();
            }
        }

        private List<DateTime> GetDays(DateTime start, DateTime end, string weeks)
        {
            var result = new List<DateTime>();
            List<string> weekList = null;
            if (!weeks.IsNullOrEmpty())
            {
                weekList = weeks.Split(',').ToList();
            }
            while (start <= end)
            {
                if (weekList == null || weekList.Contains(start.DayOfWeek.ToString()))
                {
                    result.Add(start);
                }
                start = start.AddDays(1);
            }
            return result;
        }

        #endregion PriceRule

        #region RulePriceMap

        public List<TktRulePriceMapModel> GetMaps(string productId, List<DateTime> days)
        {
            var sql = new Sql();
            sql.Append(@"SELECT * FROM TktRulePriceMap WHERE ProductId = @0", productId);
            if (days.Count > 0)
            {
                sql.Append(@" AND CurrentDate IN ( @0 ", days[0]);
                foreach (DateTime day in days.Where(p => p != days[0]))
                {
                    sql.Append(@", @0 ", day);
                }
                sql.Append(@" )");
            }
            return _mapDao.Fetch(sql.SQL, sql.Arguments);
        }

        public List<TktRulePriceMapModel> GetMaps(string productId, int ruleId)
        {
            return _mapDao.Fetch(@"SELECT * FROM TktRulePriceMap WHERE ProductId = @0 AND RuleId = @1", productId, ruleId);
        }

        public List<TktRulePriceMapModel> GetMaps(string productId, string start , string end)
        {

            var sql = new Sql();
            sql.Append(@"SELECT * FROM TktRulePriceMap WHERE ProductId = @0", productId);
            DateTime start1, end1;
            if (!string.IsNullOrEmpty(start) && DateTime.TryParse(start, out start1))
                sql.Append(" AND CurrentDate>=@0 ", start1);
            if (!string.IsNullOrEmpty(end) && DateTime.TryParse(end, out end1))
                sql.Append(" AND CurrentDate<=@0 ", end1);

            return _mapDao.Fetch(sql.SQL, sql.Arguments);

        }

        #endregion RulePriceMap
    }
}