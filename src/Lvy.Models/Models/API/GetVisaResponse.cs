using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lvy.Visa.Models.API
{
    public class VisaCategoryData
    {
        /// <summary>
        /// 分类id 主键
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// 分类编码
        /// </summary>
        public string CategoryCode { get; set; }
        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// 产品编码
        /// </summary>
        public string InformationCode { get; set; }
        /// <summary>
        /// 签证材料列表
        /// </summary>
        public IList<VisaMaterialData> MaterialDataList { get; set; }
    }

    public class VisaMaterialData
    {
        /// <summary>
        /// 项目Id主键
        /// </summary>
        public int DataId { get; set; }
        /// <summary>
        /// 项目编码
        /// </summary>
        public string DataCode { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string DataName { get; set; }
        /// <summary>
        /// 项目说明
        /// </summary>
        public string DataExplain { get; set; }
        /// <summary>
        /// 是否必须
        /// </summary>
        public int IsNeed { get; set; }
        /// <summary>
        /// 是否模板
        /// </summary>
        public int IsTemplate { get; set; }
        /// <summary>
        /// 产品编码
        /// </summary>
        public string InformationCode { get; set; }
        /// <summary>
        /// 分类编码
        /// </summary>
        public string CategoryCode { get; set; }
        /// <summary>
        /// 是否原件
        /// </summary>
        public int? IsOriginal { get; set; }
        /// <summary>
        /// 材料数量
        /// </summary>
        public int? DataCount { get; set; }
        /// <summary>
        /// 是否退还材料
        /// </summary>
        public int IsBack { get; set; }
        /// <summary>
        /// 材料附件
        /// </summary>
        public IList<VisaMaterialFilesData> MeterialFilesList { get; set; }
    }


    public class VisaMaterialFilesData
    {
        /// <summary>
        /// 签证材料编码
        /// </summary>
        public string DataCode { get; set; }
        /// <summary>
        /// 附件地址
        /// </summary>
        public string FileUrl { get; set; }
        /// <summary>
        /// 附件说明
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 产品编码
        /// </summary>
        public string InformationCode { get; set; }
    }
}
