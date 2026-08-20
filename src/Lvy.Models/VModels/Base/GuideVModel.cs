using Lvy.Models;
using Lvy.Models.BaseDB;

namespace Lvy.VModels.Base
{
    public class GuideVModel : BaseVModel
    {
        public GuideVModel()
        {
            if (GuideModel == null)
                GuideModel = new GuideModel();
            if (GuidePageList == null)
                GuidePageList = new PagedList<GuideModel>();

            this.SortKey = 1;
        }

        /// <summary>
        /// 联系电话
        /// </summary>
        /// <remarks>用于查询</remarks>
        public string ContactNumber { get; set; }

        public string CrmTeamId { get; set; }

        /// <summary>
        /// 查询对象
        /// </summary>
        public GuideModel GuideModel { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public int SortKey { get; set; }

        /// <summary>
        /// 查询列表
        /// </summary>
        public PagedList<GuideModel> GuidePageList { get; set; }
    }
}