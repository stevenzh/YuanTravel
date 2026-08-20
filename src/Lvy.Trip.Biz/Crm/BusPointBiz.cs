using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;
using PetaPoco;
using Lvy.Models;
using Lvy.Trip.Dao.Crm;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using Lvy.Models.BaseDB;
using Lvy.Models.CrmDB;

namespace Lvy.Trip.Biz.Crm
{
    /// <summary>
    /// 上车点管理业务处理
    /// </summary>
    public class BusPointBiz : BaseBiz
    {
        private readonly BusPointDao _dao = new BusPointDao();
        private readonly BusPointGroupDao _groupDao = new BusPointGroupDao();

        #region 上车点管理

        /// <summary>
        /// 获取上车点信息
        /// </summary>
        /// <param name="busPointId">上车点Id</param>
        /// <returns></returns>
        public BaseBusPointModel GetBusPoint(int busPointId)
        {
            return _dao.SingleOrDefault("SELECT * FROM BaseBusPoint WHERE Id=@0", busPointId);
        }

        /// <summary>
        /// 获取上车点列表数据
        /// </summary>
        /// <param name="ownerCode">所属商户</param>
        /// <returns></returns>
        public List<BaseBusPointModel> GetBusPointsList(string ownerCode)
        {
            return _dao.Fetch("SELECT * FROM BaseBusPoint WHERE OwnerCode=@0 ORDER BY IsValid DESC, BusPoint, Id", ownerCode);
        }

        /// <summary>
        /// 获取上车点列表数据
        /// </summary> 
        /// <param name="groupId">组别Id</param>
        /// <returns></returns>
        public List<BaseBusPointModel> GetBusPointByGroup(string OutCity, string groupId, string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@"SELECT * FROM BaseBusPoint WHERE IsValid=1 AND OwnerCode=@0", ownerCode);
            if (!string.IsNullOrEmpty(OutCity))
                sql.Append(@" AND OutCity = @0", Ansi(OutCity));
            if (!groupId.IsNullOrEmpty())
                sql.Append(@" and GroupId LIKE @0", AnsiLike(groupId));
            sql.Append(@" ORDER BY IsValid DESC, BusPoint");
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 新增上车点
        /// </summary>
        /// <param name="model">上车点信息实体</param>
        /// <returns></returns>
        public int AddBusPoint(BaseBusPointModel model)
        {
            return _dao.Insert(model).ToInt();
        }

        /// <summary>
        /// 编辑上车点
        /// </summary>
        /// <param name="model">上车点信息实体</param>
        /// <returns></returns>
        public int UpdateBusPoint(BaseBusPointModel model)
        {
            return _dao.Update(model);
        }

        /// <summary>
        /// 获取上车点分页对象
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<BaseBusPointModel> GetPagedBusPoint(BusPointVModel vModel)
        {
            var sql = new Sql();
            sql.Append(@"SELECT * FROM BaseBusPoint WHERE OwnerCode=@0", vModel.OwnerCode);
            if (!vModel.BusPoint.IsNullOrEmpty())
                sql.Append(@" AND BusPoint LIKE @0", AnsiLike(vModel.BusPoint));
            if (!vModel.GroupId.IsNullOrEmpty())
                sql.Append(@" AND GroupId LIKE @0", AnsiLike(vModel.GroupId));
            if (!vModel.OutCity.IsNullOrEmpty())
                sql.Append(@" AND OutCity = @0", Ansi(vModel.OutCity));
            sql.Append(@" ORDER BY IsValid DESC, BusPoint");
            return _dao.Pager(vModel.PagedModel.PageIndex, vModel.PagedModel.PageSize, sql.SQL, sql.Arguments);
        }

        #endregion

        #region 组别管理

        /// <summary>
        /// 获取有效上车点组别对象
        /// </summary>
        /// <returns></returns>
        public List<BusPointGroupModel> GetGroupItems(string outCity, string ownerCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT bpg.*, bd.`Value` OutCityName FROM BusPointGroup bpg
INNER JOIN basedictionarydetail bd ON bd.`Key` = bpg.OutCity AND bd.Name='OutCityEnum'
WHERE bpg.IsValid=1 AND bpg.OwnerCode=@0 ", ownerCode);
            if (!string.IsNullOrEmpty(outCity))
                sql.Append(" AND bpg.OutCity=@0 ", Ansi(outCity));

            return _groupDao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 获取组别键值对
        /// </summary>
        /// <returns></returns>
        public List<KeyValueBean> GetGroupList(string outCity, string ownerCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT * FROM BusPointGroup WHERE OwnerCode=@0 AND IsValid=1", ownerCode);
            if (!string.IsNullOrEmpty(outCity))
                sql.Append(" AND OutCity=@0 ", outCity);
            var groups = _groupDao.Fetch(sql.SQL, sql.Arguments);
            return groups.Select(item => new KeyValueBean { Key = item.Id.ToString(CultureInfo.InvariantCulture), Value = item.GroupName }).ToList();
        }

        /// <summary>
        /// 更新组别
        /// </summary>
        /// <param name="model"></param>
        public void UpdateGroup(BusPointGroupModel model, CrmAccountModel user)
        {
            var updateModel = _groupDao.GetById(model.Id);
            updateModel.GroupName = model.GroupName;
            updateModel.OutCity = model.OutCity;
            updateModel.ModifiedBy = user.Code;
            updateModel.ModifiedTime = DateTime.Now;
            _groupDao.Update(updateModel);
        }

        /// <summary>
        /// 新增组别
        /// </summary>
        /// <param name="model"></param>
        public void AddGroup(BusPointGroupModel model, CrmAccountModel user)
        {
            model.IsValid = 1;
            model.ModifiedBy = user.Code;
            model.ModifiedTime = DateTime.Now;
            model.OwnerCode = user.OwnerCode;
            _groupDao.Insert(model);
        }

        /// <summary>
        /// 获取组别
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public BusPointGroupModel GetGroupById(int groupId)
        {
            return _groupDao.GetById(groupId);
        }

        /// <summary>
        /// 删除组别
        /// </summary>
        /// <param name="groupId"></param>
        public void DeleteGroup(int groupId, CrmAccountModel user)
        {
            var updateModel = _groupDao.GetById(groupId);
            updateModel.IsValid = 0;
            updateModel.ModifiedBy = user.Code;
            updateModel.ModifiedTime = DateTime.Now;
            using (var scope = new TransactionScope())
            {
                _groupDao.Update(updateModel);
                _dao.Execute(@"UPDATE BaseBusPoint SET GroupId=0 where GroupId=@0", groupId);
                scope.Complete();
            }
        }

        #endregion
    }
}
