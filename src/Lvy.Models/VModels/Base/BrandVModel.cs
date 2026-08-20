using Lvy.Models;
using Lvy.Models.BaseDB;

namespace Lvy.VModels.Base
{
    public class BrandVModel: BaseVModel
    {
        public BrandVModel()
        {
            if (BrandModel == null)
            {
                BrandModel = new BrandModel();
            }
            if (BrandList == null)
            {
                BrandList = new PagedList<BrandModel>();
            }
        }

        public BrandModel BrandModel { get; set; }

        public PagedList<BrandModel> BrandList { get; set; }
    }
}