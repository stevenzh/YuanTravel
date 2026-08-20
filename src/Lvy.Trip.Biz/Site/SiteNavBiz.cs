using Lvy.Models;
using Lvy.Models.SiteDB;
using Lvy.Trip.Dao.Site;
using Lvy.Visa.Dao;
using Lvy.Visa.VModels;
using Lvy.VModels.Online;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Site
{
    /// <summary>
    /// 站点导航管理
    /// </summary>
    public class SiteNavBiz : BaseBiz
    {
        private readonly SiteNavDao _navDao = new SiteNavDao();
        private readonly SiteNavItemDao _dao = new SiteNavItemDao();
        private readonly SiteNavListDao _listDao = new SiteNavListDao();
        private readonly VisaProductDao _visaDao = new VisaProductDao();

        #region NAV

        public List<SiteNavModel> GetAllNavs()
        {
            string sql = "SELECT * FROM site_navs WHERE IsValid=1 ";
            return _navDao.Fetch(sql);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="qmodel"></param>
        /// <returns></returns>
        public PagedList<SiteNavModel> NavPageList(NavQModel qmodel)
        {
            var sql = new Sql();
            sql.Append("SELECT * FROM site_navs WHERE OwnerCode=@0 ", qmodel.OwnerCode);

            if (!string.IsNullOrEmpty(qmodel.Name))
                sql.Append("and Name like @0", AnsiLike(qmodel.Name));

            return _navDao.Pager(qmodel.PageList.PageIndex, qmodel.PageList.PageSize, sql.SQL, sql.Arguments);
        }

        public SiteNavModel GetNavByID(int id)
        {
            return _navDao.GetById(id);
        }

        public void SaveNav(SiteNavModel model)
        {
            if (model.NavID == default(int))
            {
                _navDao.Insert(model);
            }
            else
            {
                _navDao.Update(model);
            }
        }

        public void AddNav(SiteNavModel model)
        {
            _navDao.Insert(model);
        }

        #endregion NAV

        #region NavItem

        public List<SiteNavItemModel> GetChildNavList(string code, string city)
        {
            var sql = new Sql();
            sql.Append(" SELECT * FROM site_nav_items WHERE IsVaild=1 ");
            if (city != "ALL")
            {
                sql.Append(" and OutCity=@0", city);
            }
            if (!string.IsNullOrEmpty(code))
                sql.Append(" and `Level`>1 and Code Like '" + code + "%'");
            else
                sql.Append(" and `Level`=1 ");
            sql.Append(" Order By SortOrder ");

            return _dao.Query(sql.SQL, sql.Arguments).ToList();
        }

        public void DeleteHotVisa(int listId)
        {
            _listDao.Delete("WHERE ListID=@0 ", listId);
        }

        public IList<SiteNavItemModel> SearchModuleList(string ownerCode)
        {
            return _dao.Fetch(@"SELECT sni.* FROM site_nav_items sni 
INNER JOIN site_navs sn ON sn.Code=sni.NavCode 
  WHERE sni.IsGroup=1 AND sn.OwnerCode=@0 ", ownerCode);
        }

        public List<SiteNavItemModel> GetAllNavItems(string navCode)
        {
            var sql = new Sql();
            sql.Append(@" SELECT sni.*, bd.Value AS OutCityName FROM site_nav_items sni
 LEFT JOIN basedictionarydetail bd ON bd.`Key`= sni.OutCity AND bd.Name = 'OutCityEnum'
 WHERE sni.NavCode = @0 ORDER BY sni.SortOrder ", navCode);

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        public void SaveNavItem(SiteNavItemModel model)
        {
            if (model.ItemID == default(int))
            {
                _dao.Insert(model);
            }
            else
            {
                _dao.Update(model);
            }
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        public void AddNavItem(SiteNavItemModel model)
        {
            _dao.Insert(model);
        }

        public SiteNavItemModel GetNavItemByID(int itemID)
        {
            return _dao.GetById(itemID);
        }

        public List<SiteNavItemModel> GetLineDests(string code, string OwnerCode)
        {
            var sql = new Sql();
            sql.Append(" SELECT * FROM site_nav_items WHERE NavCode=@0 AND IsValid=1 ORDER BY SortOrder ", code);

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        public SiteNavItemModel GetNavItem(int itemId)
        {
            Sql sql = new Sql();
            sql.Append(" SELECT * FROM site_nav_items WHERE ItemID=@0 AND IsValid=1 AND NavCode=@1 ", itemId, "W001");

            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        public IList<SiteNavItemModel> SearchList(string code, string city, int itemId = 0)
        {
            Sql sql = new Sql();
            sql.Append(" SELECT * FROM site_nav_items WHERE OutCity=@0 AND IsValid=1 AND NavCode=@1 ", Ansi(city), Ansi(code));

            if (itemId == 0)
                sql.Append(" AND `Level`=1 ");
            else
                sql.Append(" AND `Level`>1 AND ParentID=@0", itemId);

            sql.Append(" ORDER BY SortOrder ");
            return _dao.Query<SiteNavItemModel>(sql.SQL, sql.Arguments).ToList();
        }

        public void AddProToHotVisa(HotProductQModel qmodel)
        {
            var proArr = qmodel.SelProCodes.Substring(0, qmodel.SelProCodes.Length - 1).Split(',');
            var count = 1;
            var visamodel = _listDao.Fetch("SELECT * FROM site_nav_list WHERE ItemID=@0 ORDER BY SortOrder DESC ", qmodel.ItemID).FirstOrDefault();
            if (visamodel != null)
                count = visamodel.SortOrder + 1;
            for (var i = 0; i < proArr.Length; i++)
            {
                var model = new SiteNavListModel();
                model.ItemID = qmodel.ItemID;
                model.ProductId = proArr[i];
                model.SortOrder = count;
                model.CreatedBy = qmodel.CreatedBy;
                model.CreatedTime = DateTime.Now;

                _listDao.Insert(model);

                count++;
            }
        }

        #endregion NavItem

        public void SaveNavList(int itemID, string[] listStr)
        {
            var rr = GetNavList(itemID).Select(m => m.ProductId).ToArray();

            // 添加没有的
            AddList(itemID, listStr.Except(rr).ToArray());
            // 删除废弃的
            DeleteList(itemID, rr.Except(listStr).ToArray());
        }

        public void SaveHotVisaSort(HotProductQModel qmodel)
        {
            foreach (var item in qmodel.HotProductList)
            {
                _listDao.Update("set SortOrder=@1 where ListID=@0 ", item.ListID, item.SortOrder);
            }
        }

        private void AddList(int accountCode, string[] selectedRoleIds)
        {
            if (selectedRoleIds == null || selectedRoleIds.Length <= 0)
                return;

            foreach (var selectedRoleId in selectedRoleIds)
            {
                var model = new SiteNavListModel()
                {
                    ItemID = accountCode,
                    ProductId = selectedRoleId,
                    SortOrder = 0
                };
                _listDao.Insert(model);
            }
        }

        private void DeleteList(int ItemID, string[] selectedRoleIds)
        {
            if (selectedRoleIds == null || selectedRoleIds.Length <= 0)
                return;

            _listDao.Execute("DELETE FROM site_nav_list WHERE ItemID=@0 AND ProductID IN (@1)", ItemID, selectedRoleIds);
        }

        public string CheckProIsExist(HotProductQModel qmodel)
        {
            Sql sql = new Sql();
            sql.Append("select distinct ProductID from site_nav_list where ItemID=@0 ", qmodel.ItemID);
            sql.Append(" and ProductID in ( @0 )", qmodel.SelProCodes.Split(','));
            return string.Join(",", _dao.Query<string>(sql.SQL, sql.Arguments));
        }

        public void SetValidStateByDest(int id)
        {
            throw new NotImplementedException();
        }

        public List<SiteNavListModel> GetNavList(int itemId)
        {
            return _listDao.Fetch("select * from site_nav_list WHERE ItemId =@0 ", itemId);
        }

        /// <summary>
        /// 取得已关联列表
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public List<SiteNavListModel> GetVisaList(int itemId)
        {
            return _listDao.Fetch(@"select snl.*, b.InformationName ProductName,
b.SellPrice SalePrice, bdd.Value VTypeValue, bdd4.Value VisaAreaValue
FROM Visa_Information b INNER JOIN site_nav_list snl ON b.InformationCode = snl.ProductID
  INNER JOIN site_nav_items sni ON sni.ItemID = snl.ItemID
  inner join BaseDictionaryDetail bdd on b.VType=bdd.`Key` and bdd.Name='VisaVTypeEnum'
  inner join BaseDictionaryDetail bdd4 on b.VisaArea=bdd4.`Key` and bdd4.Name='VisaAreaEnum'
WHERE b.IsValid=1 AND sni.ItemID=@0
ORDER BY snl.SortOrder", itemId);
        }

        /// <summary>
        /// 取得已关联列表
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public List<SiteNavListModel> GetLineList(int itemId)
        {
            return _listDao.Fetch(@"select snl.*, tl.LineName ProductName, bdd.Value OurCityName
FROM TpLine tl INNER JOIN site_nav_list snl ON tl.LineID = snl.ProductID
  INNER JOIN site_nav_items sni ON sni.ItemID = snl.ItemID
  inner join BaseDictionaryDetail bdd on tl.DepartDest=bdd.`Key` and bdd.Name='OutCityEnum'
WHERE sni.ItemID=@0
ORDER BY snl.SortOrder", itemId);
        }

        /// <summary>
        /// 取得已关联列表
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public List<SiteNavListModel> GetTicketList(int itemId)
        {
            return _listDao.Fetch(@"select snl.*, tp.ProductName, bp.PlaceName
FROM TktProduct tp INNER JOIN site_nav_list snl ON tp.ProductID = snl.ProductID
  INNER JOIN site_nav_items sni ON sni.ItemID = snl.ItemID
  INNER JOIN BasePlace bp on tp.PlaceCode=bp.PlaceCode
WHERE sni.ItemID=@0
ORDER BY snl.SortOrder", itemId);
        }

        /// <summary>
        /// 取得已关联列表 （酒店）
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public List<SiteNavListModel> GetHotelList(int itemId)
        {
            return _listDao.Fetch(@"select snl.*, tp.HotelName AS ProductName
FROM hotels tp INNER JOIN site_nav_list snl ON tp.HotelCode = snl.ProductID
  INNER JOIN site_nav_items sni ON sni.ItemID = snl.ItemID
WHERE sni.ItemID=@0
ORDER BY snl.SortOrder", itemId);
        }


    }
}