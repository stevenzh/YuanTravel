using Lvy.Models;
using Lvy.Models.BaseDB;

namespace Lvy.VModels.Base
{
    public class FileVModel : BaseVModel
    {
        public FileVModel()
        {
            this.QueryModel = new BaseFileModel();
            this.FileModels = new PagedList<BaseFileModel>();
        }

        public BaseFileModel QueryModel { get; set; }

        /// <summary>
        /// 集合
        /// </summary>
        public PagedList<BaseFileModel> FileModels { get; set; }
    }
}