using Lvy.Models;
using Lvy.Models.CrmDB;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.VModels.Crm
{
    public class HostVModel : BaseVModel
    {
        public HostVModel()
        {
            if (Customer == null)
                Customer = new CrmCustomerModel();
            if (Customers == null)
                Customers = new PagedList<SysPlatformModel>();
            this.SortKey = 2;
        }

        /// <summary>
        /// 排序集合
        /// </summary>
        public Dictionary<int, KeyValueBean> SortCollection
        {
            get
            {
                Dictionary<int, KeyValueBean> dic = new Dictionary<int, KeyValueBean>();
                dic.Add(1, new KeyValueBean { Key = "c.Code ASC", Value = "客户编号升序" });
                dic.Add(2, new KeyValueBean { Key = "c.Code DESC", Value = "客户编号降序" });
                dic.Add(3, new KeyValueBean { Key = "c.Name ASC", Value = "名称升序" });
                dic.Add(4, new KeyValueBean { Key = "c.Name DESC", Value = "名称降序" });
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


        /// <summary>
        /// 审核状态 0：未审核 1：已审核 2：审核不通过
        /// </summary>
        public string CustomerState { get; set; }

        /// <summary>
        /// 查询对象
        /// </summary>
        public CrmCustomerModel Customer { get; set; }

        /// <summary>
        /// 显示列表
        /// </summary>
        public PagedList<SysPlatformModel> Customers { get; set; }
    }
}