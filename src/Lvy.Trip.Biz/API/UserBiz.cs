using PetaPoco;
using Lvy.Trip.Dao;
using Lvy.Trip.Dao.API.User;
using Lvy.Models.ApiDB;

namespace Lvy.API.Biz.User
{
    public class UserBiz : YuanDbRepository<ImpLog>
    {
        /// <summary>
        /// 用户验证
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="token">Token</param>
        /// <returns>父级Code</returns>
        public string CheckUserToken(string username, string token)
        {
            var ownerCode = new UserDao().CheckUserToken(username, token);
            return ownerCode;
        }

        /// <summary>
        /// IP地址校检
        /// </summary>
        /// <param name="username"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public bool CheckUserIpAdress(string username, string ipAddress)
        {
            return new UserDao().CheckUserIpAdress(username, ipAddress);
        }

        /// <summary>
        /// 接口访问记录
        /// </summary>
        /// <param name="log"></param>
        public int RecordApiVisit(ImpLog log)
        {
              return int.Parse(_repo.Insert(log).ToString());
        }

        public void RecordApiVistiUpdata(ImpLog existLog)
        {
            Sql sql = new Sql();
            sql.Append("Set DoTime=@0 Where Id=@1", existLog.DoTime, existLog.Id);
            _repo.Update<ImpLog>(sql.SQL, sql.Arguments);
        }
    }
}
