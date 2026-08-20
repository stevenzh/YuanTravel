using PetaPoco;
using System;

namespace Lvy.Models.ApiDB
{
    /// <summary>
    /// 访问日志表
    /// </summary>
    [TableName("ImplLog")]
    [PrimaryKey("Id")]
    public class ImpLog
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public DateTime CreatedTime { get; set; }

        public string MethodName { get; set; }

        public string MethodParam { get; set; }

        public int DoTime { get; set; }

        public string Ip { get; set; }
    }

    /// <summary>
    /// 用户表
    /// </summary>
    [TableName("ImplUser")]
    [PrimaryKey("Code")]
    [Serializable]
    public class ImplUser
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// IP地址集
        /// </summary>
        public string IpList { get; set; }
    }
}