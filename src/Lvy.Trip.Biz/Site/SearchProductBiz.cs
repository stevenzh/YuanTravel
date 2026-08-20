using Lvy.Models;
using Lvy.Models.HotelDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Dao;
using Lvy.Trip.Dao.Product;
using Lvy.Visa.Dao;
using Lvy.Visa.Models;
using Lvy.Visa.VModels;
using Lvy.VModels.Online;
using Lvy.VModels.Product;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Site
{
    /// <summary>
    /// 前台产品查询（线路、签证、酒店）
    /// </summary>
    public class SearchProductBiz : BaseBiz
    {
        private readonly TpTourPlanDao _planDao = new TpTourPlanDao();
        private readonly TpLineDao _lineDao = new TpLineDao();
        private readonly VisaProductDao _visaDao = new VisaProductDao();
        private readonly HotelDao _hotelDao = new HotelDao();

        #region 旅游线路

        public PagedList<TourInfoVModel> GetProducts(SearchProductVModel vModel, string ownerCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT tl.LineId, tl.LineSpecial, tl.TrafficType, tl.Themes, tl.CustomerCode, tl.CustomerName, tl.LogoPath, ");
            sql.Append(@"tp.Id AS TourId, tl.LineName, tp.OutDate, tp.TourNo, tp.Price, tq.PlanQuota,tq.UseQuota,tq.UsedQuota,");
            sql.Append(@"tp.SingleRoom, tpp.TeJiaFanLi");
            sql.Append(@" FROM TpLine tl INNER JOIN TpTourPlan tp ON tp.LineId=tl.LineId");
            sql.Append(@" INNER JOIN TpTourQuotaMap ON TpTourQuotaMap.TourId=tp.Id");
            sql.Append(@" INNER JOIN TpQuota tq ON tq.Id=TpTourQuotaMap.QuotaId");
            sql.Append(@" INNER JOIN TpPrice tpp ON tpp.TourId=tp.Id AND tpp.IsStandard=1 And tpp.IsValid=1 ");
            sql.Append(@" WHERE tl.OwnerCode=@0", Ansi(ownerCode));
            sql.Append(@" AND tl.IsValid=1");               //有效
            sql.Append(@" AND tl.LineState=3");             //上线
            sql.Append(@" AND tp.TourState=3");             //上线（团计划）
            sql.Append(@" AND tp.BookingLastDays>=@0", DateTime.Today);

            if (vModel.LineType > 0)                       // 线路类型
                sql.Append(@" AND tl.LineType=@0", vModel.LineType);

            if (!vModel.ArriveDest.IsNullOrEmpty())
                sql.Append(@" AND LEFT(tl.ArriveDest, @1)=@0", vModel.ArriveDest, vModel.ArriveDest.Length);

            // 出发日期
            if (!vModel.MinOutDate.IsNullOrEmpty())
                sql.Append(@" AND tp.OutDate>=@0", vModel.MinOutDate.ToDateTime());
            if (!vModel.MaxOutDate.IsNullOrEmpty())
                sql.Append(@" AND tp.OutDate<=@0", vModel.MaxOutDate.ToDateTime());

            if (!vModel.LineName.IsNullOrEmpty())
                sql.Append(@" AND tl.LineName LIKE @0", AnsiLike(vModel.LineName));

            //主题标签
            if (!vModel.Themes.IsNullOrEmpty())
            {
                sql.Append(@" AND tl.Themes LIKE @0", AnsiLike(vModel.Themes));
            }
            //行程天数
            if (vModel.TravelDays > 0)
            {
                if (vModel.TravelDays < 7)
                    sql.Append(@" AND tl.TravelDays = @0", vModel.TravelDays);
                else
                    sql.Append(@" AND tl.TravelDays >= @0", vModel.TravelDays);
            }
            // 排序方式
            if (!vModel.OrderBy.IsNullOrEmpty())
            {
                sql.Append(@" ORDER BY " + vModel.OrderOption.FirstOrDefault(p => p.Key == vModel.OrderBy).Value.Key);
                sql.Append(@" , tp.OutDate");
            }
            else
            {
                sql.Append(@" ORDER BY tp.OutDate");
            }

            var pagedList = _planDao.Pager<TourInfoVModel>(vModel.ProductPagedList.PageIndex, vModel.ProductPagedList.PageSize, sql.SQL, sql.Arguments);

            return pagedList;
        }

        /// <summary>
        /// 外网首页产品列表
        /// </summary>
        /// <param name="ItemCode">推荐板块ID</param>
        /// <returns></returns>
        public List<HotTourVModel> GetHotTours(string ItemCode, string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@"SELECT TpLine.LineId, TpLine.LineType,TpLine.LineName,TpLine.LogoPath,
    TpTourPlan.OutDate,TpTourPlan.Price,TpTourPlan.TourNo,TpTourPlan.SingleRoom,
    TpTourQuotaMap.TourId,TpTourQuotaMap.QuotaId,
    TpQuota.PlanQuota,TpQuota.UseQuota,TpQuota.UsedQuota,TpQuota.HoldQuota,
    TpPrice.TeJiaFanLi ");
            sql.Append(@" FROM TpLine INNER JOIN TpTourPlan ON TpTourPlan.lineId=TpLine.LineId");
            sql.Append(@" INNER JOIN site_nav_list snl ON snl.ProductId=TpLine.LineId");
            sql.Append(@" INNER JOIN site_nav_items snt ON snl.ItemId=snt.ItemID");
            sql.Append(@" INNER JOIN TpTourQuotaMap ON TpTourQuotaMap.TourId=TpTourPlan.Id");
            sql.Append(@" INNER JOIN TpQuota ON TpQuota.Id=TpTourQuotaMap.QuotaId");
            sql.Append(@" INNER JOIN TpPrice ON TpPrice.TourId=TpTourPlan.Id AND TpPrice.IsStandard=1");
            sql.Append(@" WHERE TpLine.OwnerCode=@0", ownerCode);
            sql.Append(" AND snt.Code=@0 ", ItemCode);
            sql.Append(" AND TpQuota.UseQuota>0 AND TpTourPlan.BookingLastDays>=@0", DateTime.Today);   // 有余位 且可以预定
            sql.Append(" AND TpLine.LineState=@0", 3);        // 产品上线
            sql.Append(" AND TpLine.IsValid=1");              // 产品有效
            sql.Append(" AND TpTourPlan.TourState=@0", 3);    // 团计划上线

            List<HotTourVModel> hotTours = _planDao.Query<HotTourVModel>(sql.SQL, sql.Arguments).ToList();

            return hotTours.GroupBy(t => t.LineId).Select(t => t.First()).ToList();
        }

        public List<HotelModel> GetHotHotels(string ItemCode, string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@" SELECT h.*, bd.Name AS CityName
FROM hotels h 
LEFT JOIN basedestination bd ON h.CityCode=bd.Id 
INNER JOIN site_nav_list snl ON snl.ProductId=h.HotelCode
INNER JOIN site_nav_items snt ON snl.ItemId=snt.ItemID
WHERE h.HotelState=3 ");
            sql.Append(" AND h.OwnerCode=@0", ownerCode);
            sql.Append(" AND snt.Code=@0 ", ItemCode);
            //sql.Append(" AND TpLine.LineState=@0", 3);        // 产品上线
            //sql.Append(" AND TpLine.IsValid=1");              // 产品有效
            //sql.Append(" AND TpTourPlan.TourState=@0", 3);    // 团计划上线

            List<HotelModel> hotTours = _hotelDao.Query(sql.SQL, sql.Arguments).ToList();
            return hotTours.GroupBy(t => t.HotelCode).Select(t => t.First()).ToList();
        }
        /// <summary>
        /// 取得所有 上线的线路
        /// </summary>
        /// <returns></returns>
        public List<TpLineModel> GetAllLine(string ownerCode)
        {
            // 有开班 提前预定日期 上线的所有线路
            var sql = new Sql();
            sql.Append(@"SELECT DISTINCT TpLine.LineId, TpLine.LineName, TpLine.LineType,TpLine.DepartDest,TpLine.TeamID ");
            sql.Append(@" FROM TpLine INNER JOIN TpTourPlan ON TpTourPlan.LineId=TpLine.LineId");
            sql.Append(@" INNER JOIN TpTourQuotaMap ON TpTourQuotaMap.TourId=TpTourPlan.Id");
            sql.Append(@" INNER JOIN TpQuota ON TpQuota.Id=TpTourQuotaMap.QuotaId");
            sql.Append(@" WHERE TpLine.OwnerCode = @0",ownerCode);
            sql.Append(@" AND TpTourPlan.BookingLastDays>=@0", DateTime.Today);
            sql.Append(@" AND TpLine.LineState = @0", 3);
            sql.Append(@" AND TpLine.IsValid=1");
            sql.Append(@" AND TpTourPlan.TourState = @0", 3);

            return _lineDao.Query<TpLineModel>(sql.SQL, sql.Arguments).ToList();
        }

        public PagedList<TpLineModel> GetProListByCondition(SearchProductVModel qModel, int pageIndex, int pageSize)
        {
            var sql = new Sql();
            sql.Append(@"SELECT DISTINCT TpLine.LineId, TpLine.LineName, TpLine.LineType,TpLine.DepartDest,TpLine.TeamID ");
            sql.Append(@" FROM TpLine INNER JOIN TpTourPlan ON TpTourPlan.LineId=TpLine.LineId");
            sql.Append(@" INNER JOIN TpTourQuotaMap ON TpTourQuotaMap.TourId=TpTourPlan.Id");
            sql.Append(@" INNER JOIN TpQuota ON TpQuota.Id=TpTourQuotaMap.QuotaId");
            sql.Append(@" WHERE TpLine.OwnerCode = @0", qModel.OwnerCode);
            sql.Append(@" AND TpTourPlan.BookingLastDays>=@0", DateTime.Today);
            sql.Append(@" AND TpLine.LineState = @0", 3);
            sql.Append(@" AND TpLine.IsValid=1");
            sql.Append(@" AND TpTourPlan.TourState = @0", 3);

            if (!qModel.LineId.IsNullOrEmpty())
            {
                sql.Append(" and TpLine.LineID = @0", Ansi(qModel.LineId.Trim()));
            }
            if (!qModel.LineName.IsNullOrEmpty())
            {
                sql.Append(" and TpLine.LineName like @0", AnsiLike(qModel.LineName.Trim()));
            }
            if (qModel.LineType != default(int) && qModel.LineType != -1)
            {
                sql.Append(" and TpLine.LineType=@0", qModel.LineType);
            }

            return _lineDao.Pager<TpLineModel>(pageIndex, pageSize, sql.SQL, sql.Arguments);
        }

        #endregion 旅游线路

        #region 签证部分

        /// <summary>
        /// 取得所有的 签证 上线 的产品
        /// </summary>
        /// <returns></returns>
        public List<HotTourVModel> GetAllVisa()
        {
            Sql sql = new Sql();
            sql.Append(@"select vi.InformationCode LineId, vi.InformationName LineName,
  cc.Name VisaIssuePlaceName, bdd1.Value VisaTypeValue
FROM  Visa_Information vi INNER JOIN BaseDestination cc ON cc.ParentStr = vi.VisaIssuePlace
INNER JOIN BaseDictionaryDetail bdd1 on vi.VisaType=bdd1.`Key` and bdd1.Name='VisaTypeEnum'
where vi.IsValid=1 and vi.State=5 and VType=1 ");

            List<HotTourVModel> hotTours = _planDao.Query<HotTourVModel>(sql.SQL, sql.Arguments).ToList();

            return hotTours;
        }

        /// <summary>
        /// 取得所有上线的团签
        /// </summary>
        /// <param name="vtype">1个签 2团签</param>
        /// <returns></returns>
        public List<VisaProductQModel> GetB2bVisaList(string vtype)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT b.InformationCode ProductCode,b.InformationName ProductName,c.Name VisaIssuePlace,
 b.TradePrice, b.SellPrice SalePrice,b.ImgUrl CountryImgUrl,b.VisaCountryParentStr CountryCode,d.Name CountryName
FROM Visa_Information b,BaseDestination c,BaseDestination d
WHERE b.VisaIssuePlace = c.ParentStr AND b.VisaCountryParentStr = d.ParentStr
   AND b.State=5 AND b.IsValid=1 ");

            if (!string.IsNullOrEmpty(vtype))
            {
                sql.Append(" AND b.VType=@0 ", vtype);
            }

            List<VisaProductQModel> productList = _visaDao.Query<VisaProductQModel>(sql.SQL, sql.Arguments).ToList();

            return productList;
        }

        /// <summary>
        /// 前台使用
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public List<VisaProductQModel> GetHotVisaList(string code)
        {
            var tempSql = @"select b.InformationCode ProductCode,b.InformationName ProductName,c.Name VisaIssuePlace,
b.SellPrice SalePrice,b.ImgUrl CountryImgUrl,b.VisaCountryParentStr CountryCode,d.Name CountryName
FROM Visa_Information b
  INNER JOIN site_nav_list snl ON b.InformationCode = snl.ProductID
  INNER JOIN site_nav_items sni ON sni.ItemID = snl.ItemID
  INNER JOIN BaseDestination c ON b.VisaIssuePlace = c.ParentStr
  INNER JOIN BaseDestination d ON b.VisaCountryParentStr = d.ParentStr
WHERE  b.State=5 AND b.IsValid=1 AND b.VType=1 AND sni.Code=@0
ORDER BY snl.SortOrder";

            List<VisaProductQModel> productList = _visaDao.Query<VisaProductQModel>(tempSql, code).ToList();

            return productList;
        }

        public IList<VisaCountryQModel> GetHotCountryList()
        {
            string tempSql = @"SELECT a.Continent BanKuaiKey,b.ParentStr CountryCode,b.Name CountryName,b.Id AreaId
FROM Visa_Information a INNER JOIN BaseDestination b ON a.VisaCountryParentStr = b.ParentStr
WHERE a.VType=1 AND a.State=5 AND a.IsValid=1
GROUP BY a.Continent,b.Name,b.ParentStr,b.Id
ORDER BY b.Name ";

            List<VisaCountryQModel> countryList = _visaDao.Query<VisaCountryQModel>(tempSql).ToList();

            return countryList;
        }

        public List<VisaInformationModel> QueryVisaProductList(string visaCountryParentStr, int vtype)
        {
            Sql sql = new Sql();
            sql.Append("select * from Visa_Information where State=5 and IsValid=1 ", visaCountryParentStr);
            if (!string.IsNullOrEmpty(visaCountryParentStr))
                sql.Append("and VisaCountryParentStr=@0", visaCountryParentStr);
            if (vtype != 0)
                sql.Append(" and VType=@0 ", vtype);

            return _visaDao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 个签
        /// </summary>
        /// <param name="countryName"></param>
        /// <param name="visaType"></param>
        /// <param name="productName"></param>
        /// <returns></returns>
        public List<VisaInformationModel> QueryVisaProductList(string countryName, int visaType, string productName)
        {
            Sql sql = new Sql();
            sql.Append(@"select vi.*, bd.Name VisaIssuePlaceName from Visa_Information
inner join BaseDestination bd on vi.VisaIssuePlace=bd.ParentStr
where vi.IsValid=1 and vi.State=5 and vi.VType=1 ");

            if (!string.IsNullOrEmpty(countryName))
            {
                sql.Append(" and vi.VisaCountry=@0", countryName);
            }
            if (visaType > 0)
            {
                sql.Append(" and vi.VisaType=@0", visaType);
            }
            if (!string.IsNullOrEmpty(productName))
            {
                sql.Append(" and vi.InformationName like @0", AnsiLike(productName));
            }

            return _visaDao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 取得产品信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public VisaInformationModel GetVisaProductInfo(string code)
        {
            Sql sql = new Sql();
            sql.Append(@"select vi.*, bd.Name VisaIssuePlaceName from Visa_Information vi
inner join BaseDestination bd on vi.VisaIssuePlace=bd.ParentStr
where vi.IsValid=1 and vi.InformationCode=@0 ", code);

            return _visaDao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 获得签证的国家列表
        /// </summary>
        /// <param name="vtype">类型</param>
        /// <param name="isGroupBk">是否分大洲</param>
        /// <returns></returns>
        public List<VisaInformationModel> QueryVisaCountryList(int vtype, bool isGroupBk)
        {
            Sql sql = new Sql();
            sql.Append("select * from Visa_Information where State=5 and IsValid=1");
            if (vtype > 0)
            {
                sql.Append(" and VType=@0 ", vtype);
            }
            var query = _visaDao.Fetch(sql.SQL, sql.Arguments);
            if (isGroupBk)
            {
                var tempQuery = from a in query
                                group a by
                                new
                                {
                                    a.VType,
                                    a.VisaCountryParentStr,
                                    a.VisaCountry,
                                    a.Continent
                                }
                                    into b
                                select new VisaInformationModel
                                {
                                    VType = b.Key.VType,
                                    Continent = b.Key.Continent,
                                    VisaCountryParentStr = b.Key.VisaCountryParentStr,
                                    VisaCountry = b.Key.VisaCountry
                                };
                return tempQuery.ToList();
            }
            else
            {
                var tempQuery = from a in query
                                group a by
                                new
                                {
                                    a.VType,
                                    a.VisaCountryParentStr,
                                    a.VisaCountry
                                }
                    into b
                                select new VisaInformationModel
                                {
                                    VType = b.Key.VType,
                                    VisaCountryParentStr = b.Key.VisaCountryParentStr,
                                    VisaCountry = b.Key.VisaCountry
                                };
                return tempQuery.ToList();
            }
        }

        /// <summary>
        /// 个签
        /// </summary>
        /// <param name="pCodeArrayStr"></param>
        /// <returns></returns>
        public List<VisaInformationModel> QueryVisaProductList(string pCodeArrayStr)
        {
            Sql sql = new Sql();
            sql.Append(@"select vi.*, bd.Name VisaIssuePlaceName from Visa_Information
inner join BaseDestination bd on vi.VisaIssuePlace=bd.ParentStr
where vi.IsValid=1 and vi.State=5 and vi.VType=1 ");

            if (string.IsNullOrEmpty(pCodeArrayStr))
            {
                sql.Append(" and vi.InforamationCode in (@0)", pCodeArrayStr.Split(','));
            }

            return _visaDao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 后台预定产品查询
        /// </summary>
        /// <param name="qModel"></param>
        /// <returns></returns>
        public PagedList<VisaInformationModel> QueryOnLineProductPagedList(VisaBookingQModel qModel)
        {
            Sql sql = new Sql();
            sql.Append(@"select vi.*, cc.Name VisaIssuePlaceName, bdd.Value InterviewTypeValue, bdd1.Value VisaTypeValue,
    bdd2.Value ContinentValue, bdd3.Value VisaAreaValue
FROM Visa_Information vi inner join BaseDestination cc on cc.ParentStr = vi.VisaIssuePlace
inner join BaseDictionaryDetail bdd on vi.InterviewType=bdd.`Key` and bdd.Name='InterviewTypeEnum'
inner join BaseDictionaryDetail bdd1 on vi.VisaType=bdd1.`Key` and bdd1.Name='VisaTypeEnum'
inner join BaseDictionaryDetail bdd2 on vi.Continent=bdd2.`Key` and bdd2.Name='ContinentEnum'
inner join BaseDictionaryDetail bdd3 on vi.VisaArea=bdd3.`Key` and bdd3.Name='VisaAreaEnum'
WHERE vi.OwnerCode=@0 AND vi.IsValid=1 and vi.State=5 and VType=1 ", qModel.OwnerCode);

            if (!qModel.InformationCode.IsNullOrEmpty())
            {
                sql.Append(" and vi.InformationCode like @0", AnsiLike(qModel.InformationCode.Trim()));
            }
            if (!qModel.InformationName.IsNullOrEmpty())
            {
                sql.Append(" and vi.InformationName like @0", AnsiLike(qModel.InformationName.Trim()));
            }
            if (qModel.VisaType != default(int) && qModel.VisaType > 0)
            {
                sql.Append(" and vi.VisaType=@0", qModel.VisaType);
            }
            if (!qModel.VisaCountry.IsNullOrEmpty())
            {
                sql.Append(" and vi.VisaCountryParentStr=@0", qModel.VisaCountry);
            }
            if (!qModel.VisaIssuePlace.IsNullOrEmpty())
            {
                sql.Append(" and cc.Name like @0", AnsiLike(qModel.LinqQuValue));
            }
            if (!qModel.LinqQuValue.IsNullOrEmpty())
            {
                sql.Append(" and vi.VisaArea=@0", qModel.LinqQuValue);
            }

            sql.Append(" ORDER BY vi.CreateTime DESC ");

            var list = _visaDao.Pager<VisaInformationModel>(qModel.VisaInformationList.PageIndex, qModel.VisaInformationList.PageSize, sql.SQL, sql.Arguments);

            return list;
        }

        public PagedList<VisaInformationModel> GetProListByCondition(VisaInformationQModel qModel, int pageIndex, int pageSize)
        {
            Sql sql = new Sql();
            sql.Append(@"select vi.*, cc.Name VisaIssuePlaceName, bdd.Value InterviewTypeValue, bdd1.Value VisaTypeValue, bdd2.Value ContinentValue, bdd3.Value VisaAreaValue
from  Visa_Information vi inner join BaseDestination cc on cc.ParentStr = vi.VisaIssuePlace
inner join BaseDictionaryDetail bdd on vi.InterviewType=bdd.`Key` and bdd.Name='InterviewTypeEnum'
inner join BaseDictionaryDetail bdd1 on vi.VisaType=bdd1.`Key` and bdd1.Name='VisaTypeEnum'
inner join BaseDictionaryDetail bdd2 on vi.Continent=bdd2.`Key` and bdd2.Name='ContinentEnum'
inner join BaseDictionaryDetail bdd3 on vi.VisaArea=bdd3.`Key` and bdd3.Name='VisaAreaEnum'
where vi.IsValid=1 and vi.State=5 and VType=1 ");

            if (qModel.Info != null)
            {
                if (!qModel.Info.InformationCode.IsNullOrEmpty())
                {
                    sql.Append(" and vi.InformationCode like @0", AnsiLike(qModel.Info.InformationCode.Trim()));
                }
                if (!qModel.Info.InformationName.IsNullOrEmpty())
                {
                    sql.Append(" and vi.InformationName like @0", AnsiLike(qModel.Info.InformationName.Trim()));
                }
                if (qModel.Info.VType != default(int) && qModel.Info.VType != -1)
                {
                    sql.Append(" and vi.VType=@0", qModel.Info.VType);
                }
                if (qModel.Info.State != default(int) && qModel.Info.State != -1)
                {
                    sql.Append(" and vi.State=@0", qModel.Info.State);
                }
                if (qModel.Info.VisaType != default(int) && qModel.Info.VisaType != -1)
                {
                    sql.Append(" and vi.VisaType=@0", qModel.Info.VisaType);
                }
            }

            return _visaDao.Pager<VisaInformationModel>(pageIndex, pageSize, sql.SQL, sql.Arguments);
        }

        #endregion 签证部分

        #region 门票部分

        /// <summary>
        /// 取得所有在售门票产品
        /// </summary>
        /// <returns></returns>
        public List<HotTourVModel> GetAllTicket(string ownerCode)
        {
            string sql = @"SELECT a.ProductId AS LineId, a.ProductName AS LineName, b.*, c.*
FROM TktProduct a LEFT JOIN TktPriceRule b ON a.ProductId=b.ProductId
 LEFT JOIN TktQuota c ON a.ProductId=c.ProductId
 WHERE a.OwnerCode=@0 AND a.ProductState=4
  AND b.Year=@1 AND (c.EndTime is null or c.EndTime>=now())
  AND b.Id IN (SELECT RuleId FROM TktRulePriceMap WHERE CurrentDate=@2)
  ORDER BY a.ModifiedTime DESC ";

            List<HotTourVModel> hotTours = _planDao.Query<HotTourVModel>(sql, ownerCode, DateTime.Today.Year, DateTime.Today).ToList();

            return hotTours;
        }

        #endregion

        #region 酒店部分
      
        /// <summary>
        /// 取得所有在售酒店产品
        /// </summary>
        /// <returns></returns>
        public List<HotTourVModel> GetAllHotel(string ownerCode)
        {
            string sql = @"SELECT DISTINCT h.HotelCode AS LineId, h.HotelName AS LineName FROM hotels h 
INNER JOIN hotel_rooms hr ON h.HotelCode=hr.HotelCode
INNER JOIN hotel_stock hs ON hr.RoomID = hs.RoomID
WHERE h.HotelState=3 AND h.isvalid=1 AND hs.CheckInDate>NOW() ";

            List<HotTourVModel> hotTours = _planDao.Query<HotTourVModel>(sql, ownerCode, DateTime.Today.Year, DateTime.Today).ToList();

            return hotTours;
        }

        #endregion
    }
}