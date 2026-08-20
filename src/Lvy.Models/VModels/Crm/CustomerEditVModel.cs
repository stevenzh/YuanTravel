using Lvy.Models.CrmDB;
using System;

namespace Lvy.VModels.Crm
{
    public class CustomerEditVModel : BaseVModel
    {
        public CrmCustomerModel Customer { get; set; }

        [Obsolete]
        public SysPlatformModel Platform { get; set; }
    }
}