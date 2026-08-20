using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models.CrmDB;

namespace Lvy.VModels
{
    public class OnlineInfo
    {
        public OnlineInfo()
        {
            OnlineUser = new Dictionary<string, CrmAccountModel>();
        }
        public Dictionary<string, CrmAccountModel> OnlineUser { get; set; }
    }

}
