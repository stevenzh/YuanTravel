using Lvy.Models.BaseDB;
using System.Collections.Generic;

namespace Lvy.VModels.Ticket
{
    /// <summary>
    /// 门票前台首页
    /// </summary>
    public class IndexVModel : BaseVModel
    {
        /// <summary>
        /// 公告栏信息
        /// </summary>
        public List<BaseArticleModel> Notices { get; set; }

        /// <summary>
        /// 特惠
        /// </summary>
        public List<TktProductVModel> TeHuiList { get; set; }

        /// <summary>
        /// 推荐
        /// </summary>
        public List<TktProductVModel> TuiJianList { get; set; }

    }
}