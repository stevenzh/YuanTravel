using PetaPoco;
using System;

namespace Lvy.Models.SiteDB
{
    /// <summary>
    /// 网站项目
    /// </summary>
    [TableName("site_navs")]
    [PrimaryKey("NavID")]
    public partial class SiteNavModel
    {
        public int NavID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
        public string OutCity { get; set; }
        public bool IsValid { get; set; }
        public string OwnerCode { get; set; }
    }

    /// <summary>
    /// 网站项目列表（树）
    /// </summary>
    [TableName("site_nav_items")]
    [PrimaryKey("ItemID")]
    public partial class SiteNavItemModel
    {
        public int ItemID { get; set; }
        public string NavCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
        public int Level { get; set; }
        public string LinkUrl { get; set; }
        public string Region { get; set; }

        /// <summary>
        /// 产品类型 1:线路 3:签证 4:酒店 9:其他
        /// </summary>
        public string ProductType { get; set; }

        public int ParentID { get; set; }
        public string OutCity { get; set; }
        public string ImageUrl { get; set; }
        public string WapImageUrl { get; set; }
        public string BgColor { get; set; }
        public string OP { get; set; }
        public bool IsLeaf { get; set; }

        /// <summary>
        /// 是否产品组
        /// </summary>
        public bool IsGroup { get; set; }

        public bool IsValid { get; set; }
        public int SortOrder { get; set; }

        [ResultColumn]
        public string ParentName { get; set; }

        [ResultColumn]
        public SiteNavItemModel ParentNode { get; set; }
        [ResultColumn]
        public string OutCityName { get; set; }
    }

    [TableName("site_nav_list")]
    [PrimaryKey("ListID")]
    public class SiteNavListModel
    {
        public int ListID { get; set; }
        /// <summary>
        /// 父结构ID
        /// </summary>
        public int ItemID { get; set; }
        /// <summary>
        /// 关联产品编号
        /// </summary>
        public string ProductId { get; set; }
        public int SortOrder { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [ResultColumn]
        public string ProductName { get; set; }
        /// <summary>
        /// 销售价
        /// </summary>
        [ResultColumn]
        public decimal SellPrice { get; set; }
        /// <summary>
        /// 签证类型
        /// </summary>
        [ResultColumn]
        public string VTypeValue { get; set; }
        /// <summary>
        /// 签证使用
        /// </summary>
        [ResultColumn]
        public string VisaAreaValue { get; set; }
        /// <summary>
        /// 出发城市（线路）
        /// </summary>
        [ResultColumn]
        public string OurCityName { get; set; }
        /// <summary>
        /// 景区名称（门票）
        /// </summary>
        [ResultColumn]
        public string PlaceName { get; set; }
        /// <summary>
        /// 所在城市（酒店）
        /// </summary>
        [ResultColumn]
        public string CityName { get; set; }
    }

    /// <summary>
    /// 滚动栏图片列表
    /// </summary>
    [TableName("site_banners")]
    [PrimaryKey("BannerID")]
    public class SiteBannerModel
    {
        public int BannerID { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        /// <summary>
        /// 图片路径
        /// </summary>
        public string PicturePath { get; set; }
        public string LinkUrl { get; set; }
        public int SortOrder { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string OwnerCode { get; set; }
    }
}