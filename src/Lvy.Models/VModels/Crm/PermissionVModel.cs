using Lvy.Models;
using Lvy.Models.CrmDB;
using System.Collections.Generic;

namespace Lvy.VModels.Crm
{
    public class PermissionVModel : BaseVModel
    {
        /// <summary>
        /// 角色组
        /// </summary>
        public List<KeyValueBean> RoleBeans { get; set; }

        /// <summary>
        /// 功能列表
        /// </summary>
        public List<SysFunctionModel> Functions { get; set; }

        #region PostForm

        public int RoleId { get; set; }

        public string[] FuncIds { get; set; }

        #endregion PostForm
    }
}