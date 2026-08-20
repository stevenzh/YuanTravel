using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lvy.Models.OrderDB;

namespace Lvy.VModels.Order
{
    public class TpOrderFileVModel
    {

        public TpOrderFileVModel()
        {

            if (OrderFileList == null)
            {
                OrderFileList = new List<TpOrderFileModel>();
            }
        }

        public string SourceType { get; set; }

        public int KeyId { get; set; }

        public string OrderCode { get; set; }

        public string Remark { get; set; }
        public List<TpOrderFileModel> OrderFileList { get; set; }

    }
}
