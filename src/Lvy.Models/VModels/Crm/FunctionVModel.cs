using Lvy.Models;
using Lvy.Models.CrmDB;
using System.Collections.Generic;

namespace Lvy.VModels.Crm
{
    public class FunctionVModel : BaseVModel
    {
        public FunctionVModel()
        {
            if (Function == null)
                Function = new SysFunctionModel();
            if (Functions == null)
                Functions = new PagedList<SysFunctionModel>();
        }

        #region 查询对象

        public int FunctionId { get; set; }

        public int Type { get; set; }

        #endregion 查询对象

        /// <summary>
        /// 临时菜单名称
        /// </summary>
        public string MenuName { get; set; }

        /// <summary>
        /// 临时模块名称
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// 编辑对象
        /// </summary>
        public SysFunctionModel Function { get; set; }

        public PagedList<SysFunctionModel> Functions { get; set; }

        #region 所属模块

        public IEnumerable<KeyValueBean> ModuleBeans { get; set; }

        #endregion 所属模块
    }
}