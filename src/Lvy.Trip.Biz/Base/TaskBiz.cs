using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Trip.Dao.Base;
using Lvy.VModels.Base;
using PetaPoco;
using System;
using System.Collections.Generic;

namespace Lvy.Trip.Biz.Base
{
    /// <summary>
    /// 公告
    /// </summary>
    public class TaskBiz : BaseBiz
    {
        private BaseTaskDao _dao = new BaseTaskDao();

        /// <summary>
        /// 获取公告集合
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<BaseTaskModel> GetPageList(TaskVModel vModel)
        {
            var sql = new Sql();
            sql.Append("select * from BaseTasks where OwnerCode=@0 ", vModel.Task.OwnerCode);

            if (!vModel.Task.Originator.IsNullOrEmpty())
                sql.Append(" and Originator=@0 ", Ansi(vModel.Task.Originator));
            if (!vModel.Task.TeamID.IsNullOrEmpty())
                sql.Append(" and ( TeamID=@0 or WorkmanTeam=@0 ) ", Ansi(vModel.Task.TeamID));

            sql.Append(" order by CreatedTime DESC ");

            var list = _dao.Pager(vModel.TaskPageList.PageIndex, vModel.TaskPageList.PageSize, sql.SQL, sql.Arguments);
            return list;
        }

        /// <summary>
        /// 获取公告前*条
        /// </summary>
        /// <param name="topCount"></param>
        /// <returns></returns>
        public List<BaseTaskModel> GetSubTasks(long taskId)
        {
            var sql = new Sql();
            sql.Append(@"SELECT * FROM BaseTasks WHERE ParentID=@0 ", taskId);
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        public List<BaseTaskModel> GetTaskList(int top, string userCode)
        {
            var sql = new Sql();
            sql.Append(@"select * from BaseTasks where Status=0 and ( Workman=@0 or Originator=@0) ", userCode);
            sql.Append(" order by CreatedTime DESC LIMIT " + top);

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 添加公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public long AddTask(BaseTaskModel model)
        {
            return _dao.Insert(model).ToInt();
        }

        /// <summary>
        /// 更新公告
        /// </summary>
        /// <param name="noticeModel"></param>
        /// <returns>返回1：true 0:false</returns>
        public int UpdateTask(BaseTaskModel noticeModel)
        {
            return _dao.Update(noticeModel);
        }

        /// <summary>
        /// 获取一个角色对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseTaskModel GetByTaskId(long id)
        {
            return _dao.GetById(id);
        }
    }
}