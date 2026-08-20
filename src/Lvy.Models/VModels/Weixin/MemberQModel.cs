using Lvy.Models;
using Lvy.Models.WeixinDB;
using System;
using System.Collections.Generic;

namespace Lvy.VModels.Weixin
{
    [Serializable]
    public class MemberQModel : BaseVModel
    {
        public MemberQModel()
        {
            this.MemberPageList = new PagedList<Member>();
        }

        public string OpenID { get; set; }
        public string Name { get; set; }
        public string Sales { get; set; }
        public string CustomerName { get; set; }

        //public string Subscribe { get; set; }
        public string Binding { get; set; }

        public string Approved { get; set; }

        /// <summary>
        /// 是否公司员工
        /// </summary>
        public string Employee { get; set; }

        public IList<Member> MemberList { set; get; }
        public PagedList<Member> MemberPageList { get; set; }

        public List<MemberLocation> Locations { get; set; }
    }
}