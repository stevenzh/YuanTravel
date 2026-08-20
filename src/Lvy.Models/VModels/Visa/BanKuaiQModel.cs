using System;
using System.Collections.Generic;

namespace Lvy.Visa.VModels
{
    [Serializable]
    public class BanKuaiQModel
    {
        public string BanKuaiKey { get; set; }
        public string BanKuaiValue { get; set; }
        public IList<VisaCountryQModel> CountryList { get; set; }
    }
}