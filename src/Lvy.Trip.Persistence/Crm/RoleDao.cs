using Lvy.Models.CrmDB;
using Lvy.VModels.Crm;
using PetaPoco;
using System.Collections.Generic;

namespace Lvy.Trip.Dao.Crm
{
    public class RoleDao : YuanDbRepository<SysRoleModel>
    {
        /// <summary>
        /// 查询出所有有效的角色
        /// </summary>
        /// <returns></returns>
        public List<SysRoleModel> SearchRole(RoleVModel model)
        {
            Sql sql = new Sql();
            sql.Append("select * from SysRole where OwnerCode=@0 ", model.OwnerCode);
            if (model.IsValid != -1)
                sql.Append(" and IsValid =@0 ", model.IsValid);
            return _repo.Fetch<SysRoleModel>(sql);
        }

        /// <summary>
        /// 是否存在角色名称
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool CheckRoleName(string roleName, string ownerCode)
        {
            string sql = "select count(1) from SysRole where Name=@0 and OwnerCode=@1";
            long cnt = ExecuteScalar<long>(sql, Ansi(roleName), ownerCode);
            return cnt > 0;
        }
    }

    public class SysUserRoleMapDao : YuanDbRepository<SysUserRoleMapModel> { }
}