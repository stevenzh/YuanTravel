using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.APIVModels.Res
{
    public class BaseResponse
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public BaseResponse()
        {
            StatusCode = "200";
            Msg = null;
        }

        /// <summary>
        /// 状态码
        /// 200 成功执行
        /// 400 参数有误
        /// 401 验证失败
        /// 403 权限不足
        /// 404 未找到
        /// 405 不允许操作
        /// 500 内部错误
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Msg { get; set; }

        public void SetFailedResultCode(string msg, string statusCode = "404")
        {
            StatusCode = statusCode;
            Msg = msg;
        }
    }
}
