using Lvy.Models;
using Lvy.Models.BaseDB;

namespace Lvy.VModels.Base
{
    public class FileResVModel : BaseVModel
    {
        #region 查询对象

        public BaseFileResModel QueryModel { get; set; }

        #endregion 查询对象

        /// <summary>
        /// 集合
        /// </summary>
        public PagedList<BaseFileResModel> FileResModels { get; set; }
    }
}