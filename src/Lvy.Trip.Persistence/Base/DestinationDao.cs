using Lvy.Models.BaseDB;
using System.Collections.Generic;

namespace Lvy.Trip.Dao.Crm
{
    public class DestinationDao : YuanDbRepository<BaseDestinationModel>
    {
        /// <summary>
        /// 获取所有的目的地
        /// </summary>
        /// <returns></returns>
        public List<BaseDestinationModel> GetDests()
        {
            string sql = "select * from BaseDestination where isvalid=1";

            return Fetch(sql);
        }
    }
}