using System.Collections.Generic;

namespace Lvy.Visa.Models
{
    public class VisaDataQModel
    {
        /// <summary>
        /// 产品编码
        /// </summary>
        public string InformationCode { get; set; }

        /// <summary>
        /// 产品状态
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 角色  是不是产品录入员
        /// </summary>
        public bool IsAdderRoler { get; set; }

        /// <summary>
        /// 角色 是不是产品经理
        /// </summary>
        public bool IsProductManageRoler { get; set; }

        /// <summary>
        /// 签证材料分类列表
        /// </summary>
        public IList<VisaCategoryModel> CategroyList { get; set; }

        /// <summary>
        /// 材料列表 无分类的
        /// </summary>
        public IList<VisaDataModel> DataList { get; set; }

        /// <summary>
        /// 材料列表 有分类
        /// </summary>
        public IList<VisaDataModel> DataListInCategroy { get; set; }

        /// <summary>
        /// 当前Span
        /// </summary>
        public int CurrentSpanNum { get; set; }

        /// <summary>
        /// 材料分类code
        /// </summary>
        public string CategoryCode { get; set; }

        /// <summary>
        /// 签证材料
        /// </summary>
        public VisaDataModel visaData { get; set; }

        /// <summary>
        /// 是否分类
        /// </summary>
        public int IsCategory { get; set; }

        /// <summary>
        /// 是不是第一次添加分类
        /// </summary>
        public int IsFirst { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 材料分类编号
        /// </summary>
        public string CategoryCodeNum { get; set; }

        /// <summary>
        /// 附件列表
        /// </summary>
        public IList<VisaDataFileModel> FilesList { get; set; }
    }
}