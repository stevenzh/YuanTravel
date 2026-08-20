using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Dao.Crm;
using Lvy.VModels.Crm;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Crm
{
    /// <summary>
    /// 部门管理
    /// </summary>
    public class TeamBiz : BaseBiz
    {
        private readonly TeamDao _dao = new TeamDao();

        /// <summary>
        /// 获取部门信息
        /// </summary>
        /// <param name="teamID">上车点Id</param>
        /// <returns></returns>
        public CrmTeamModel GetTeam(string teamID)
        {
            return _dao.SingleOrDefault(@"SELECT * FROM CrmTeam WHERE TeamID=@0", teamID);
        }

        /// <summary>
        /// 获取部门
        /// </summary>
        /// <param name="ownerCode">所属商户</param>
        /// <returns></returns>
        public List<CrmTeamModel> GetTeamsList(string ownerCode)
        {
            return _dao.Fetch(@"SELECT * FROM CrmTeam WHERE OwnerCode=@0 and IsValid=1 ORDER BY TeamName, IsValid DESC, TeamID", ownerCode);
        }

        /// <summary>
        /// 新增部门
        /// </summary>
        /// <param name="model">部门实体</param>
        /// <returns></returns>
        public int AddTeam(CrmTeamModel model)
        {
            return _dao.Insert(model).ToInt();
        }

        /// <summary>
        /// 编辑部门
        /// </summary>
        /// <param name="model">部门实体</param>
        /// <returns></returns>
        public int UpdateTeam(CrmTeamModel model)
        {
            return _dao.Update(model);
        }

        /// <summary>
        /// 获取部门分页对象
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<CrmTeamModel> GetPagedTeam(TeamVModel vModel)
        {
            var sql = new Sql();
            sql.Append(@"SELECT * FROM CrmTeam WHERE OwnerCode=@0", vModel.OwnerCode);
            if (!vModel.TeamName.IsNullOrEmpty())
                sql.Append(@" AND TeamName LIKE @0", AnsiLike(vModel.TeamName));
            if (!vModel.DepartType.IsNullOrEmpty())
                sql.Append(@" AND DepartCode=@0", vModel.DepartType);
            sql.Append(@" ORDER BY IsValid DESC, TeamName");
            return _dao.Pager(vModel.PagedModel.PageIndex, vModel.PagedModel.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 职能编码: 1、综合部 2、计调部 5、销售部 6、签证部 7、挂靠部 9、财务部
        /// </summary>
        /// <param name="departCode">职能</param>
        /// <returns></returns>
        public List<CrmTeamModel> GetTeams(string departCode, string ownerCode)
        {
            var sql = new Sql();
            sql.Append("select * from CrmTeam where OwnerCode=@0 and IsValid=1 and DepartCode=@1", Ansi(ownerCode), Ansi(departCode));

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 取得所有销售组
        /// </summary>
        /// <returns></returns>
        public List<CrmTeamModel> GetSalesTeams(string ownerCode)
        {
            var sql = new Sql();
            sql.Append("SELECT * FROM CrmTeam WHERE OwnerCode=@0 AND IsValid=1 AND (DepartCode='1' OR DepartCode='5') ORDER BY TeamName ",
                 Ansi(ownerCode));

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 取得所有操作组
        /// </summary>
        /// <returns></returns>
        public List<CrmTeamModel> GetOpTeams(string ownerCode)
        {
            var sql = new Sql();
            sql.Append("SELECT * FROM CrmTeam WHERE OwnerCode=@0 AND IsValid=1 AND (DepartCode='1' OR DepartCode='2' OR DepartCode='6') ORDER BY TeamName ",
                 Ansi(ownerCode));

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }
        /// <summary>
        /// 取得所有团单组
        /// </summary>
        /// <returns></returns>
        public List<CrmTeamModel> GetBalanceTeams(string ownerCode)
        {
            var sql = new Sql();
            sql.Append("SELECT * FROM CrmTeam WHERE OwnerCode=@0 AND IsValid=1 AND (DepartCode='1' OR DepartCode='2' OR DepartCode='6' OR DepartCode='7') ORDER BY TeamName ",
                 Ansi(ownerCode));

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        public CrmAccountModel GetTeamLeader(string ownerCode, string TeamID)
        {
            var sql = new Sql();
            sql.Append(@"SELECT ca.* FROM CrmAccount ca, CrmTeam ct
WHERE ca.Code=ct.LeaderCode AND ca.IsValid=1 AND ca.OwnerCode=@0 AND ct.TeamID=@1", ownerCode, TeamID);

            return _dao.Query<CrmAccountModel>(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        public List<CrmTeamModel> HasSalesTeam(string[] teamId, string ownerCode)
        {
            var sql = new Sql();
            sql.Append(" SELECT * FROM CrmTeam WHERE OwnerCode=@0 AND IsValid=1 AND DepartCode=5 AND TeamID IN ( @1)",
                 Ansi(ownerCode), teamId);

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }
    }
}