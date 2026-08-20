using Lvy.Models.ApiDB;
using System;

namespace Lvy.Trip.Dao.API.User
{
    public class UserDao : YuanDbRepository<ImplUser>
    {
        /// <summary>
        /// 检查用户密钥
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public string CheckUserToken(string uid, string token)
        {
            string sql = "select OwnerCode from ImplUser where ImplUser.UserName=@0 And ImplUser.Token=@1";
            return _repo.ExecuteScalar<String>(sql, uid, token);
        }

        /// <summary>
        /// 检查用户访问IP
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="ipaddress">地址</param>
        /// <returns></returns>
        public bool CheckUserIpAdress(string username, string ipaddress)
        {
            string sql = "select IPList from ImplUser Where ImplUser.UserName=@0";
            var iplistString = _repo.ExecuteScalar<String>(sql, username);
            return iplistString.Contains(ipaddress);
        }
    }
}