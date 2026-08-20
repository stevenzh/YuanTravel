using Lvy.Models.ProductDB;
using System.Collections.Generic;

namespace Lvy.VModels.Product
{
    public class LineCostRuleVModel : BaseVModel
    {
        public TpLineModel TpLine { get; set; }

        public List<TpLineCostRuleModel> CostModels { get; set; }
    }
}