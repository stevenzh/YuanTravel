using Lvy.Models;
using Lvy.Models.BaseDB;

namespace Lvy.VModels.Base
{
    public class LogVModel : BaseVModel
    {
        public LogVModel()
        {
            if (BizLog == null)
                BizLog = new BizLogModel();
            if (LogList == null)
                LogList = new PagedList<BizLogModel>();
        }

        /// <summary>
        /// 查询对象
        /// </summary>
        public BizLogModel BizLog { get; set; }

        /// <summary>
        /// 集合
        /// </summary>
        public PagedList<BizLogModel> LogList { get; set; }

        public int IsLeader { get; set; }
    }
}