using Lvy.Models;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Product
{
    public class ProductVModel : BaseVModel
    {
        public ProductVModel()
        {
            if (ProductModel == null)
            {
                ProductModel = new TpProductModel();
            }
            if (ProductPageList == null)
            {
                ProductPageList = new PagedList<TpProductModel>();
            }
        }

        public TpProductModel ProductModel { get; set; }

        public PagedList<TpProductModel> ProductPageList { get; set; }
    }
}