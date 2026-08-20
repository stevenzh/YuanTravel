using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Dao.Crm;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Crm
{
    /// <summary>
    /// 平台维护
    /// </summary>
    public class PlatformBiz : BaseBiz
    {
        private SysPlatformDao _platformDao = new SysPlatformDao();

        /// <summary>
        /// 根据Id获取一个平台对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SysPlatformModel GetById(int id)
        {
            return _platformDao.GetById(id);
        }

        public List<SysPlatformModel> GetAllPlatform()
        {
            return _platformDao.Fetch(" select * from SysPlatform ");
        }

        /// <summary>
        /// 根据商户编码获取一个平台对象
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public SysPlatformModel GetByCustomerCode(string customerCode)
        {
            return _platformDao.FirstOrDefault(@" SELECT * FROM SysPlatform WHERE CustomerCode=@0  ", customerCode);
        }

        /// <summary>
        /// 根据发布地址区分商户
        /// </summary>
        /// <param name="hostUrl"></param>
        /// <returns></returns>
        public SysPlatformModel GetByHostUrl(string hostUrl)
        {
            return _platformDao.FirstOrDefault(@" SELECT * FROM SysPlatform WHERE Url like @0  ", AnsiLike(hostUrl));
        }

        public List<KeyValueBean> GetPlatforms()
        {
            return (from kv in GetAllPlatform()
                    select new KeyValueBean() { Key = kv.CustomerCode, Value = kv.Name }).ToList();
        }
    }
}