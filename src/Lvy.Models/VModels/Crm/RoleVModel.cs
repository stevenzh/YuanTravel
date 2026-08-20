using Lvy.Models.CrmDB;
using System.Collections.Generic;

namespace Lvy.VModels.Crm
{
    public class RoleVModel
    {
        public RoleVModel()
        {
            this.Role = new SysRoleModel();
            this.IsValid = -1;
        }

        public string OwnerCode { get; set; }
        public int IsValid { get; set; }

        /// <summary>
        /// 保存对象
        /// </summary>
        public SysRoleModel Role { get; set; }

        /// <summary>
        /// 列表集合
        /// </summary>
        public List<SysRoleModel> Roles { get; set; }
    }
}