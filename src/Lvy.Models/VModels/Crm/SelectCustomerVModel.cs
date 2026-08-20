using Lvy.Models;
using Lvy.Models.CrmDB;

namespace Lvy.VModels.Crm
{
    public class SelectCustomerVModel : BaseVModel
    {
        /// <summary>
        /// 查询关键字
        /// </summary>
        public string KeyWord { get; set; }

        /// <summary>
        ///
        /// </summary>
        public PagedList<CrmCustomerModel> PagedCustomers { get; set; }
    }
}