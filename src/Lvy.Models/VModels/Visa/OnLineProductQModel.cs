using Lvy.Visa.Models;
using System.Collections.Generic;

namespace Lvy.Visa.VModels
{
    public class OnLineProductQModel
    {
        /// <summary>
        /// 签证产品列表
        /// </summary>
        public IList<VisaInformationModel> ProductModels { get; set; }

        /// <summary>
        /// 签证产品信息
        /// </summary>
        public VisaInformationModel VisaModel { get; set; }

        /// <summary>
        /// 签证国家
        /// </summary>
        public string VisaCountry { get; set; }

        /// <summary>
        /// 签证类型
        /// </summary>
        public string VisaType { get; set; }

        /// <summary>
        /// 洲
        /// </summary>
        public string Continent { get; set; }

        /// <summary>
        /// 领区
        /// </summary>
        public string VisaArea { get; set; }

        /// <summary>
        /// 护照签发地
        /// </summary>
        public string PassPortIssueAt { get; set; }

        /// <summary>
        /// 签证材料分类列表
        /// </summary>
        public List<VisaCategoryModel> VisaCategoryModels { get; set; }

        public List<VisaDataModel> VisaDataModels { get; set; }

        /// <summary>
        /// 签证材料附件列表
        /// </summary>
        public List<VisaDataFileModel> VisaDataFileModels { get; set; }

        /// <summary>
        /// 签证材料分类
        /// </summary>
        public VisaCategoryModel VisaCategoryModel { get; set; }

        /// <summary>
        /// 浏览历史
        /// </summary>
        public IDictionary<string, string> BrowserHistory { get; set; }

        public string Binding { get; set; }
    }
}