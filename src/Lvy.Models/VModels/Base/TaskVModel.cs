using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.JModels;

namespace Lvy.VModels.Base
{
    public class TaskVModel : BaseVModel
    {
        public TaskVModel()
        {
            if (Task == null)
                Task = new BaseTaskModel();
            if (JsonModel == null)
                JsonModel = new TaskJModel();
            if (TaskPageList == null)
                TaskPageList = new PagedList<BaseTaskModel>();
        }

        public int IsLeader { get; set; }

        public string Note { get; set; }

        /// <summary>
        /// 查询对象
        /// </summary>
        public BaseTaskModel Task { get; set; }

        public TaskJModel JsonModel { get; set; }

        /// <summary>
        /// 集合
        /// </summary>
        public PagedList<BaseTaskModel> TaskPageList { get; set; }
    }
}