using Lvy.Models.CrmDB;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Online
{
    public class BusinessCardVModel : BaseVModel
    {
        /// <summary>
        /// 专线供应商专管员
        /// </summary>
        public TpLineAdminModel CustomerAdmin { get; set; }

        /// <summary>
        /// 专线供应商专管员账号
        /// </summary>
        public CrmAccountModel CustomerAccount { get; set; }

        /// <summary>
        /// 平台供应商专管员
        /// </summary>
        public TpLineAdminModel PlatAdmin { get; set; }

        /// <summary>
        /// 平台供应商专管员账号
        /// </summary>
        public CrmAccountModel PlatAccount { get; set; }
    }
}