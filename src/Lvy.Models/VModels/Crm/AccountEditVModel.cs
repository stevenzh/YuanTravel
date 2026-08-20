using Lvy.Models;
using Lvy.Models.CrmDB;
using System.Collections.Generic;

namespace Lvy.VModels.Crm
{
    /// <summary>
    /// EditAccount视图模型
    /// </summary>
    public class AccountEditVModel : BaseVModel
    {
        /// <summary>
        /// 当前用户角色
        /// </summary>
        public int IsLeader { get; set; }

        /// <summary>
        /// 变更部门是否 调整客户
        /// </summary>
        public int AsyncCustomer { get; set; }

        public string SalesTeam { get; set; }

        #region 表单对象

        public CrmAccountModel Account { get; set; }

        public string[] SelectedRoleIds { get; set; }

        public string[] SelectedDestIds { get; set; }

        public string[] SelectedTeamIds { get; set; }

        public CrmCustomerModel Customer { get; set; }

        #endregion 表单对象

        public IEnumerable<KeyValueBean> CustomerBeans { get; set; }

        public IEnumerable<KeyValueBean> SexBeans { get; set; }

        public IEnumerable<KeyValueBean> DepartBeans { get; set; }

        public IEnumerable<KeyValueBean> RoleBeans { get; set; }

        //public IEnumerable<KeyValueBean> DestinationBeans { get; set; }

        public IEnumerable<KeyValueBean> AccountTypeBeans { get; set; }

        public IEnumerable<KeyValueBean> AccountTeamBeans { get; set; }
    }
}