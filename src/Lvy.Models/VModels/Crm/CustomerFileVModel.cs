using Lvy.Models;
using Lvy.Models.CrmDB;

namespace Lvy.VModels.Crm
{
    public class CustomerFileVModel : BaseModel
    {
        public CustomerFileVModel()
        {
            if (CustomerFile == null)
                CustomerFile = new CustomerFileModel();
            if (FilePageList == null)
                FilePageList = new PagedList<CustomerFileModel>();

            this.SortKey = 1;
        }

        public string FileId { get; set; }

        /// <summary>
        /// 查询对象
        /// </summary>
        public CustomerFileModel CustomerFile { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public int SortKey { get; set; }

        /// <summary>
        /// 查询列表
        /// </summary>
        public PagedList<CustomerFileModel> FilePageList { get; set; }
    }
}