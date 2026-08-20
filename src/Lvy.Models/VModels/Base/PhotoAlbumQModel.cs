using Lvy.Models;
using Lvy.Models.BaseDB;
using System.Collections.Generic;

namespace Lvy.VModels.Base
{
    public class PhotoAlbumQModel
    {
        /// <summary>
        /// 区域编号
        /// </summary>
        public long areaId { get; set; }

        public string AreaName { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public long areaId1 { get; set; }

        public string AreaName1 { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        public long areaId2 { get; set; }

        public string AreaName2 { get; set; }

        public PhotoInfoModel photoInfo { get; set; }
        //public List<AreaInfoModel> modelList { get; set; }

        public PagedList<PhotoInfoModel> PhotoPageList { get; set; }
        public List<PhotoInfoModel> PhotoList { get; set; }

        /// <summary>
        /// 国内，国外景
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// 当前页面
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每一页显示的数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页面
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// 当前相册
        /// </summary>
        public int PhotoId { get; set; }

        public PhotoAlbumModel Model { get; set; }

        public IList<PhotoAlbumModel> List { get; set; }

        public long Total { get; set; }
    }
}