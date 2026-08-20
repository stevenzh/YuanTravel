using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.CrmDB;
using Lvy.Models.TicketDB;
using Lvy.Trip.Biz;
using System.Collections.Generic;

namespace Lvy.Web.Common
{

    /// <summary>
    ///  字典工具类
    /// </summary>
    public class DictionaryTools
    {
        /// <summary>
        /// 通过key取关联的字典（枚举）对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<KeyValueBean> GetEnumsBy(string key)
        {
            return DictionaryBiz.GetEnumsBy(key);
        }

        public static List<KeyValueBean> GetEnumsBys(string key)
        {
            return DictionaryBiz.GetEnumsBys(key);
        }

        /// <summary>
        /// 通过key取关联的字典（枚举）对象
        /// </summary>
        /// <param name="key">字典key</param>
        /// <param name="enumKeys">通过枚举key 找到相应的value </param>
        /// <returns></returns>
        public static IEnumerable<KeyValueBean> GetEnumsBy(string key, string[] enumKeys)
        {
            return DictionaryBiz.GetEnumsBy(key, enumKeys);
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
            return DictionaryBiz.GetEnumValue(key, enumKey, defualtFlag);
        }

        public static string GetDestName(string id)
        {
            return DictionaryBiz.GetDestName(id);
        }

        public static string GetDestNameStr(string id)
        {
            return DictionaryBiz.GetDestNameStr(id);
        }


        #region 数据常用key value对象集合

        #region 商户

        /// <summary>
        /// 获取客户的对象
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static CrmCustomerModel GetCachedCustomer(string code)
        {
            return DictionaryBiz.GetCachedCustomer(code, GlobalContext.Current.OwnerCode);
        }

        /// <summary>
        /// 获取客户对象字典
        /// key = code
        /// value = 客户Obj
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, CrmCustomerModel> GetCachedCustomerDict()
        {
            return DictionaryBiz.GetCachedCustomerDict(GlobalContext.Current.OwnerCode);
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
            return DictionaryBiz.GetCachedSuppliser(code);
        }

        /// <summary>
        /// 获取客户对象字典
        /// key = code
        /// value = 客户Obj
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, CrmCustomerModel> GetCachedSuppliserDict()
        {
            return DictionaryBiz.GetCachedSuppliserDict();
        }


        #endregion 供应商

        #region 账户

        /// <summary>
        /// 获取账户的名称
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static CrmAccountModel GetCachedAccount(string code)
        {
            return DictionaryBiz.GetCachedAccount(code, GlobalContext.Current.OwnerCode);
        }

        /// <summary>
        /// 获取表的code 和 name
        /// key = code
        /// value = name
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, CrmAccountModel> GetCachedAccountDict()
        {
            return DictionaryBiz.GetCachedAccountDict(GlobalContext.Current.OwnerCode);
        }

        #endregion 账户

        #region 部门

        public static CrmTeamModel GetCachedTeam(string code)
        {
            return DictionaryBiz.GetCachedTeam(code);
        }

        public static Dictionary<string, CrmTeamModel> GetCachedTeamDict()
        {
            return DictionaryBiz.GetCachedTeamDict();
        }

        #endregion 部门

        #region 品牌

        public static BrandModel GetCachedBrand(string code)
        {
            return DictionaryBiz.GetCachedBrand(code);
        }

        public static Dictionary<string, BrandModel> GetCachedBrandDict()
        {
            return DictionaryBiz.GetCachedBrandDict();
        }


        #endregion 品牌

        #region 航空共公司

        public static BaseAirlineModel GetCachedAirline(string code)
        {
            return DictionaryBiz.GetCachedAirline(code);
        }

        public static Dictionary<string, BaseAirlineModel> GetCachedAirlineDict()
        {
            return DictionaryBiz.GetCachedAirlineDict();
        }

        #endregion 航空共公司

        #region 门票

        /// <summary>
        /// 所有门票产品
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, TktProductModel> GetCachedTicketDict()
        {
            return DictionaryBiz.GetCachedTicketDict(GlobalContext.Current.OwnerCode);
        }


        #endregion 门票

        #region 景区

        public static string GetPlaceName(string code)
        {
            return DictionaryBiz.GetPlaceName(code);
        }
        #endregion 景区

        #endregion 数据常用key value对象集合
    }
}