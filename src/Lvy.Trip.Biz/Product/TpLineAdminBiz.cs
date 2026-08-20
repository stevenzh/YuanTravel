using Lvy.Models.CrmDB;
using Lvy.Models.ProductDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Dao.Product;
using Lvy.VModels.Product;
using Lvy.Web.Common;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Lvy.Trip.Biz.Product
{
    /// <summary>
    /// 专管员
    /// </summary>
    public class TpLineAdminBiz : BaseBiz
    {
        private readonly TpLineAdminDao _dao = new TpLineAdminDao();

        /// <summary>
        /// 根据LineId获取专管员对象
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public List<TpLineAdminModel> GetByLineId(string lineId)
        {
            return _dao.Fetch(@"SELECT * FROM TpLineAdmin WHERE LineId = @0", lineId);
        }

        /// <summary>
        /// 根据LineId获取专管员对象
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public List<CrmAccountModel> GetAccountByLineId(string lineId)
        {
            return _dao.Query<CrmAccountModel>(@"SELECT ca.* FROM TpLineAdmin tla
inner join CrmAccount ca on ca.Code = tla.AccountCode
WHERE tla.LineId = @0", lineId).ToList();
        }

        /// <summary>
        /// 取得线路主负责人
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public CrmAccountModel GetPrimaryAdmin(string lineId)
        {
            return _dao.Query<CrmAccountModel>(@"SELECT ca.* FROM TpLineAdmin tla
inner join CrmAccount ca on ca.Code = tla.AccountCode
WHERE tla.LineId = @0 and tla.IsPrimary=1 and tla.Department=1 ", lineId).FirstOrDefault();
        }

        /// <summary>
        /// 根据订单编码 取得计调专管员
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public CrmAccountModel GetLineAdmin(string OrderCode)
        {
            return _dao.Query<CrmAccountModel>(@"SELECT ca.* FROM TpLineAdmin tla
inner join CrmAccount ca on ca.Code = tla.AccountCode
inner join TpOrder o on o.LineId = tla.LineId
WHERE tla.IsPrimary=1 and tla.Department=1 and o.OrderCode = @0 ", OrderCode).FirstOrDefault();
        }

        /// <summary>
        /// 获取专管员编辑对象
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public EditLineAdminVModel GetEditLineAdminVModel(string lineId, string teamId)
        {
            var lineBiz = new TpLineBiz();
            var accountBiz = new AccountBiz();

            var vModel = new EditLineAdminVModel
            {
                LineId = lineId,
                CustomerLineAdmin = new List<LineAdminVModel>(),
                PlatLineAdmin = new List<LineAdminVModel>()
            };
            var line = lineBiz.GetLineById(lineId);
            var lineAdmins = GetByLineId(lineId);
            /*
             * 默认在创建线路时，会将创建者作为线路专管员
             */

            List<CrmAccountModel> customerUsers = new List<CrmAccountModel>();//线路创建者对应的供应商的用户
            List<CrmAccountModel> platUsers = new List<CrmAccountModel>();//当前平台供应商用户
            if (line.IsImport == false)
            {
                //平台用户编辑
                //若编辑者与创建者均为平台供应商用户，则供应商用户列表无需显示
                customerUsers = new List<CrmAccountModel>();
                platUsers = accountBiz.GetAccountByTeam(line.OwnerCode, teamId);
            }
            else
            {
                //供应商用户编辑
                customerUsers = accountBiz.GetAllAccount(line.CustomerCode);
                platUsers = accountBiz.GetAccountByTeam(line.OwnerCode, teamId);
            }
            foreach (var customer in customerUsers)
            {
                var lineAdmin = lineAdmins.Find(p => p.AccountCode == customer.Code);
                vModel.CustomerLineAdmin.Add(new LineAdminVModel
                {
                    Checked = lineAdmin == null ? 0 : 1,
                    AccountCode = customer.Code,
                    Name = customer.Name,
                    LineAdminId = lineAdmin == null ? 0 : lineAdmin.Id,
                    Department = 0,
                    IsPrimary = lineAdmin == null ? 0 : lineAdmin.IsPrimary
                });
            }
            foreach (var platUser in platUsers)
            {
                var lineAdmin = lineAdmins.Find(p => p.AccountCode == platUser.Code);
                vModel.PlatLineAdmin.Add(new LineAdminVModel
                {
                    Checked = lineAdmin == null ? 0 : 1,
                    AccountCode = platUser.Code,
                    Name = platUser.Name,
                    LineAdminId = lineAdmin == null ? 0 : lineAdmin.Id,
                    Department = 1,
                    IsPrimary = lineAdmin == null ? 0 : lineAdmin.IsPrimary
                });
            }

            return vModel;
        }

        /// <summary>
        /// 保存线路专管员
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public string SaveLineAdmin(EditLineAdminVModel vModel)
        {
            var lineId = vModel.LineId;
            var lineAdmins = GetByLineId(lineId);
            var add = new List<TpLineAdminModel>();
            var update = new List<TpLineAdminModel>();
            var delete = new List<TpLineAdminModel>();

            #region 专线供应商

            foreach (var cAdmin in vModel.CustomerLineAdmin)
            {
                if (cAdmin.Checked == 1 && cAdmin.LineAdminId == 0)
                {
                    add.Add(new TpLineAdminModel
                    {
                        AccountCode = cAdmin.AccountCode,
                        Department = 0,
                        IsPrimary = cAdmin.IsPrimary,
                        LineId = lineId
                    });
                }
                else if (cAdmin.Checked == 1 && cAdmin.LineAdminId > 0)
                {
                    var model = lineAdmins.SingleOrDefault(p => p.Id == cAdmin.LineAdminId);
                    if (null != model)
                    {
                        model.Department = 0;
                        model.IsPrimary = cAdmin.IsPrimary;
                        update.Add(model);
                    }
                }
                else if (cAdmin.Checked == 0 && cAdmin.LineAdminId > 0)
                {
                    var model = lineAdmins.SingleOrDefault(p => p.Id == cAdmin.LineAdminId);
                    if (null != model)
                        delete.Add(model);
                }
            }

            #endregion 专线供应商

            #region 平台供应商

            foreach (var pAdmin in vModel.PlatLineAdmin)
            {
                if (pAdmin.Checked == 1 && pAdmin.LineAdminId == 0)
                {
                    add.Add(new TpLineAdminModel
                    {
                        AccountCode = pAdmin.AccountCode,
                        Department = 1,
                        IsPrimary = pAdmin.IsPrimary,
                        LineId = lineId
                    });
                }
                else if (pAdmin.Checked == 1 && pAdmin.LineAdminId > 0)
                {
                    var model = lineAdmins.SingleOrDefault(p => p.Id == pAdmin.LineAdminId);
                    if (null != model)
                    {
                        model.Department = 1;
                        model.IsPrimary = pAdmin.IsPrimary;
                        update.Add(model);
                    }
                }
                else if (pAdmin.Checked == 0 && pAdmin.LineAdminId > 0)
                {
                    var model = lineAdmins.SingleOrDefault(p => p.Id == pAdmin.LineAdminId);
                    if (null != model)
                        delete.Add(model);
                }
            }

            #endregion 平台供应商

            using (var scope = new TransactionScope())
            {
                foreach (var a in add)
                {
                    _dao.Insert(a);
                }
                foreach (var u in update)
                {
                    _dao.Update(u);
                }
                foreach (var d in delete)
                {
                    _dao.Delete(d);
                }
                scope.Complete();
            }
            return "success";
        }
    }
}