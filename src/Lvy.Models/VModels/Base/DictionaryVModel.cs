using Lvy.Models.BaseDB;
using System.Collections.Generic;

namespace Lvy.VModels
{
    public class DictionaryVModel
    {
        public BaseDictionaryDetailModel DetailModel { get; set; }

        public List<BaseDictionaryDetailModel> DetailModels { get; set; }
    }
}