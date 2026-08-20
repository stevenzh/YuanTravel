using Arch.Common.Models;
using System;

namespace Lvy.Visa.VModels
{
    [Serializable]
    public class VisaProductQModel
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int VisaType { get; set; }

        /// <summary>
        /// 同业价
        /// </summary>
        public decimal TradePrice { get; set; }

        /// <summary>
        /// 直客价
        /// </summary>
        public decimal SalePrice { get; set; }

        public string VisaIssuePlace { get; set; }
        public string CountryImgUrl { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }

        public string VisaExpiryDate { get; set; }
        public string ProcessingTime { get; set; }
        public string LongDwellTime { get; set; }
        public string ArrivalNum { get; set; }
        public int MianShi { get; set; }
        public KeyValueBean MianShiValue { get; set; }
    }
}