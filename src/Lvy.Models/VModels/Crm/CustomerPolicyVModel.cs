using Lvy.Models;
using Lvy.Models.CrmDB;
using System.Collections.Generic;

namespace Lvy.VModels.Crm
{
    public class CustomerPolicyVModel
    {
        public CustomerPolicyVModel()
        {
            this.PolicyEntity = new CustomerPolicyModel();
        }

        public string CustomerCode { get; set; }

        public string RegionName { get; set; }

        public CustomerPolicyModel PolicyEntity { get; set; }
        public List<CustomerPolicyModel> Items { get; set; }
        public List<KeyValueBean> RebateBeans { get; set; }
    }
}