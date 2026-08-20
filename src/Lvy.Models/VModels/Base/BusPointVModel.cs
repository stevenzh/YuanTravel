using System.Collections.Generic;
using Lvy.Models;
using Lvy.Models.BaseDB;

namespace Lvy.VModels.Base
{

    public class BusPointVModel : BaseVModel
    {
        #region Query Condition

        /// <summary>
        /// 上车点
        /// </summary>	
        public string BusPoint { get; set; }
        /// <summary>
        /// 组别Id
        /// </summary>
        public string GroupId { get; set; }
        public string OutCity { get; set; }

        #endregion

        /// <summary>
        /// 上车点分页对象
        /// </summary>
        public PagedList<BaseBusPointModel> PagedModel { get; set; }

        public List<BusPointGroupModel> GroupList { get; set; }
    }
}
