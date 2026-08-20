using Lvy.Models;
using Lvy.Models.CrmDB;
using System.Collections.Generic;

namespace Lvy.VModels.Crm
{
    public class AuditAccountEditVModel : BaseVModel
    {
        public List<KeyValueBean> SexBeans { get; set; }
        public List<KeyValueBean> CustomerBeans { get; set; }
        public List<KeyValueBean> DepartBeans { get; set; }
        public CrmAccountModel Account { get; set; }
        public CrmCustomerModel Customer { get; set; }

        /// <summary>
        /// 客户注册信息
        /// </summary>
        public CustomerRegistrationModel Registration { get; set; }
    }
}