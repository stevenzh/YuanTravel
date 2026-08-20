using Arch.Common;
using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.TicketDB;
using Lvy.Trip.Dao.Base;
using Lvy.Trip.Dao.Ticket;
using Lvy.VModels.Ticket;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Ticket
{
    public class TktOnlineBiz : BaseBiz
    {
        TktProductDao _dao = new TktProductDao();

        /// <summary>
        /// 根据条件查询产品
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<TktProductVModel> GetProducts(SearchVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT a.Id AS ProductId, a.*,
 b.MarketPrice ,b.SettlePrice, b.SysPrice, 
 c.PlanQuota, c.HoldQuota, c.UsedQuota, c.LastDate, c.BeginBuyTime 
FROM TktProduct a LEFT JOIN TktPriceRule b ON a.ProductId=b.ProductId
LEFT JOIN TktQuota c ON a.ProductId=c.ProductId")
                .Append(" WHERE a.OwnerCode=@0 AND a.ProductState=3", vModel.OwnerCode)
                .Append(" AND b.Year=@0 ", DateTime.Now.Year)
                //---------根据日期来获取相应的标准价 modify by yuan 2013.4.24-------------------
                .Append(" AND b.Id IN (select RuleId from TktRulePriceMap where CurrentDate=@0) ", DateTime.Today);
            //-----------------------------------------------------

            if (!vModel.DestId.IsNullOrEmpty())  // 目的地
                sql.Append(" AND a.ArriveDest=@0", vModel.DestId);
            if (!vModel.ProductId.IsNullOrEmpty()) // 产品ID
                sql.Append(" AND a.Id=@0", vModel.ProductId);
            //if (!vModel.ProductId.IsNullOrEmpty()) // 产品名称
            //    sql.Append(" and a.ProductName like @0", AnsiLike(vModel.ProductId));
            sql.Append(" ORDER BY a.ModifiedTime DESC");

            var list = _dao.Pager<TktProductVModel>(vModel.Products.PageIndex, 50, sql.SQL, sql.Arguments);
            return list;
        }


        /// <summary>
        /// 获取推荐数据
        /// </summary>
        /// <returns></returns>
        public List<TktProductVModel> GetHotTickets(string code, string ownerCode)
        {
            string sql = @"SELECT a.*, b.* FROM TktProduct a
INNER JOIN site_nav_list snl ON snl.ProductID=a.ProductId 
INNER JOIN site_nav_items sni ON sni.ItemID=snl.ItemID
LEFT JOIN TktPriceRule b ON b.ID=(
 SELECT tp.ID FROM tktpricerule tp
 WHERE tp.ProductID = a.ProductId AND tp.IsGeneral=1 AND tp.IsValid=1
 LIMIT 1 )
    WHERE a.OwnerCode=@0 AND a.ProductState=3 AND sni.Code=@1
    ORDER BY snl.SortOrder ";

            return _dao.Query<TktProductVModel>(sql, ownerCode, code).ToList();
        }

        /// <summary>
        /// 获取景区文件
        /// </summary>
        /// <returns></returns>
        public List<BaseFileResModel> GetFiles(string ownerCode)
        {
            string sql = @"SELECT * FROM BaseFileRes WHERE IsValid=1 AND ResType=1 AND OwnerCode=@0 ORDER BY sort DESC LIMIT 8";

            BaseFileResDao dao = new BaseFileResDao();

            return dao.Fetch(sql, ownerCode);
        }

        /// <summary>
        /// 获取热门目的地
        /// </summary>
        /// <returns></returns>
        public List<KeyValueBean> GetHotDestBeans(string ownerCode)
        {
            string sql = @"SELECT * FROM (
                            SELECT ArriveDest, COUNT(1) AS Cnt FROM TktProduct WHERE ownercode=@0 GROUP BY ArriveDest
                            ) a ORDER BY Cnt DESC";

            var destIds = _dao.Query<string>(sql, ownerCode);

            var kvs = new List<KeyValueBean>();
            KeyValueBean kv = null;
            foreach (var destId in destIds)
            {
                kv = new KeyValueBean()
                {
                    Key = destId,
                    Value = DictionaryBiz.GetDestNameStr(destId)
                };
                kvs.Add(kv);
            }
            return kvs;
        }

        /// <summary>
        /// 显示报价明细
        /// </summary>
        /// <returns></returns>
        public List<TktPriceRuleModel> GetPrices(string productId)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT a.*, b.* 
FROM TktPriceRule a INNER JOIN TktPrice b ON a.Id=b.RuleId
 WHERE a.ProductId=@0 AND a.Year>=@1 AND b.IsValid=1", productId, DateTime.Now.Year);

            TktPriceRuleDao dao = new TktPriceRuleDao();
            return dao.Query<TktPriceRuleModel, TktPriceModel, TktPriceRuleModel>
                  (new TktPriceRuleToPriceRelator().MapIt, sql.SQL, sql.Arguments).ToList();
        }
    }
}