using Lvy.Models;
using Lvy.Models.CrmDB;
using System.Collections.Generic;

namespace Lvy.VModels.Base
{
    public class LoginVModel : BaseVModel
    {
        public CrmAccountModel Account { get; set; }

        public string ValidateCode { get; set; }

        public int AutoLogin { get; set; }

        public List<KeyValueBean> Platforms { get; set; }
    }
}