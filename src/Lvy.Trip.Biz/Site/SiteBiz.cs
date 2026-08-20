using Lvy.Models.BaseDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Dao.Base;
using Lvy.Trip.Dao.Order;
using Lvy.VModels.Online;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Site
{
    /// <summary>
    /// 一些公用的模块
    /// </summary>
    public class SiteBiz : BaseBiz
    {
        /// <summary>
        /// 点击次数更新
        /// </summary>
        /// <param name="id">标签编号</param>
        /// <returns></returns>
        public int UpdateClickCnt(int id)
        {
            return new BaseTagDao().UpdateClickCnt(id);
        }

        /// <summary>
        /// 获取点击数量最高的标签集合
        /// </summary>
        /// <returns></returns>
        public List<BaseTagModel> GetClickCntTopTags(int type, int size, string ownerCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT * FROM BaseTag WHERE ProductType=@0 AND OwnerCode=@1 ", type, ownerCode);
            sql.Append("ORDER BY ClickCnt DESC LIMIT {0} ".With(size));

            return new BaseTagDao().Fetch(sql.SQL, sql.Arguments);
        }

        public List<OrderFlowVModel> GetOrderFlow(string ownerCode, int size = 6)
        {
            TpOrderDao dao = new TpOrderDao();
            string sql = "SELECT t.Id AS OrderId, t.TourId, l.LineName, t.OutDate, t.CreatedTime FROM TpOrder t INNER JOIN TpLine l ON t.LineId=l.LineId WHERE t.OwnerCode=@0 ORDER BY t.CreatedTime DESC LIMIT {0} ".With(size);
            return dao.Query<OrderFlowVModel>(sql, ownerCode).ToList();
        }

        /// <summary>
        /// 获取名片对象
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public BusinessCardVModel GetBusinessCard(string lineId)
        {
            var line = new TpLineBiz().GetLineById(lineId);
            if (line == null)
                return null;

            var lineAdmins = new TpLineAdminBiz().GetByLineId(lineId);
            if (lineAdmins == null || lineAdmins.Count == 0)
                return null;

            // 专线批发商
            var vModel = new BusinessCardVModel();
            var customerAdmins = lineAdmins.Where(p => p.Department == 0).ToList();
            if (customerAdmins.Count == 0)
            {
                vModel.CustomerAccount = null;
                vModel.CustomerAdmin = null;
            }
            else
            {
                vModel.CustomerAdmin = customerAdmins.FirstOrDefault(p => p.IsPrimary == 1) ?? customerAdmins[0];
                vModel.CustomerAccount = new AccountBiz().GetAccountCustomer(vModel.CustomerAdmin.AccountCode);
            }

            // 平台供应商
            var platAdmins = lineAdmins.Where(p => p.Department == 1).ToList();
            if (platAdmins.Count == 0)
            {
                vModel.PlatAccount = null;
                vModel.PlatAdmin = null;
            }
            else
            {
                vModel.PlatAdmin = platAdmins.FirstOrDefault(p => p.IsPrimary == 1) ?? platAdmins[0];
                vModel.PlatAccount = new AccountBiz().GetAccountCustomer(vModel.PlatAdmin.AccountCode);
            }
            return vModel;
        }
    }
}