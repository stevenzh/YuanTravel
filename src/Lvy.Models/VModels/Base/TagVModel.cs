using Lvy.Models;
using Lvy.Models.BaseDB;

namespace Lvy.VModels.Base
{
    public class TagVModel : BaseVModel
    {
        public TagVModel()
        {
            if (TagModel == null)
            {
                TagModel = new BaseTagModel();
            }
            if (TagPagedList == null)
            {
                TagPagedList = new PagedList<BaseTagModel>();
            }
        }

        public BaseTagModel TagModel { get; set; }

        public PagedList<BaseTagModel> TagPagedList { get; set; }
    }
}