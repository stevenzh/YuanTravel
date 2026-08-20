using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.CrmDB;
using Lvy.Models.ProductDB;
using System.Collections.Generic;

namespace Lvy.VModels.Product
{
    public enum LineOperationType
    {
        AddLine = 1,
        EditLine = 2,
        CopyLine = 3
    }

    public class AddLineVModel : BaseVModel
    {
        public AddLineVModel()
        {
            this.LineFileVModel = new TpLineFileVModel();
            this.TpLine = new TpLineModel();
        }

        /// <summary>
        /// 操作类型
        /// </summary>
        public LineOperationType OperationType { get; set; }

        /// <summary>
        /// 出发城市
        /// </summary>
        public List<KeyValueBean> OutCities { get; set; }

        /// <summary>
        /// 目的地名称
        /// </summary>
        public string ArriveDestName { get; set; }

        /// <summary>
        /// 线路对象
        /// </summary>
        public TpLineModel TpLine { get; set; }

        /// <summary>
        /// 主题Id数组
        /// </summary>
        public string[] ThemeIds { get; set; }

        /// <summary>
        /// 选择的途径目的地
        /// </summary>
        public string[] SelectedMutliDest { get; set; }

        /// <summary>
        /// 选择的子产品
        /// </summary>
        public string[] SelectedItem { get; set; }

        ///// <summary>
        ///// 线路负责人
        ///// </summary>
        //public TpLineAdminModel LineAdmin { get; set; }

        // public CrmTeamModel Team { get; set; }

        /// <summary>
        /// 是否锁定产品名称
        /// </summary>
        public int LockName { get; set; }

        /// <summary>
        /// 客户联系人账户
        /// </summary>
        public List<KeyValueBean> AccountListBean { get; set; }

        public List<KeyValueBean> MutliDestBeans { get; set; }

        public TpLineFileVModel LineFileVModel { get; set; }

        public List<TpLineRouteModel> TpLineRouteList { get; set; }

        /// <summary>
        /// 签证信息列表
        /// </summary>
        public List<TpLineVisaModel> TpLineVisaList { get; set; }

        /// <summary>
        /// 管理子产品
        /// </summary>
        public List<TpProductModel> LineItemList { get; set; }

        #region 图片选择

        public string CountryCode { get; set; }
        public string CountryName { get; set; }

        /// <summary>
        ///  图片库分页列表  页码
        /// </summary>
        public int ImagePagedIndex { get; set; }

        /// <summary>
        /// 图片库分页列表
        /// </summary>
        public PagedList<PhotoInfoModel> PhotoInfoList { get; set; }

        #endregion 图片选择
    }
}