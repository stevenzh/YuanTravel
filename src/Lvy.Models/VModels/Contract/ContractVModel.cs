using Lvy.Models;
using System.Collections.Generic;

namespace Lvy.VModels.Contract
{
    public class ContractVModel
    {
        public ContractVModel()
        {
            this.Contract = new ContractInfo();
            this.TouristList = new List<ContractTourist>();
        }

        public ContractInfo Contract { get; set; }

        /// <summary>
        /// 游客列表
        /// </summary>
        public List<ContractTourist> TouristList { get; set; }

        /// <summary>
        /// 附件列表
        /// </summary>
        public List<ContractFiles> FilesList { get; set; }

        /// <summary>
        /// 购物列表
        /// </summary>
        public List<ContractShopping> ShoppingList { get; set; }

        /// <summary>
        /// 自愿付费项目列表
        /// </summary>
        public List<ContractPayItem> PayItemList { get; set; }
    }
}