using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.CrmDB;
using Lvy.Models.SiteDB;
using Lvy.Models.TicketDB;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Site;
using Lvy.Trip.Biz.Ticket;
using Lvy.Trip.Dao.Crm;
using Lvy.VModels.Online;
using Lvy.Web.Common;
using Microsoft.Extensions.Caching.Memory;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz
{
    /// <summary>
    ///  字典工具类
    /// </summary>
    public class DictionaryBiz
    {
        //private static string connectionString = "Server=112.124.7.61;uid=yuan;pwd=58f55f5s;Charset=utf8;database=yuandb;port=3306;Allow User Variables=True";
        private static string providerName = "MySql.Data.MySqlClient";

        private static MemoryCache _memoryCache;

        public static MemoryCache Current
        {
            get
            {
                if (_memoryCache == null)
                {
                    return _memoryCache = new MemoryCache(new MemoryCacheOptions());
                }

                return _memoryCache;
            }
        }

        /// <summary>
        /// 通过key取关联的字典（枚举）对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<KeyValueBean> GetEnumsBy(string key)
        {
            var obj = Current.Get(key);
            if (obj != null)
                return obj as List<KeyValueBean>;

            Database db = new Database(MyHelper.connectionString, providerName);
            Sql sql = new Sql();
            sql.Append("SELECT `Key`,`Value` FROM BaseDictionaryDetail WHERE Name=@0 AND IsValid=1 ", key);
            sql.Append(" ORDER BY `Key` ");

            var results = db.Fetch<KeyValueBean>(sql);
            Current.Set(key, results);

            return results;
        }

        public static List<KeyValueBean> GetEnumsBys(string key)
        {
            var obj = Current.Get(key);
            if (obj != null)
                return obj as List<KeyValueBean>;

            Database db = new Database(MyHelper.connectionString, providerName);
            Sql sql = new Sql();
            sql.Append("SELECT `Key`,Value FROM BaseDictionaryDetail WHERE Name=@0 AND IsValid=1 ", key);

            var results = db.Fetch<KeyValueBean>(sql);
            Current.Set(key, results);

            return results;
        }

        /// <summary>
        /// 通过key取关联的字典（枚举）对象
        /// </summary>
        /// <param name="key">字典key</param>
        /// <param name="enumKeys">通过枚举key 找到相应的value </param>
        /// <returns></returns>
        public static IEnumerable<KeyValueBean> GetEnumsBy(string key, string[] enumKeys)
        {
            var enums = GetEnumsBy(key);
            foreach (var keyValueBean in enums)
            {
                if (enumKeys.Contains(keyValueBean.Key))
                {
                    yield return keyValueBean;
                }
            }
        }

        /// <summary>
        /// 根据key取对应的value值
        /// </summary>
        /// <param name="key">字典Key</param>
        /// <param name="enumKey">字典详细Key</param>
        /// <param name="defualtFlag">
        /// true:enumKey=0 的场合，return "";
        /// false: enumKey=0的场合， return value;
        /// </param>
        /// <returns></returns>
        public static string GetEnumValue(string key, string enumKey, bool defualtFlag = true)
        {
            // key=0为默认值
            if (enumKey == "0" && defualtFlag)
                return string.Empty;
            var obj = Current.Get(key);
            if (obj == null)
            {
                //throw new Exception("缓存中没有{0}。请联系管理员！".With(key));
                GetEnumsBy(key);
                obj = Current.Get(key);
            }

            return (obj as List<KeyValueBean>).First(a => a.Key == enumKey).Value;
        }

        public static string GetDestName(string id)
        {
            var dests = Current.Get(Consts.Destination) as List<BaseDestinationModel>;
            if (dests == null)
            {
                Database db = new Database(MyHelper.connectionString, providerName);
                dests = db.Fetch<BaseDestinationModel>("SELECT * FROM BaseDestination WHERE isvalid=1");
                Current.Set(Consts.Destination, dests, TimeSpan.FromMilliseconds(Consts.OutputCacheDuration1));
            }
            var dest = dests.FirstOrDefault(a => a.Id == Convert.ToInt32(id));
            if (dest == null)
                return "";
            return dest.Name;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetDestNameStr(string id)
        {
            var dests = Current.Get(Consts.Destination) as List<BaseDestinationModel>;
            if (dests == null)
            {
                Database db = new Database(MyHelper.connectionString, providerName);
                dests = db.Fetch<BaseDestinationModel>("SELECT * FROM BaseDestination WHERE isvalid=1");
                Current.Set(Consts.Destination, dests, TimeSpan.FromMilliseconds(Consts.OutputCacheDuration1));
            }
            var dest = dests.FirstOrDefault(a => a.ParentStr == id);
            if (dest == null)
                return "";
            return dest.Name;
        }

        #region 数据常用key value对象集合

        #region 商户

        /// <summary>
        /// 获取客户的对象
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static CrmCustomerModel GetCachedCustomer(string code, string ownerCode)
        {
            var dic = GetCachedCustomerDict(ownerCode);
            if (!dic.Keys.Contains(code))
            {
                dic = GetCustomerDictionary(ownerCode);
                Current.Set(Consts.CustomerStrDic, dic);
                if (!dic.Keys.Contains(code))
                {
                    return new CrmCustomerModel();
                }

                //  throw new Exception("没有对应的商户信息。code=" + code);
            }
            return dic[code];
        }

        /// <summary>
        /// 获取客户对象字典
        /// key = code
        /// value = 客户Obj
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, CrmCustomerModel> GetCachedCustomerDict(string ownerCode)
        {
            Dictionary<string, CrmCustomerModel> dic = null;
            var obj = Current.Get(Consts.CustomerStrDic);

            if (obj == null)
            {
                dic = GetCustomerDictionary(ownerCode);
                Current.Set(Consts.CustomerStrDic, dic);
            }
            else
                dic = obj as Dictionary<string, CrmCustomerModel>;

            return dic;
        }

        private static Dictionary<string, CrmCustomerModel> GetCustomerDictionary(string ownerCode)
        {
            var dic = new Dictionary<string, CrmCustomerModel>();
            Database db = new Database(MyHelper.connectionString, providerName);
            var sql = "SELECT * FROM CrmCustomer WHERE ownercode=@0";
            var objs = db.Query<CrmCustomerModel>(sql, ownerCode);
            foreach (var item in objs)
            {
                dic.Add(item.Code, item);
            }
            return dic;
        }

        #endregion 商户

        #region 供应商

        /// <summary>
        /// 获取客户的对象
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static CrmCustomerModel GetCachedSuppliser(string code)
        {
            var dic = GetCachedSuppliserDict();
            if (!dic.Keys.Contains(code))
            {
                dic = GetSuppliserDictionary();
                Current.Set(Consts.SupplierStrDic, dic);
                if (!dic.Keys.Contains(code))
                    throw new Exception("没有对应的供应商信息。code=" + code);
            }
            return dic[code];
        }

        /// <summary>
        /// 获取客户对象字典
        /// key = code
        /// value = 客户Obj
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, CrmCustomerModel> GetCachedSuppliserDict()
        {
            Dictionary<string, CrmCustomerModel> dic = null;
            var obj = Current.Get(Consts.SupplierStrDic);

            if (obj == null)
            {
                dic = GetSuppliserDictionary();
                Current.Set(Consts.SupplierStrDic, dic);
            }
            else
                dic = obj as Dictionary<string, CrmCustomerModel>;

            return dic;
        }

        private static Dictionary<string, CrmCustomerModel> GetSuppliserDictionary()
        {
            var dic = new Dictionary<string, CrmCustomerModel>();
            Database db = new Database(MyHelper.connectionString, providerName);
            var sql = "SELECT * FROM CrmCustomer WHERE isvalid=1 AND IsSupplier=1";
            var objs = db.Query<CrmCustomerModel>(sql);
            foreach (var item in objs)
            {
                dic.Add(item.Code, item);
            }
            return dic;
        }

        #endregion 供应商

        #region 账户

        /// <summary>
        /// 获取账户的名称
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static CrmAccountModel GetCachedAccount(string code, string ownerCode)
        {
            var dic = GetCachedAccountDict(ownerCode);
            if (!dic.Keys.Contains(code))
            {
                dic = GetAccountDictionary(ownerCode);
                Current.Set(Consts.AccountStrDic, dic);
                if (!dic.Keys.Contains(code))
                {
                    // throw new Exception("没有对应的账户信息。code=" + code);
                    return null;
                }
            }
            return dic[code];
        }

        /// <summary>
        /// 获取表的code 和 name
        /// key = code
        /// value = name
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, CrmAccountModel> GetCachedAccountDict(string ownerCode)
        {
            Dictionary<string, CrmAccountModel> dic = null;
            var obj = Current.Get(Consts.AccountStrDic);

            if (obj == null)
            {
                dic = GetAccountDictionary(ownerCode);
                Current.Set(Consts.AccountStrDic, dic);
            }
            else
                dic = obj as Dictionary<string, CrmAccountModel>;

            return dic;
        }

        private static Dictionary<string, CrmAccountModel> GetAccountDictionary(string ownerCode)
        {
            var dic = new Dictionary<string, CrmAccountModel>();
            Database db = new Database(MyHelper.connectionString, providerName);
            var sql = "SELECT * FROM CrmAccount WHERE ownercode=" + ownerCode;
            var objs = db.Query<CrmAccountModel>(sql);
            foreach (var item in objs)
            {
                dic.Add(item.Code, item);
            }
            return dic;
        }

        #endregion 账户

        #region 部门

        public static CrmTeamModel GetCachedTeam(string code)
        {
            var dic = GetCachedTeamDict();
            if (!dic.Keys.Contains(code))
            {
                dic = GetTeamDictionary();
                Current.Set(Consts.TeamStrDic, dic);
                if (!dic.Keys.Contains(code))
                    throw new Exception("没有对应的部门信息。code=" + code);
            }
            return dic[code];
        }

        public static Dictionary<string, CrmTeamModel> GetCachedTeamDict()
        {
            Dictionary<string, CrmTeamModel> dic = null;
            var obj = Current.Get(Consts.TeamStrDic);

            if (obj == null)
            {
                dic = GetTeamDictionary();
                Current.Set(Consts.TeamStrDic, dic);
            }
            else
                dic = obj as Dictionary<string, CrmTeamModel>;

            return dic;
        }

        private static Dictionary<string, CrmTeamModel> GetTeamDictionary()
        {
            var dic = new Dictionary<string, CrmTeamModel>();
            Database db = new Database("MySql.Data.MySqlClient", providerName);
            var sql = "SELECT * FROM CrmTeam WHERE IsValid=1 ";
            var objs = db.Query<CrmTeamModel>(sql);
            foreach (var item in objs)
            {
                dic.Add(item.TeamID, item);
            }
            return dic;
        }

        #endregion 部门

        #region 品牌

        public static BrandModel GetCachedBrand(string code)
        {
            var dic = GetBrandDictionary();
            if (!dic.Keys.Contains(code))
            {
                dic = GetCachedBrandDict();
                Current.Set(Consts.BrandStrDic, dic);
                if (!dic.Keys.Contains(code))
                    throw new Exception("没有对应的品牌信息。code=" + code);
            }
            return dic[code];
        }

        public static Dictionary<string, BrandModel> GetCachedBrandDict()
        {
            Dictionary<string, BrandModel> dic = null;
            var obj = Current.Get(Consts.BrandStrDic);

            if (obj == null)
            {
                dic = GetBrandDictionary();
                Current.Set(Consts.BrandStrDic, dic);
            }
            else
                dic = obj as Dictionary<string, BrandModel>;

            return dic;
        }

        private static Dictionary<string, BrandModel> GetBrandDictionary()
        {
            var dic = new Dictionary<string, BrandModel>();
            Database db = new Database(MyHelper.connectionString, providerName);
            var sql = "SELECT * FROM BaseBrands WHERE IsValid=1 ";
            var objs = db.Query<BrandModel>(sql);
            foreach (var item in objs)
            {
                dic.Add(item.Code, item);
            }
            return dic;
        }

        #endregion 品牌

        #region 航空共公司

        public static BaseAirlineModel GetCachedAirline(string code)
        {
            var dic = GetAirlineDictionary();
            if (!dic.Keys.Contains(code))
            {
                dic = GetCachedAirlineDict();
                Current.Set(Consts.AirlineStrDic, dic);
                if (!dic.Keys.Contains(code))
                    throw new Exception("没有对应的航空公司信息。code=" + code);
            }
            return dic[code];
        }

        public static Dictionary<string, BaseAirlineModel> GetCachedAirlineDict()
        {
            Dictionary<string, BaseAirlineModel> dic = null;
            var obj = Current.Get(Consts.AirlineStrDic);

            if (obj == null)
            {
                dic = GetAirlineDictionary();
                Current.Set(Consts.AirlineStrDic, dic);
            }
            else
                dic = obj as Dictionary<string, BaseAirlineModel>;

            return dic;
        }

        private static Dictionary<string, BaseAirlineModel> GetAirlineDictionary()
        {
            var dic = new Dictionary<string, BaseAirlineModel>();
            Database db = new Database(MyHelper.connectionString, providerName);
            var sql = "select * from BaseAirlines ";
            var objs = db.Query<BaseAirlineModel>(sql);
            foreach (var item in objs)
            {
                dic.Add(item.Code, item);
            }
            return dic;
        }

        #endregion 航空共公司

        #region 通用产品

        /// <summary>
        /// 所有门票产品
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, TktProductModel> GetCachedTicketDict(string ownerCode)
        {
            Dictionary<string, TktProductModel> dic = null;
            var obj = Current.Get(Consts.TicketStrDic);

            if (obj == null)
            {
                dic = GetTicketDictionary(ownerCode);
                Current.Set(Consts.TicketStrDic, dic);
            }
            else
                dic = obj as Dictionary<string, TktProductModel>;

            return dic;
        }

        private static Dictionary<string, TktProductModel> GetTicketDictionary(string ownerCode)
        {
            var dic = new Dictionary<string, TktProductModel>();
            Database db = new Database(MyHelper.connectionString, providerName);
            var sql = "SELECT * FROM TktProduct WHERE OwnerCode=@0";
            var objs = db.Query<TktProductModel>(sql, ownerCode);
            foreach (var item in objs)
            {
                dic.Add(item.ProductId, item);
            }
            return dic;
        }

        #endregion 通用产品

        #region 景区

        public static string GetPlaceName(string code)
        {
            var dests = Current.Get(Consts.BasePlace) as List<BasePlaceModel>;
            if (dests == null)
            {
                Database db = new Database(MyHelper.connectionString, providerName);
                dests = db.Fetch<BasePlaceModel>("SELECT * FROM BasePlace WHERE IsValid=1");
                Current.Set(Consts.BasePlace, dests, TimeSpan.FromMilliseconds(Consts.OutputCacheDuration1));
            }
            var dest = dests.FirstOrDefault(a => a.PlaceCode == code);
            if (dest == null)
                return "";
            return dest.PlaceName;
        }

        public static List<BasePlaceModel> GetCachePlaces()
        {
            List<BasePlaceModel> dest = null;

            if (Current.Get(Consts.BasePlace) == null)
            {
                var list = BasePlaceBiz.GetPlaces();
                Current.Set(Consts.BasePlace, list, TimeSpan.FromMilliseconds(Consts.OutputCacheDuration1));
            }

            dest = Current.Get(Consts.BasePlace) as List<BasePlaceModel>;

            return dest;
        }

        #endregion 景区

        #endregion 数据常用key value对象集合

        /// <summary>
        /// 获取所有的目的地
        /// </summary>
        /// <returns></returns>
        public static List<BaseDestinationModel> GetCacheDests()
        {
            if (Current.Get(Consts.Destination) == null)
            {
                var list = DestinationBiz.GetDests();
                Current.Set(Consts.Destination, list, TimeSpan.FromMilliseconds(Consts.OutputCacheDuration1));
            }

            return Current.Get(Consts.Destination) as List<BaseDestinationModel>;
        }

        public static string GetCacheDestName(string id)
        {
            var dests = Current.Get(Consts.Destination) as List<BaseDestinationModel>;
            if (dests == null)
                return "";

            return dests.FirstOrDefault(a => a.Id == id.ToInt()).Name;
        }

        public static string GetCacheDestNameStr(string id)
        {
            var dests = Current.Get(Consts.Destination) as List<BaseDestinationModel>;
            if (dests == null)
                return "";

            return dests.FirstOrDefault(a => a.ParentStr == id).Name;
        }

        /// <summary>
        /// 获取有开班线路关联的目的地
        /// 旧版首页目的地导航
        /// </summary>
        /// <returns></returns>
        public static List<DestNavVModel> GetLineDestsCached(string ownerCode)
        {
            if (Current.Get("DestNavVModel") == null)
            {
                var destNav = GetLineDests(ownerCode);
                Current.Set("DestNavVModel", destNav, TimeSpan.FromMilliseconds(Consts.OutputCacheDuration1));
            }

            return Current.Get("DestNavVModel") as List<DestNavVModel>;
        }

        /// <summary>
        /// 获取所有有开班线路关联的目的地
        /// </summary>
        /// <returns></returns>
        public static List<DestNavVModel> GetLineDests(string ownerCode)
        {
            // 包含线路的所有目的地
            var sql1 = @" select a.LineType, a.ArriveDest
from TpLine a where OwnerCode=@0
and IsValid=1
group by a.ArriveDest ,a.LineType  ";

            //今后 有团的线路
            var sql2 = @" SELECT distinct a.LineType, a.DepartDest, a.ArriveDest
FROM TpLine a
INNER JOIN TpTourPlan b ON a.LineId = b.LineId
WHERE b.OutDate>=@0 AND b.TourState=3 AND a.OwnerCode=@1
 AND a.IsValid=1 AND a.LineState=3 ";

            DestinationDao _dao = new DestinationDao();

            var allLineDest = _dao.Query<DestNavVModel>(sql1, ownerCode).ToList();
            var tourDest = _dao.Query<DestNavVModel>(sql2, DateTime.Today, ownerCode).ToList();  // 有出团的线路

            var vmodels = new List<DestNavVModel>();
            foreach (var destNavVModel in allLineDest)
            {
                // 补充有团的标记
                destNavVModel.IsHasTour = tourDest.Count(a => a.LineType == destNavVModel.LineType && a.ArriveDest == destNavVModel.ArriveDest) > 0;

                var obj = GetCacheDests().FirstOrDefault(a => a.ParentStr == destNavVModel.ArriveDest);

                destNavVModel.ArriveDestName = obj.Name;
                destNavVModel.ParentId = destNavVModel.ParentId;
                destNavVModel.ClickCnt = obj.ClickCnt;
                vmodels.Add(destNavVModel);
            }

            return vmodels;
        }

        /// <summary>
        /// 取得省份列表
        /// </summary>
        /// <returns></returns>
        public List<BaseDestinationModel> GetProvinceList()
        {
            var dests = Current.Get(Consts.Destination) as IList<BaseDestinationModel>;
            if (dests == null)
            {
                dests = new DestinationDao().GetDests();
                Current.Set(Consts.Destination, dests);
            }

            return dests.Where(t => t.Level == 10 && t.IsChina == 1).ToList();
        }

        /// <summary>
        /// 取得城市列表
        /// </summary>
        /// <param name="destId">省份</param>
        /// <returns></returns>
        public List<BaseDestinationModel> GetChildList(string destId)
        {
            var dests = Current.Get(Consts.Destination) as IList<BaseDestinationModel>;
            if (dests == null)
            {
                dests = new DestinationDao().GetDests();
                Current.Set(Consts.Destination, dests);
            }

            int prv = Convert.ToInt32(destId);
            return dests.Where(t => t.IsChina == 1 && t.ParentId == prv).ToList();
        }

        public List<SiteNavItemModel> GetLineDestsCached(string code, string OwnerCode)
        {
            SiteNavBiz biz = new SiteNavBiz();
            if (Current.Get("SiteNavModel" + code) == null)
            {
                var destNav = biz.GetLineDests(code, OwnerCode);
                Current.Set("SiteNavModel" + code, destNav, TimeSpan.FromMilliseconds(Consts.OutputCacheDuration1));
            }

            return Current.Get("SiteNavModel" + code) as List<SiteNavItemModel>;
        }

        #region Common

        /// <summary>
        /// 所有上线的门票产品
        /// </summary>
        /// <returns></returns>
        public List<TktProductModel> GetCacheProducts(string ownerCode)
        {
            if (Current.Get(Consts.TktProduct) == null)
            {
                TktProductBiz biz = new TktProductBiz();
                var list = biz.GetProducts(ownerCode);
                Current.Set(Consts.TktProduct, list, TimeSpan.FromMilliseconds(Consts.OutputCacheDuration2));
            }

            return Current.Get(Consts.TktProduct) as List<TktProductModel>;
        }

        #endregion Common
    }
}