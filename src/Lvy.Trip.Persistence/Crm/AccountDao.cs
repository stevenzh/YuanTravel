using Lvy.Models.CrmDB;
using PetaPoco;

namespace Lvy.Trip.Dao.Crm
{
    /// <summary>
    ///
    /// </summary>
    public class AccountDao : YuanDbRepository<CrmAccountModel>
    {
        #region 用户角色关联

        /// <summary>
        /// 根据用户code删除关联
        /// </summary>
        public void DelRoleMapByAccountCode(string accountCode)
        {
            string sql = "delete from SysUserRoleMap where AccountCode=@0";
            _repo.Execute(sql, new AnsiString(accountCode));
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="selectedRoleIds"></param>
        public void InsertBatchUserRoleMap(string accountCode, string[] selectedRoleIds)
        {
            if (selectedRoleIds == null || selectedRoleIds.Length <= 0)
                return;

            SysUserRoleMapModel model = null;
            foreach (var selectedRoleId in selectedRoleIds)
            {
                model = new SysUserRoleMapModel()
                {
                    AccountCode = accountCode,
                    RoleId = selectedRoleId
                };
                _repo.Insert(model);
            }
        }

        public void DeleteBatchUserRoleMap(string accountCode, string[] selectedRoleIds)
        {
            if (selectedRoleIds == null || selectedRoleIds.Length <= 0)
                return;

            foreach (var selectedRoleId in selectedRoleIds)
            {
                _repo.Execute("delete from SysUserRoleMap where AccountCode=@0 AND RoleId=@1 ", new AnsiString(accountCode), selectedRoleId);
            }
        }

        #endregion 用户角色关联

        #region 用户组关联

        /// <summary>
        ///
        /// </summary>
        /// <param name="accountCode"></param>
        public void DelTeamMapByAccountCode(string accountCode)
        {
            string sql = "delete from TeamAccountMap where AccountCode=@0";

            _repo.Execute(sql, new AnsiString(accountCode));
        }

        public void InsertBatchUserTeamMap(string accountCode, string[] SelectedTeamIds)
        {
            if (SelectedTeamIds == null || SelectedTeamIds.Length <= 0)
            {
                return;
            }
            TeamAccountMapModel model = null;
            foreach (var SelectedTeamId in SelectedTeamIds)
            {
                model = new TeamAccountMapModel()
                {
                    AccountCode = accountCode,
                    TeamID = SelectedTeamId
                };
                _repo.Insert(model);
            }
        }

        #endregion 用户组关联

        #region 销售关联客户和联系人

        /// <summary>
        /// 更新销售所有客户的部门
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="TeamId"></param>
        public void UpdateCustomerTeam(string accountCode, string TeamId)
        {
            string sql = " update CrmCustomer set TeamID=@0 where SalerCode=@1 and IsValid=1 ";

            _repo.Execute(sql, new AnsiString(TeamId), new AnsiString(accountCode));
        }

        /// <summary>
        /// 跟新销售关联的所有联系人的部门
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="TeamId"></param>
        public void UpdateContactTeam(string accountCode, string TeamId)
        {
            string sql = " update CrmAccount set TeamID=@0 where SalerCode=@1 and IsValid=1 ";

            _repo.Execute(sql, new AnsiString(TeamId), new AnsiString(accountCode));
        }

        #endregion 销售关联客户和联系人
    }
}