using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.APIVModels.Req
{
    public class BaseRequest
    {
        #region 授权认证

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 授权码
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 系统编号
        /// </summary>
        public string OwnerCode { get; set; }

        #endregion
    }
}
