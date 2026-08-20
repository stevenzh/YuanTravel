using Lvy.Models;
using Lvy.Models.SiteDB;
using System;
using System.Collections.Generic;

namespace Lvy.VModels.Online
{
    [Serializable]
    public class NavQModel : BaseVModel
    {
        public NavQModel()
        {
            this.SiteNavModel = new SiteNavModel();
            this.NavModel = new SiteNavItemModel();

            this.NavList = new List<SiteNavModel>();
            this.NavItemList = new List<SiteNavModel>();
            this.PageList = new PagedList<SiteNavModel>();
        }

        public string Name { get; set; }
        public List<SiteNavModel> NavItemList { set; get; }
        public SiteNavModel SiteNavModel { get; set; }

        public PagedList<SiteNavModel> PageList { set; get; }
        public List<SiteNavModel> NavList { set; get; }
        public SiteNavItemModel NavModel { get; set; }
    }

    /// <summary>
    /// 微网站使用
    /// </summary>
    public class NavSearchVModel
    {
        public int ParentID { get; set; }
        public int ItemID { get; set; }
        public string Title2 { get; set; }
        public string ImgUrl { get; set; }
        public string OutCity { get; set; }
        public string Words { get; set; }
        public string Region { get; set; }
    }

    public class NavItemVModel
    {
        public SiteNavItemModel NavItem { get; set; }

        public List<SiteNavListModel> NavList { get; set; }
        public List<HotTourVModel> LineList { get; set; }
    }
}