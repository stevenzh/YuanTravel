using Lvy.Models;
using System.Collections.Generic;

namespace Lvy.VModels.Contract
{
    public class AdditionsVModel
    {
        public string content { get; set; }

        public List<ContractAdditions> List { get; set; }
    }
}