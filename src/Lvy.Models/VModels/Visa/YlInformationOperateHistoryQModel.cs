using Lvy.Visa.Models;
using Lvy.VModels;
using System.Collections.Generic;

namespace Lvy.Visa.VModels
{
    public class YlInformationOperateHistoryQModel : BaseVModel
    {
        /// <summary>
        /// 产品操作历史列表
        /// </summary>
        public IList<VisaInformationOperateHistoryModel> OperateHistoryModels { get; set; }

        /// <summary>
        /// 产品编号
        /// </summary>
        public string InformationCode { get; set; }
    }
}