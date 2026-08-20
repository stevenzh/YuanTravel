using Lvy.Models;
using Lvy.Visa.Models;
using Lvy.VModels;

namespace Lvy.Visa.VModels
{
    public class VisaBookingQModel : BaseVModel
    {
        public VisaBookingQModel()
        {
            this.VisaInformationList = new PagedList<VisaInformationModel>();
        }

        public VisaInformationModel visaInformationModel { get; set; }

        public PagedList<VisaInformationModel> VisaInformationList { get; set; }

        #region --查询条件--

        public string InformationCode { get; set; }
        public string InformationName { get; set; }
        public int? VisaType { get; set; }
        public string VisaCountry { get; set; }
        public string LivePassportArea { get; set; }
        public string keyword { get; set; }
        public string VisaIssuePlace { get; set; }
        public string LinqQuValue { get; set; }

        #endregion --查询条件--

        public string SortProperty { get; set; }
        public bool IsAscending { get; set; }
        public string xmlPath { get; set; }
    }
}