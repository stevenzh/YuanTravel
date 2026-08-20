using Lvy.Models.BaseDB;
using System.Collections.Generic;

namespace Lvy.VModels
{
    public class DestinationVModel : BaseVModel
    {
        public DestinationVModel()
        {
            if (DestModel == null)
                DestModel = new BaseDestinationModel();
        }

        #region 查询对象

        public string Name { get; set; }

        public string TreeId { get; set; }

        #endregion 查询对象

        public BaseDestinationModel DestModel { get; set; }

        /// <summary>
        /// 列表
        /// </summary>
        public List<BaseDestinationModel> DestModels { get; set; }
    }
}