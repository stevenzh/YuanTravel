using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Models.TicketDB;
using Lvy.Trip.Dao.Ticket;
using Lvy.VModels.Ticket;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Lvy.Trip.Biz.Ticket
{
    /// <summary>
    /// 门票产品管理
    /// </summary>
    public class TktProductBiz : BaseBiz
    {
        private readonly TktProductDao _dao = new TktProductDao();
        private readonly TktAdminDao adminDao = new TktAdminDao();
        private readonly TktPriceRuleDao ruleDao = new TktPriceRuleDao();
        private readonly TktPriceDao priceDao = new TktPriceDao();
        private readonly TktRulePriceMapDao mapDao = new TktRulePriceMapDao();
        private readonly TktCategoryDao categoryDao = new TktCategoryDao();
        private readonly TktFileDao _fileDao = new TktFileDao();

        #region Base

        /// <summary>
        /// 根据Id获取TktProductModel
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public TktProductModel GetById(string productId)
        {
            return _dao.GetByProductId(productId);
        }

        /// <summary>
        /// 根据ProductName获取TktProductModel
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public TktProductModel GetByName(string productName, string ownerCode)
        {
            if (productName.IsNullOrEmpty())
                throw new Exception("productName is null.");
            return _dao.FirstOrDefault(@"SELECT * FROM TktProduct WHERE ProductName = @0 AND OwnerCode = @1", Ansi(productName), Ansi(ownerCode));
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(TktProductModel model, CrmAccountModel currentUser)
        {
            model.ModifiedBy = currentUser.Code;
            model.ModifiedTime = DateTime.Now;
            return _dao.Update(model);
        }

        #endregion Base

        #region Common

        /// <summary>
        /// 所有上线的门票产品
        /// </summary>
        /// <returns></returns>
        public List<TktProductModel> GetProducts(string ownerCode)
        {
            return _dao.Fetch(@"SELECT * FROM TktProduct WHERE ProductState=@0 AND OwnerCode = @1 ORDER BY PinYin", 3, Ansi(ownerCode));
        }

        #endregion Common

        #region Search

        /// <summary>
        /// 获取门票查询页 分页对象
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<TktProductModel> GetPagedTicket(SearchTicketVModel vModel, string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@"SELECT A.*,bp.PlaceName, B.PlanQuota FROM TktProduct A 
LEFT JOIN BasePlace bp ON bp.PlaceCode=A.PlaceCode 
LEFT JOIN TktQuota B ON B.ProductId = A.ProductId
WHERE A.ProductState!=0 AND A.OwnerCode=@0", ownerCode);

            if (!vModel.IsImport.IsNullOrEmpty())
                sql.Append(@" AND A.IsImport=@0", (vModel.IsImport == "1" ? true : false));
            if (!vModel.ProductID.IsNullOrEmpty())
                sql.Append(@" AND A.ProductID = @0", Ansi(vModel.ProductID));
            if (!vModel.ProductName.IsNullOrEmpty())
                sql.Append(@" AND A.ProductName LIKE @0", AnsiLike(vModel.ProductName));
            if (!string.IsNullOrEmpty(vModel.ProductType))  // 大类
                sql.Append(@" AND A.ProductType = @0", vModel.ProductType);
            if (vModel.ProductState != 0)                      // 上线状态
                sql.Append(@" AND A.ProductState = @0", vModel.ProductState);
            if (!vModel.ArriveDest.IsNullOrEmpty())          // 目的地
                sql.Append(@" AND A.ArriveDest = @0", Ansi(vModel.ArriveDest));
            if (!vModel.TeamID.IsNullOrEmpty())              // 部门
                sql.Append(@" AND A.TeamID = @0", Ansi(vModel.TeamID));
            if (!string.IsNullOrEmpty(vModel.ProductCategory))        // 少用
                sql.Append(@" AND A.ProductCategory = @0", vModel.ProductCategory);
            sql.Append(@" ORDER BY A.ModifiedTime DESC");

            var pagedModel = _dao.Pager<TktProductModel>(vModel.PagedTickets.PageIndex, vModel.PagedTickets.PageSize, sql.SQL, sql.Arguments);

            /*
             * 碍于分页跨表查询限制，暂时作如下处理：
             * 1.在查询出门票数据；
             * 2.遍历门票，通过一次请求将相关专管员信息取出；
             * 3.将专管员与门票匹配
            */
            if (pagedModel.Items != null && pagedModel.Items.Count > 0)
            {
                var sql4Admin = new Sql();
                var tickets = pagedModel.Items;
                sql4Admin.Append(@"SELECT * FROM TktAdmin WHERE ProductId IN (@0)", tickets.Select(t => t.ProductId).ToArray());
                var admins = adminDao.Fetch(sql4Admin.SQL, sql4Admin.Arguments);
                if (admins != null && admins.Count > 0)
                {
                    pagedModel.Items.ForEach(p => p.Admins = admins.FindAll(m => m.ProductId == p.ProductId));
                }
            }

            return pagedModel;
        }

        #endregion Search

        #region 添加门票

        /// <summary>
        /// 新增门票
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public string AddTicket(EditTicketVModel vModel, CrmAccountModel currentUser)
        {
            var time = DateTime.Now;
            var ticket = RenderNewTicket(vModel, time, currentUser);
            var admin = new TktAdminModel() { AccountCode = currentUser.Code };

            using (var scope = new TransactionScope())
            {
                _dao.Insert(ticket);
                admin.ProductId = ticket.ProductId;
                adminDao.Insert(admin);
                scope.Complete();
            }
            return ticket.ProductId;
        }

        #endregion Add

        #region Add Copy Common

        private TktProductModel RenderNewTicket(EditTicketVModel vModel, DateTime time, CrmAccountModel currentUser)
        {
            var ticket = vModel.TicketProduct;
            ticket.Id = 0;

            #region 赋予门票默认值

            ticket.ProductId = "T" + DBTools.GetTicketSeqNo();
            //ticket.PinYin = ticket.ProductName.ConvertPinYin();
            //ticket.JPinYin = ticket.PinYin.IsNullOrEmpty() ? "" : ticket.PinYin.ConvertJPinYin();
            ticket.ProductState = 2;
            ticket.CreatedBy = currentUser.Code;
            ticket.CreatedTime = time;
            ticket.ModifiedBy = currentUser.Code;
            ticket.ModifiedTime = time;
            ticket.OwnerCode = currentUser.OwnerCode;

            //vModel.TicketProduct.LastDate = Request.Form["TicketProduct.LastDate"].ToDateTime();
            if (!string.IsNullOrEmpty(vModel.OutDateRange))
            {
                var t = vModel.OutDateRange.Split('-');
                ticket.StartTime = t[0].ToDateTime();
                ticket.EndTime = t[1].ToDateTime();
            }

            #endregion 赋予门票默认值

            return ticket;
        }


        #endregion Add Copy Common

        #region 复制门票

        public string CopyTicket(EditTicketVModel vModel, CrmAccountModel currentUser)
        {
            var productId = vModel.TicketProduct.ProductId;
            var time = DateTime.Now;
            var ticket = RenderNewTicket(vModel, time, currentUser);

            var ruleBiz = new TktPriceRuleBiz();
            var rules = ruleBiz.GetModels(productId);
            var priceList = ruleBiz.GetPriceListByProduct(productId);
            ///var maps = ruleBiz.GetMaps(productId);
            var admin = new TktAdminModel() { AccountCode = currentUser.Code };

            using (var scope = new TransactionScope())
            {
                _dao.Insert(ticket);
                foreach (var rule in rules)
                {
                    var oldRuleId = rule.Id;
                    rule.Id = 0;
                    rule.ProductId = ticket.ProductId;
                    int ruleId = Convert.ToInt32(ruleDao.Insert(rule));

                    //var insertMaps = maps.FindAll(p => p.RuleId == oldRuleId);
                    //foreach (var map in insertMaps)
                    //{
                    //    map.ProductId = ticket.ProductId;
                    //    map.RuleId = ruleId;
                    //    map.Id = 0;
                    //    mapDao.Insert(map);
                    //}
                    var insertPriceList = priceList.FindAll(p => p.RuleId == oldRuleId);
                    foreach (var price in insertPriceList)
                    {
                        price.RuleId = ruleId;
                        price.TktType = ticket.TktType;
                        price.Id = 0;
                        priceDao.Insert(price);
                    }
                }
                admin.ProductId = ticket.ProductId;
                adminDao.Insert(admin);
                scope.Complete();
            }
            return ticket.ProductId;
        }

        #endregion Copy

        #region 编辑门票

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="vModel"></param>
        public void SaveEdit(EditTicketVModel vModel)
        {
            #region Ticket

            var entity = GetById(vModel.TicketProduct.ProductId);
            var changeTktType = entity.TktType == vModel.TicketProduct.TktType;
            entity.ProductName = vModel.TicketProduct.ProductName;
            entity.ArriveDest = vModel.TicketProduct.ArriveDest;
            entity.TktType = vModel.TicketProduct.TktType;
            entity.Themes = vModel.Themes.Join("|");
            entity.SupplierCode = vModel.TicketProduct.SupplierCode;
            entity.TuiJianType = vModel.TicketProduct.TuiJianType;
            entity.ProductType = vModel.TicketProduct.ProductType;
            entity.ProductCategory = vModel.TicketProduct.ProductCategory;
            entity.BookingDesc = vModel.TicketProduct.BookingDesc;
            entity.ProductDesc = vModel.TicketProduct.ProductDesc;
            entity.ProductState = 2;    //Default: set offline after edit.
            entity.ImgUrl = vModel.TicketProduct.ImgUrl;
            entity.PlaceCode = vModel.TicketProduct.PlaceCode;
            entity.TeamID = vModel.TicketProduct.TeamID;
            entity.PreDays = vModel.TicketProduct.PreDays;
            entity.PreTime = vModel.TicketProduct.PreTime;

            if (entity.TuiJianType == 2)  // 限制库存模式
            {
                entity.PlanQuota = vModel.TicketProduct.PlanQuota;
                entity.HoldQuota = vModel.TicketProduct.HoldQuota;
                //entity.LastDate = vModel.TicketProduct.LastDate;
                entity.LimitQuota = vModel.TicketProduct.LimitQuota;
                //entity.BeginBuyTime = vModel.TicketProduct.BeginBuyTime;
            }

            //vModel.TicketProduct.LastDate = Request.Form["TicketProduct.LastDate"].ToDateTime();
            if (!string.IsNullOrEmpty(vModel.OutDateRange))   // 商品使用期限
            {
                var t = vModel.OutDateRange.Split('-');
                entity.StartTime = t[0].ToDateTime();
                entity.EndTime = t[1].ToDateTime();
            }
            else
            {
                entity.StartTime = null;
                entity.EndTime = null;
            }
            if (!string.IsNullOrEmpty(vModel.BookingRange))   // 商品使用期限
            {
                var t = vModel.BookingRange.Split('-');
                entity.BeginBuyTime = t[0].ToDateTime();
                entity.LastDate = t[1].ToDateTime();
            }
            else
            {
                entity.BeginBuyTime = null;
                entity.LastDate = null;
            }

            #endregion Ticket

            List<TktPriceRuleModel> rules = null;
            List<TktPriceModel> prices = null;
            if (!changeTktType)  // 库存模式改变
            {
                var ruleBiz = new TktPriceRuleBiz();
                rules = ruleBiz.GetModels(entity.ProductId);  // 取得  时间段规则

                prices = new List<TktPriceModel>();
                foreach (var item in rules)
                {
                    item.TktType = entity.TktType;
                    var pricesPer = ruleBiz.GetPriceList(item.Id);
                    pricesPer.ForEach(p => p.TktType = entity.TktType);   // 每一项都重新赋值
                    prices.AddRange(pricesPer);
                }
            }

            using (var scope = new TransactionScope())
            {
                _dao.Update(entity);

                if (rules != null)
                    rules.ForEach(p => ruleDao.Update(p));
                if (prices != null && prices.Count > 0)
                    prices.ForEach(p => priceDao.Update(p));

                scope.Complete();
            }
        }

        #endregion Edit

        #region File

        public List<TktFileModel> GetFileList(string productID)
        {
            return _fileDao.Fetch("SELECT * FROM tkt_files WHERE IsValid=1 AND ProductID=@0 ", productID);
        }

        public TktFileModel GetTktFileModel(int id)
        {
            return _fileDao.GetById(id);
        }


        public int SetPrimaryPic(string productID, string filePath)
        {
            return _dao.Update("SET ImgUrl=@1 WHERE ProductID=@0 ", productID, filePath);
        }

        public void AddPhoto(TktFileModel model)
        {
            _fileDao.Insert(model);
        }

        public void DeleteFile(int id)
        {
            _fileDao.Update("SET IsValid=0 WHERE FileID=@0", id);
        }


        #endregion

        #region Category


        public TktCategoryModel GetCategoryByID(int id)
        {
            return categoryDao.GetById(id);
        }

        public void SaveCategory(TktCategoryModel model)
        {
            if (model.ID == default(int))
                categoryDao.Insert(model);
            else
                categoryDao.Update(model);
        }

        public List<TktCategoryModel> GetCategoryByType(string productType)
        {
            return categoryDao.Fetch("SELECT * FROM tkt_category WHERE ProductType=@0 ", productType);
        }

        #endregion
    }
}