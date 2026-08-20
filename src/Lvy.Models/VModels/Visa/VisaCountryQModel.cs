using Lvy.Models;
using Lvy.Visa.Models;
using Lvy.VModels;
using System.Collections.Generic;

namespace Lvy.Visa.VModels
{
    public class VisaCountryQModel : BaseVModel
    {
        public string BanKuaiKey { get; set; }
        public string BanKuaiValue { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public int AreaId { get; set; }
        public string Id { get; set; }

        public VisaCountryInfoModel VisaCountryModel { get; set; }
        public IList<VisaCountryConsularDistrictModel> LingQuList { get; set; }
        public PagedList<VisaCountryQuestionModel> CountryQuestionPagedList { get; set; }
    }
}