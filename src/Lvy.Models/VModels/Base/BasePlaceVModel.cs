using Lvy.Models;
using Lvy.Models.BaseDB;

namespace Lvy.VModels.Base
{
    public class BasePlaceVModel : BaseVModel
    {
        #region query condition

        public string PlaceName { get; set; }

        public string PlaceLevel { get; set; }

        #endregion query condition

        public PagedList<BasePlaceModel> PagedList { get; set; }
    }
}