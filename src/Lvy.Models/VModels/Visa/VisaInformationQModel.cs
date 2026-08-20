using Lvy.Models;
using Lvy.Visa.Models;
using Lvy.VModels;

namespace Lvy.Visa.VModels
{
    public class VisaInformationQModel : BaseVModel
    {
        public VisaInformationQModel()
        {
            this.VisaInformationList = new PagedList<VisaInformationModel>();
        }

        /// <summary>
        /// 签证产品
        /// </summary>
        public VisaInformationModel Info { get; set; }

        /// <summary>
        /// 产品分页
        /// </summary>
        public PagedList<VisaInformationModel> VisaInformationList { set; get; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string InformationCode { get; set; }

        /// <summary>
        /// 当前编辑的Tab
        /// </summary>
        private int _currentTabNum = 0;

        public int CurrentTabNum
        {
            get { return _currentTabNum; }
            set { _currentTabNum = value; }
        }

        /// <summary>
        /// 材料分类编号
        /// </summary>
        public string CategoryCodeNum { get; set; }

        /// <summary>
        /// 类型（个签，团签）
        /// </summary>
        public int VTypeS { get; set; }
    }
}