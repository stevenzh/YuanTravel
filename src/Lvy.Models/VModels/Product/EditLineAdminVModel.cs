using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Product
{
    public class EditLineAdminVModel : BaseVModel
    {
        /// <summary>
        /// 线路编号
        /// </summary>
        public string LineId { get; set; }
        /// <summary>
        /// 专线批发商 专管员
        /// </summary>
        public List<LineAdminVModel> CustomerLineAdmin { get; set; }
        /// <summary>
        /// 平台供应商 专管员
        /// </summary>
        public List<LineAdminVModel> PlatLineAdmin { get; set; }
    }

    public class LineAdminVModel
    {
        /// <summary>
        /// 是否选择
        /// </summary>
        public int Checked { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 专管员编号
        /// </summary>
        public int LineAdminId { get; set; }
        /// <summary>
        /// 专管员账号
        /// </summary>
        public string AccountCode { get; set; }
        /// <summary>
        /// 所属单位 0：专线批发商 1：平台供应商
        /// </summary>
        public int Department { get; set; }
        /// <summary>
        /// 是否主要负责 0：否，1：是
        /// </summary>
        public int IsPrimary { get; set; }
    }
}
