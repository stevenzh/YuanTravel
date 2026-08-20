using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Trip.Dao.Base;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Crm
{
    /// <summary>
    /// 领队
    /// </summary>
    public class GuideBiz : BaseBiz
    {
        private readonly GuideDao _dao = new GuideDao();

        /// <summary>
        /// 得到一个账户对象
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public GuideModel GetById(int Id)
        {
            return _dao.GetById(Id);
        }

        /// <summary>
        /// 导陪页面显示
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<GuideModel> GetPagedList(GuideVModel vModel)//显示插叙自己建的实体类多添加了一条列但是修改删除都是用的跟数据库对应的实体类
        {
            Sql sql = new Sql();
            sql.Append(@" select c.*, ct.TeamName from BaseGuides c
left join CrmTeam ct on ct.TeamID= c.TeamID
 where c.OwnerCode=@0", vModel.OwnerCode);

            if (!vModel.GuideModel.Name.IsNullOrEmpty())
                sql.Append(" and c.Name like @0", AnsiLike(vModel.GuideModel.Name));

            if (!vModel.GuideModel.TeamID.IsNullOrEmpty())
                sql.Append(" and c.TeamID=@0", Ansi(vModel.GuideModel.TeamID));

            if (!vModel.GuideModel.Tel.IsNullOrEmpty())
                sql.Append(" and c.Tel =@0", vModel.GuideModel.Tel);

            return _dao.Pager<GuideModel>(vModel.GuidePageList.PageIndex, vModel.GuidePageList.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public bool DeleteGuide(int Id)
        {
            int m = _dao.Delete(Id);
            return m > 0;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(GuideModel model)
        {
            return _dao.Update(model);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public object Add(GuideModel vModel)
        {
            return _dao.Insert(vModel);
        }

        public List<GuideModel> GetGuideList(string ownerCode)
        {
            Sql sql = new Sql();
            sql.Append(" select * from BaseGuides where OwnerCode=@0", ownerCode);

            return _dao.Query(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 修改查询根据ID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public GuideModel GetGuideById(int Id)
        {
            Sql sql = new Sql();
            sql.Append(@"select * from BaseGuides where Id=@0", Id);
            return _dao.Query(sql.SQL, sql.Arguments).FirstOrDefault();
        }
    }
}