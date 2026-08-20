using Lvy.Models;
using Lvy.Visa.Models;
using Lvy.VModels;

namespace Lvy.Visa.VModels
{
    public class VisaCountryQuestionQModel : BaseVModel
    {
        public VisaCountryQuestionQModel()
        {
            this.QuetionList = new PagedList<VisaCountryQuestionModel>();
        }
        public PagedList<VisaCountryQuestionModel> QuetionList { set; get; }
        public VisaCountryQuestionModel QuestionModel { get; set; }
    }
}