using Lvy.Models.CrmDB;
using System.Collections.Generic;

namespace Lvy.Trip.Dao.Crm
{
    public class FunctionDao : YuanDbRepository<SysFunctionModel>
    {
        /// <summary>
        /// 获取所有模块名称
        /// </summary>
        /// <returns></returns>
        public List<SysFunctionModel> GetModuleNames()
        {
            return Fetch(" select * from SysFunction where IsValid =1 and FuncType=1 order by Sort DESC  ");
        }

        /// <summary>
        /// 获取所有有效的模块菜单功能数据
        /// </summary>
        /// <returns></returns>
        public List<SysFunctionModel> GetAll()
        {
            return Fetch(" select * from SysFunction where IsValid =1  ");
        }
    }
}