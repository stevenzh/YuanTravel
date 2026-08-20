using Lvy.Models.CrmDB;
using System.Collections.Generic;

namespace Lvy.VModels.Crm
{
    public class RoleFuncVModel
    {
        /// <summary>
        /// 可选择的角色列表
        /// </summary>
        public List<SysRoleModel> Roles { get; set; }

        /// <summary>
        /// 可选择的功能列表
        /// </summary>
        public List<SysFunctionModel> Functions { get; set; }
    }
}