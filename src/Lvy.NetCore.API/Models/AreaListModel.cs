using Lvy.Models.BaseDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lvy.NetCore.API.Models
{
    public class AreaListModel
    {

        public string ReturnMsg { get; set; }
        public IEnumerable<BaseDestinationModel> List { get; set; }

        public int TotalCount { get; set; }

    }
}
