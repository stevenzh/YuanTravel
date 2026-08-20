using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lvy.Models;
using Lvy.Models.BaseDB;

namespace Lvy.VModels.Base
{
    public class AirlineVModel
    {

        public AirlineVModel()
        {
            if (AirlineInfo == null)
            {
                AirlineInfo = new BaseAirlineModel();
            }
            if (AirelinePageList == null)
            {
                AirelinePageList = new PagedList<BaseAirlineModel>();
            }
        }

        public BaseAirlineModel AirlineInfo { get; set; }

        public PagedList<BaseAirlineModel> AirelinePageList { get; set; }

    }
}
