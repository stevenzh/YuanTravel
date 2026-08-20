using Lvy.Models.BaseDB;
using System.Collections.Generic;

namespace Lvy.VModels.Base
{
    public class PhotoInfoQModel
    {
        public PhotoInfoModel Model { get; set; }
        public IList<PhotoInfoModel> List { get; set; }
        public long Total { get; set; }

        /// <summary>
        /// 当前页面
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每一页显示的数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页面
        /// </summary>
        public int PageCount { get; set; }
    }
}