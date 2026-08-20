using Lvy.Models.ProductDB;
using System.Collections.Generic;

namespace Lvy.VModels.Product
{
    public class TpLineFileVModel
    {
        public TpLineFileVModel()
        {
            LineFileList = new List<TpLineFileModel>();
        }

        public string LineId { get; set; }

        public string fileName { get; set; }

        public string fileNote { get; set; }
        public string SourceType { get; set; }

        public string FilePath { get; set; }
        public long PhotoId { get; set; }

        public List<TpLineFileModel> LineFileList { get; set; }
    }
}