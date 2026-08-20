using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.VModels;
using System.Collections.Generic;

namespace Lvy.Visa.Models
{
    public class CountryConsularDistrictQModel : BaseVModel
    {
        public CountryConsularDistrictQModel()
        {
            this.countryPagedList = new PagedList<VisaCountryInfoModel>();
        }
        /// <summary>
        /// 国家分页列表
        /// </summary>
        public PagedList<VisaCountryInfoModel> countryPagedList { set; get; }

        /// <summary>
        /// 领区分页列表
        /// </summary>
        public PagedList<VisaCountryConsularDistrictModel> ConsularDistrictPagedList { set; get; }

        /// <summary>
        /// 领区分页列表
        /// </summary>
        public IList<VisaCountryConsularDistrictModel> ConsularDistrictList { set; get; }

        /// <summary>
        /// 领区model
        /// </summary>
        public VisaCountryConsularDistrictModel model { get; set; }

        /// <summary>
        ///  国家model
        /// </summary>
        public VisaCountryInfoModel country { get; set; }

        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        /// <summary>
        /// 图片库分页列表
        /// </summary>
        public PagedList<PhotoInfoModel> PhotoInfoList { get; set; }

        /// <summary>
        ///  图片库分页列表  页码
        /// </summary>
        public int ImagePagedIndex { get; set; }

        public string VisaCountryParentStr { get; set; }
    }
}