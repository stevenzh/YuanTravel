using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.VModels
{
    public class PagedModel : BaseVModel
    {
        public PagedModel()
        {
            PagedSize = 20;
            PagedIndex = 1;
        }

        /// <summary>
        /// 页面数
        /// </summary>
        public int PagedCount { get; set; }
        /// <summary>
        /// 第几页
        /// </summary>
        public int PagedIndex { get; set; }
        /// <summary>
        /// 每页数
        /// </summary>
        public int PagedSize { get; set; }
        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalCount { get; set; }
    }
}
