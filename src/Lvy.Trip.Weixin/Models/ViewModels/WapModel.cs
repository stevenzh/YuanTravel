using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models.SiteDB;

namespace Lvy.Trip.Weixin.Models
{
    public class WapModel
    {
        public WapModel()
        {
        }
        public int Code { get; set; }
        public string Title { get; set; }
        public IList<SiteNavItemModel> NavList { set; get; }
    }
}
