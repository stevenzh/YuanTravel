using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Trip.Biz;

namespace CCT.Visa.Service
{
    public class VisaCountryService : BaseBiz
    {
        VisaCountryQuestionDao _dao = new VisaCountryQuestionDao();

        public PagedList<VisaCountryQuestionModel> SearchCountryQuestion(string countryCode, int index, int pagesize)
        {
            Sql sql = new Sql();
            sql.Append(@" select from Visa_CountryQuestion (nolock) 
                            inner join BaseDestination (nolock) on Visa_CountryQuestion.CountryCode = BaseDestination.ParentStr ")
              .Append(" where Visa_CountryQuestion.CountryCode=@0 ", countryCode);


            return _dao.Pager(index, pagesize, sql.SQL, sql.Arguments);
        }
        public VisaCountryInfoModel GetVisaCountryInfo(string countryCode)
        {
            return (new VisaCountryInfoDao()).GetVisaCountryInfo(countryCode);
        }
        public IList<VisaCountryConsularDistrictModel> GetCountryConsularDistrictList(string countryCode, string path)
        {
            var lingqu = (new VisaCountryConsularDistrictDao()).GetCountryConsularDistrictList(countryCode);
            var xmlTools = new XMLTools(path);
            foreach (var lqModel in lingqu)
            {
                lqModel.ConsularDistrictValue = xmlTools.GetDictionary("VisaArea", lqModel.ConsularDistrictKey);
                lqModel.ProductList = (new VisaInformationDao().Search(countryCode, Convert.ToInt32(lqModel.ConsularDistrictKey)));
                foreach (var productModel in lqModel.ProductList)
                {
                    productModel.MianShiValue = xmlTools.GetDictionary("InterviewType", productModel.MianShi.ToString());
                }
            }
            return lingqu;
        }
        public AreaInfoModel GetDestinationInfo(long areaId)
        {
            return (new AreaInfoDao()).GetDetailById(areaId);
        }
    }
}
