using Lvy.Models;
using Lvy.Models.CrmDB;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.VModels.Crm
{
    public class AccountVModel : BaseVModel
    {
        public AccountVModel()
        {
            if (Account == null)
                Account = new CrmAccountModel();
            if (Accounts == null)
                Accounts = new PagedList<CrmAccountModel>();

            this.SortKey = 2;
            this.FirstTime = true;
        }

        /// <summary>
        /// 联系电话
        /// </summary>
        /// <remarks>用于查询</remarks>
        public string ContactNumber { get; set; }

        /// <summary>
        /// 销售部门
        /// </summary>
        public string CrmTeamId { get; set; }

        /// <summary>
        /// 关联销售
        /// </summary>
        public string SalesCode { get; set; }

        /// <summary>
        /// 用于区分内部员工和外部联系人
        /// 1：内部员工 0:外部用户
        /// </summary>
        public int IsEmployee { get; set; }

        /// <summary>
        /// 当前用户角色
        /// </summary>
        public int IsLeader { get; set; }

        /// <summary>
        /// 查询对象
        /// </summary>
        public CrmAccountModel Account { get; set; }

        /// <summary>
        /// 排序集合
        /// </summary>
        public Dictionary<int, KeyValueBean> SortCollection
        {
            get
            {
                Dictionary<int, KeyValueBean> dic = new Dictionary<int, KeyValueBean>();
                dic.Add(1, new KeyValueBean { Key = "ca.Code ASC", Value = "编号升序" });
                dic.Add(2, new KeyValueBean { Key = "ca.Code DESC", Value = "编号降序" });
                dic.Add(3, new KeyValueBean { Key = "ca.Name ASC", Value = "姓名升序" });
                dic.Add(4, new KeyValueBean { Key = "ca.Name DESC", Value = "姓名降序" });

                return dic;
            }
        }

        /// <summary>
        /// 排序键值对
        /// </summary>
        public List<KeyValueBean> SortKeyValueBean
        {
            get
            {
                return SortCollection.Select(dic => new KeyValueBean { Key = dic.Key.ToString(), Value = dic.Value.Value }).ToList();
            }
        }

        /// <summary>
        /// 排序方式
        /// </summary>
        public int SortKey { get; set; }

        public IEnumerable<KeyValueBean> SexBeans { get; set; }

        /// <summary>
        /// 查询列表
        /// </summary>
        public PagedList<CrmAccountModel> Accounts { get; set; }

        public bool FirstTime { get; set; }
    }
}