using System.Collections.Generic;
using Lvy.Models;
using Lvy.Models.CrmDB;

namespace Lvy.VModels.Crm
{
    public class TeamVModel: BaseVModel
    {
        public TeamVModel()
        {
            this.Team = new CrmTeamModel();
        }

        /// <summary>
        /// 部门名称
        /// </summary>	
        public string TeamName { get; set; }
        /// <summary>
        /// 职能
        /// </summary>
        public string DepartType { get; set; }

        public CrmTeamModel Team { get; set; }

        public List<CrmAccountModel> TeamAccounts { get; set; }
        /// <summary>
        /// 职能列表
        /// </summary>
        public IEnumerable<KeyValueBean> DepartList { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public PagedList<CrmTeamModel> PagedModel { get; set; }
        /// <summary>
        /// 财务部门列表
        /// </summary>
        public List<CrmTeamModel> FinanceTeams { get; set; }
    }
}
