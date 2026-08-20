using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lvy.Models.OrderDB;

namespace Lvy.VModels.Op
{
    public class CheckTouristsVModel:BaseVModel
    {
        public CheckTouristsVModel()
        {
            if (TouristsInfo == null)
                TouristsInfo = new TpTravellerModel();
            if (TouristsFile == null || TouristsFile.Count < 0)
                TouristsFile = new List<TpOrderFileModel>();
        }

        /// <summary>
        /// 护照有效期截至
        /// </summary>
        public DateTime PassportExpiry { get; set; }
        /// <summary>
        /// 游客ID
        /// </summary>
        public int TouristsId { get; set; }

        /// <summary>
        /// 游客信息
        /// </summary>
        public TpTravellerModel TouristsInfo { get; set; }

        /// <summary>
        /// 游客资料
        /// </summary>
        public List<TpOrderFileModel> TouristsFile { get; set; }
        public int IsOP { get; set; }
    }
}
