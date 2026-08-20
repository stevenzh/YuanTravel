using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Dao.Crm;
using Lvy.VModels.Base;
using Lvy.VModels.Crm;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.Trip.Biz.Crm
{
    /// <summary>
    /// 客户
    /// </summary>
    public class CustomerBiz : BaseBiz
    {
        private static readonly CustomerDao _dao = new CustomerDao();
        private static readonly AccountDao _accountDao = new AccountDao();
        private static readonly CustomerFileDao _fileDao = new CustomerFileDao();
        private static readonly CustomerHoldDao _holdDao = new CustomerHoldDao();
        private static readonly CustomerPolicyDao _policyDao = new CustomerPolicyDao();

        /// <summary>
        /// 获得一个客户对象
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public CrmCustomerModel GetById(string code)
        {
            return _dao.GetById(code);
        }

        /// <summary>
        /// 根据客户名称获得一个客户对象
        /// </summary>
        /// <param name="customerName"></param>
        /// <returns></returns>
        public CrmCustomerModel GetByCustomerName(string ownerCode, string customerName, string code)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT * FROM CrmCustomer WHERE OwnerCode=@0
 AND Name=@0 ", Ansi(ownerCode), Ansi(customerName));   // and isValid=1
            if (!string.IsNullOrEmpty(code))
            {
                sql.Append(" AND Code!=@0", code);
            }
            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///  获取客户集合
        /// </summary>
        /// <returns></returns>
        public PagedList<CrmCustomerModel> GetPagedList(CustomerVModel vModel, CrmAccountModel currentUser)
        {
            Sql sql = new Sql();
            sql.Append(@" select c.* ,a.Name as SalerName
from CrmCustomer c
left join CrmAccount a on c.SalerCode=a.Code
where 1=1 ");   // c.IsValid=1

            if (!vModel.Customer.Name.IsNullOrEmpty())
                sql.Append(" and c.Name like @0", AnsiLike(vModel.Customer.Name));

            if (!vModel.Customer.Code.IsNullOrEmpty())
                sql.Append(" and c.Code = @0", Ansi(vModel.Customer.Code));

            if (!vModel.Customer.TeamID.IsNullOrEmpty())
                sql.Append(" and c.TeamID in (" + vModel.Customer.TeamID + ")");

            if (!vModel.Customer.SalerCode.IsNullOrEmpty())
                sql.Append(" and c.SalerCode=@0", Ansi(vModel.Customer.SalerCode));
            if (vModel.Customer.PaymentType > 0)
                sql.Append(" and c.PaymentType =@0", vModel.Customer.PaymentType);
            if (!vModel.ContactNumber.IsNullOrEmpty())
                sql.Append(" and ( c.Mobile like @0 or c.Phone like @1 )", AnsiLike(vModel.ContactNumber), AnsiLike(vModel.ContactNumber));
            if (!string.IsNullOrEmpty(vModel.CustomerType))
            {
                if (vModel.CustomerType == "1")
                    sql.Append(" and c.IsDistributors=1 ");
                else if (vModel.CustomerType == "2")
                    sql.Append(" and c.IsSupplier=1 ");
                else if (vModel.CustomerType == "3")
                    sql.Append(" and c.IsBranch=1");
            }

            // 如果是系统商户的场合,能看到该商户下所有信息
            // 如果不是，只能看到自己的创建的信息
            if (currentUser.CustomerCode == currentUser.OwnerCode)
                sql.Append(" and c.OwnerCode=@0", currentUser.OwnerCode);
            else
                sql.Append(" and c.Code=@0", vModel.Customer.Code);

            //sql.Append(" order by c.Code desc ");
            try
            {
                var sortBy = vModel.SortCollection[vModel.SortKey];
                if (sortBy != null)
                    sql.Append(" order by " + sortBy.Key);
                else
                {
                    sql.Append(" order by ModifiedTime desc");
                }
            }
            catch
            {
            }
            var list = _dao.Pager(vModel.Customers.PageIndex, vModel.Customers.PageSize, sql.SQL, sql.Arguments);

            return list;
        }

        /// <summary>
        /// 根据客户状态获取客户集合
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<CrmCustomerModel> UCSelectUse(CustomerVModel vModel, CrmAccountModel currentUser)
        {
            Sql sql = new Sql();
            sql.Append(@"select c.* ,a.Name as SalerName
from CrmCustomer c
left join CrmAccount a on c.SalerCode=a.Code
where c.InPublic=1 and c.OwnerCode=@0", currentUser.OwnerCode);

            if (!vModel.Customer.Name.IsNullOrEmpty())
                sql.Append(" and c.Name like @0", AnsiLike(vModel.Customer.Name));
            if (!vModel.Customer.Head.IsNullOrEmpty())
                sql.Append(" and c.Head like @0", AnsiLike(vModel.Customer.Head));
            if (!vModel.Customer.TeamID.IsNullOrEmpty())
                sql.Append(" and c.TeamID=@0", Ansi(vModel.Customer.TeamID));
            if (!vModel.Customer.SalerCode.IsNullOrEmpty())
                sql.Append(" and c.SalerCode=@0", Ansi(vModel.Customer.SalerCode));
            if (vModel.Customer.PaymentType > 0)
                sql.Append(" and c.PaymentType =@0", vModel.Customer.PaymentType);
            if (!vModel.ContactNumber.IsNullOrEmpty())
                sql.Append(" and ( c.Mobile like @0 or c.Phone like @1 )", AnsiLike(vModel.ContactNumber), AnsiLike(vModel.ContactNumber));

            //sql.Append(" order by c.Code desc ");
            try
            {
                var sortBy = vModel.SortCollection[vModel.SortKey];
                if (sortBy != null)
                    sql.Append(" order by " + sortBy.Key);
            }
            catch
            {
            }
            var list = _dao.Pager(vModel.Customers.PageIndex, vModel.Customers.PageSize, sql.SQL, sql.Arguments);

            return list;
        }

        /// <summary>
        /// 添加客户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public object Add(CrmCustomerModel model)
        {
            return _dao.Insert(model);
        }

        /// <summary>
        /// 更新客户资料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(CrmCustomerModel model)
        {
            return _dao.Update(model);
        }

        /// <summary>
        /// 取得owercode对应的销售
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public List<CrmAccountModel> GetTeamSales(string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@"select distinct ca.*
from CrmAccount ca
inner join SysUserRoleMap ar on ar.AccountCode = ca.Code
inner join SysRole ur on ar.RoleId = ur.Id
where ca.OwnerCode=@0 and ca.CustomerCode=@0 and ca.IsValid = 1
and ur.Name in('销售', '销售组长', '销售总监') ", Ansi(ownerCode));

            return _dao.Query<CrmAccountModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 根据分组Id获得分组下的人员信息.
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public List<CrmAccountModel> GetTeamUsersByTeamId(string teamId, string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@"select ca.*
from CrmAccount ca, TeamAccountMap ta
where ca.Code=ta.AccountCode and ca.OwnerCode=@0 and ca.CustomerCode=@0 and ca.IsValid=1 and ta.TeamID=@1 ",
                 Ansi(ownerCode), Ansi(teamId));

            return _dao.Query<CrmAccountModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 根据商户名称模糊查询对应的 商户列表
        /// </summary>
        /// <param name="customerName"></param>
        /// <param name="ownerCode"></param>
        /// <returns></returns>
        public List<CrmCustomerModel> GetCustomerCodeByName(string customerName, string ownerCode)
        {
            var sql = new Sql();
            sql.Append(@" select Code from CrmCustomer
                 where OwnerCode=@0 and Name like @1", ownerCode, AnsiLike(customerName));
            return _dao.Query<CrmCustomerModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 根据商户名称 模糊查询 对应的商户Code
        /// </summary>
        /// <param name="customerName"></param>
        /// <returns></returns>
        public string GetCustomerCodesSql(string customerName, string ownerCode)
        {
            var strTemp = "0";
            var customerModels = new List<CrmCustomerModel>();
            customerModels = GetCustomerCodeByName(customerName, ownerCode);
            if (customerModels.Count > 0)
            {
                strTemp = "";
                foreach (var crmCustomerModel in customerModels)
                {
                    strTemp += crmCustomerModel.Code + ",";
                }
                strTemp = strTemp.Substring(0, strTemp.Length - 1);
            }
            return strTemp;
        }

        /// <summary>
        /// 获取客户对象列表【客户本身及其附属客户】
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public List<CrmCustomerModel> GetCustomers(string customerCode)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT Code,Name,FastCode,Head,Mobile,Phone,Address,CreditLine,PaymentType,IsSupplier,IsDistributors,IsOwner,
SalerCode,LogoPath,ElecCertifyPath,Remarks,IsValid,ModifiedBy,ModifiedTime,OwnerCode,ParentCode
FROM CrmCustomer WHERE Code = @0 OR ParentCode = @1", Ansi(customerCode), Ansi(customerCode));

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<CrmCustomerModel> SelectCustomerSource(SelectCustomerVModel vModel, string ownerCode)
        {
            Sql sql = new Sql();
            sql.Append("SELECT * FROM CrmCustomer WHERE OwnerCode=@0", Ansi(ownerCode));
            if (!vModel.KeyWord.IsNullOrEmpty())
            {
                sql.Append(" AND (Name LIKE @0 OR FastCode LIKE @1)", AnsiLike(vModel.KeyWord), AnsiLike(vModel.KeyWord));
            }
            sql.Append(@" ORDER BY Name,FastCode,Code");
            return _dao.Pager(vModel.PagedCustomers.PageIndex, vModel.PagedCustomers.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 供应商
        /// </summary>
        /// <param name="ownerCode"></param>
        /// <returns></returns>
        public List<CrmCustomerModel> GetSupplierList(string ownerCode)
        {
            var sql = new StringBuilder();
            sql.Append("select * from CrmCustomer where isvalid=1 and ownerCode=@0 and IsSupplier=1 ");

            return _dao.Query<CrmCustomerModel>(sql.ToString(), ownerCode).ToList();
        }

        /// <summary>
        /// 获取客户的联系人信息集合
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<CrmAccountModel> GetContactPagedList(AccountVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append(@"select ca.*, sa.Name as SalerName
from CrmAccount ca left join CrmAccount sa on ca.SalerCode=sa.Code
where ca.CustomerCode=@0 and ca.IsValid=1 ", vModel.Account.CustomerCode);
            if (!string.IsNullOrEmpty(vModel.Account.Name))
            {
                sql.Append(" and ca.Name like @0 ", AnsiLike(vModel.Account.Name));
            }
            if (!string.IsNullOrEmpty(vModel.Account.Mobile))
            {
                sql.Append(" and ca.Mobile like @0 ", AnsiLike(vModel.Account.Mobile));
            }
            var list = _accountDao.Pager(vModel.Accounts.PageIndex, vModel.Accounts.PageSize, sql.SQL, sql.Arguments);

            return list;
        }

        public List<CrmAccountModel> GetContactList(string customerCode)
        {
            Sql sql = new Sql();
            sql.Append(@"select ca.*, sa.Name as SalerName
from CrmAccount ca left join CrmAccount sa on ca.SalerCode=sa.Code
where ca.CustomerCode=@0 and ca.IsValid=1 ", customerCode);

            var list = _accountDao.Fetch(sql.SQL, sql.Arguments);

            return list;
        }

        public PagedList<CustomerFileModel> Uploadings(CustomerFileVModel vModel)//显示插叙自己建的实体类多添加了一条列但是修改删除都是用的跟数据库对应的实体类
        {
            Sql sql = new Sql();
            sql.Append(@" select * from CustomerFiles c")
               .Append("where c.CustomerCode=@0 and c.IsValid=1 ", vModel.CustomerFile.CustomerCode);
            if (!vModel.CustomerFile.FileName.IsNullOrEmpty())
                sql.Append(" and c.FileName like @0", AnsiLike(vModel.CustomerFile.FileName));

            if (!vModel.CustomerFile.Subject.IsNullOrEmpty())
                sql.Append(" and c.Subject like @0", AnsiLike(vModel.CustomerFile.Subject));
            //if (!vModel.CustomerFile.StratDate.ToString().IsNullOrEmpty())
            //    sql.Append(" and c.StratDate like @0 ", vModel.CustomerFile.StratDate);
            //if (!vModel.CustomerFile.EndDate.ToString().IsNullOrEmpty())
            //    sql.Append(" and c.EndDate like @0 ", vModel.CustomerFile.EndDate.ToDateTime().AddDays(1));

            return _fileDao.Pager<CustomerFileModel>(vModel.FilePageList.PageIndex, vModel.FilePageList.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 取得有效合同
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public List<CustomerFileModel> GetValidContract(string customerCode)
        {
            Sql sql = new Sql();
            sql.Append(@" select * from CustomerFiles c")
               .Append("where c.CustomerCode=@0 and c.IsValid=1 ", customerCode);
            sql.Append(" and c.StratDate < @0 ", DateTime.Now.ToString("yyyy-MM-dd"));
            sql.Append(" and c.EndDate > @0 ", DateTime.Now.ToString("yyyy-MM-dd"));

            return _fileDao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public int DeleteUploading(int Id)
        {
            Sql sql = new Sql();
            sql.Append(" update CustomerFiles set IsValid=0 where Id=@0", Id);
            return _fileDao.Execute(sql.SQL, sql.Arguments);
        }

        public CustomerFileModel UploadingId(int Id)
        {
            return _fileDao.GetById(Id);
        }

        public int UploadingUpdate(CustomerFileModel model)
        {
            return _fileDao.Update(model);
        }

        public object Zen(CustomerFileModel model)
        {
            return _fileDao.Insert(model);
        }

        public CustomerFileModel Uploadingdownload(int Id)
        {
            var sql = new Sql();
            sql.Append(" select * from CustomerFiles where Id=@0 ", Id);
            return _fileDao.Query<CustomerFileModel>(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        public CrmCustomerModel GetParentCrmCustomerModel(string customerCode, string ownerCode)
        {
            var sql = new Sql();
            sql.Append("select (select Name from CrmCustomer where Code=c.ParentCode) as ParentName, c.* from CrmCustomer c where isvalid=1 and ownerCode=@0  and  Code=@1 and ParentCode is not null", ownerCode, customerCode);
            return _dao.Query<CrmCustomerModel>(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        #region 审核客户的相关操作方法

        /// <summary>
        /// 获取未审核的客户的列表信息
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<CrmCustomerModel> GetNoAuditPagedList(CustomerVModel vModel, CrmAccountModel currentUser)
        {
            Sql sql = new Sql();
            sql.Append(@" select c.* ,a.Name as SalerName from CrmCustomer c
left join CrmAccount a on c.SalerCode=a.Code
where c.IsValid=1 and CustomerState<>1 ");

            if (!vModel.Customer.Name.IsNullOrEmpty())
                sql.Append(" and c.Name like @0", AnsiLike(vModel.Customer.Name));

            if (!vModel.Customer.Head.IsNullOrEmpty())
                sql.Append(" and c.Head like @0", AnsiLike(vModel.Customer.Head));

            if (!vModel.Customer.TeamID.IsNullOrEmpty())
                sql.Append(" and c.TeamID =@0", Ansi(vModel.Customer.TeamID));

            //如果是销售组长 可以看到他组下的所有客户
            if (!string.IsNullOrEmpty(vModel.Customer.TeamID))
                sql.Append(" and c.TeamID =@0", vModel.Customer.TeamID);
            if (!vModel.Customer.SalerCode.IsNullOrEmpty())
                sql.Append(" and c.SalerCode=@0", Ansi(vModel.Customer.SalerCode));
            if (vModel.Customer.PaymentType > 0)
                sql.Append(" and c.PaymentType =@0", vModel.Customer.PaymentType);
            if (!vModel.ContactNumber.IsNullOrEmpty())
                sql.Append(" and ( c.Mobile like @0 or c.Phone like @1 )", AnsiLike(vModel.ContactNumber), AnsiLike(vModel.ContactNumber));

            // 如果是系统商户的场合,能看到该商户下所有信息
            // 如果不是，只能看到自己的创建的信息
            if (currentUser.CustomerCode == currentUser.OwnerCode)
                sql.Append(" and c.OwnerCode=@0", currentUser.OwnerCode);
            else
                sql.Append(" and c.Code=@0", vModel.Customer.Code);

            //sql.Append(" order by c.Code desc ");
            try
            {
                var sortBy = vModel.SortCollection[vModel.SortKey];
                if (sortBy != null)
                    sql.Append(" order by " + sortBy.Key);
                else
                {
                    sql.Append(" order by ModifiedTime desc");
                }
            }
            catch
            {
            }
            var list = _dao.Pager(vModel.Customers.PageIndex, vModel.Customers.PageSize, sql.SQL, sql.Arguments);

            return list;
        }

        #endregion 审核客户的相关操作方法

        /// <summary>
        /// 获取客户下联系人集合
        /// </summary>
        /// <param name="CustomerCode"></param>
        /// <returns></returns>
        public List<CrmAccountModel> GetContactListByCustomerCode(string CustomerCode)
        {
            Sql sql = new Sql();
            sql.Append(@"select * from CrmAccount where CustomerCode=@0 and IsValid=1 ", CustomerCode);

            return _accountDao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 取得客户相关销售（包含联系人的）
        /// </summary>
        /// <param name="CustomerCode"></param>
        /// <returns></returns>
        public List<CrmAccountModel> GetSalesByCustomerCode(string CustomerCode)
        {
            var list = GetContactListByCustomerCode(CustomerCode).Select(t => t.SalerCode);
            var code = "'" + GetById(CustomerCode).SalerCode + "'";
            if (list.Count() > 0)
            {
                string pin = string.Join("','", list);
                code = code + ",'" + (list.Count() > 1 ? pin.Substring(0, pin.Length - 2) : pin) + "'";
            }
            string query = "select * from CrmAccount where Code in (" + code + ")";

            return _accountDao.Fetch(query);
        }

        /// <summary>
        /// 获取供应商集合
        /// </summary>
        /// <returns></returns>
        public List<CrmCustomerModel> GetAllSupplier()
        {
            Sql sql = new Sql();
            sql.Append(" select Code,Name from CrmCustomer where IsSupplier=1 ");
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        public List<CrmCustomerModel> GetAllBranch()
        {
            Sql sql = new Sql();
            sql.Append(" select Code,Name from CrmCustomer where IsBranch=1 ");
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        #region 折让规则

        /// <summary>
        ///
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public List<CustomerPolicyModel> GetPolicyList(string customerCode)
        {
            Sql sql = new Sql();
            sql.Append(@"select cp.*, bd.Name RegionName from CustomerPolicys cp
left join BaseDestination bd on cp.Code=bd.ParentStr
where cp.CustomerCode=@0 ", customerCode);

            var list = _accountDao.Query<CustomerPolicyModel>(sql.SQL, sql.Arguments).ToList();

            return list;
        }

        public void DeletePolicy(long policyId)
        {
            _policyDao.Delete(policyId);
        }

        public CustomerPolicyModel GetPolicyById(long id)
        {
            return _policyDao.GetById(id);
        }

        public object AddPolicy(CustomerPolicyModel policyEntity)
        {
            return _policyDao.Insert(policyEntity);
        }

        public object UpdatePolicy(CustomerPolicyModel model)
        {
            return _policyDao.Update(model);
        }

        #endregion 折让规则

        /// <summary>
        /// 获取销售的客户和联系人数
        /// </summary>
        /// <returns></returns>
        public StatItemVModel GetCustomerBySales(string teamId, string salesCode)
        {
            Sql sql = new Sql();
            sql.Append(@" select
(select count(*) from CrmCustomer where SalerCode=@1 and TeamID=@0 and IsValid=1 ) as CustomerCount ,
(select count(*) from CrmAccount where SalerCode=@1 and TeamID=@0 and IsValid=1) as ContactCount ", teamId, salesCode);

            return _dao.Query<StatItemVModel>(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        /// <summary>
        /// 获取销售的部分客户
        /// </summary>
        /// <returns></returns>
        public List<CrmCustomerModel> GetCustomerBySales(string salesCode)
        {
            Sql sql = new Sql();
            sql.Append(@" select * from CrmCustomer where IsValid=1
and SalerCode=@0 OR Code in (select CustomerCode from CrmAccount where isValid=1 and SalerCode=@0) ", salesCode);  //取得自己的客户和 联系人自己负责的客户（公司可能其他小数负责）
            return _dao.Fetch(sql.SQL, sql.Arguments).ToList();
        }

        #region 客户领用相关

        /// <summary>
        /// 添加客户领用记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public object AddCustomerHold(CustomerHoldModel model)
        {
            return _holdDao.Insert(model);
        }

        /// <summary>
        /// 获取最后领用记录
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public CustomerHoldModel GetLastHold(string customerCode)
        {
            var sql = new Sql();
            sql.Append("select * from CustomerHolds where CustomerCode=@0 ORDER BY HoldDate DESC  ", customerCode);
            return _holdDao.First(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 获取客户所有领用记录
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public CustomerHoldModel GetHoldList(string code)
        {
            var sql = new Sql();
            sql.Append("select * from CustomerHolds where CustomerCode=@0", code);
            return _holdDao.First(sql.SQL, sql.Arguments);
        }

        public int UpdateHold(string customerCode, string salesCode)
        {
            Sql sql = new Sql();
            sql.Append(" update CrmCustomer set ReceiveDate=now(), InPublic=0, SalerCode=@0 where Code=@1", salesCode, customerCode);
            return _dao.Execute(sql.SQL, sql.Arguments);
        }

        public int MoveCustomer(string customerCode, string TeamId, string salerCode)
        {
            Sql sql = new Sql();
            sql.Append(" update CrmCustomer set TeamID=@1, SalerCode=@2 where Code=@0", customerCode, TeamId, salerCode);
            return _dao.Execute(sql.SQL, sql.Arguments);
        }

        #endregion 客户领用相关

        public int UpdateTaxNumber(string customerCode, string taxNumber)
        {
            Sql sql = new Sql();
            sql.Append(" update CrmCustomer set TaxNumber=@0 where Code=@1", taxNumber, customerCode);
            return _dao.Execute(sql.SQL, sql.Arguments);
        }

        public int UpdateBankInfo(string customerCode, string bankInfo)
        {
            Sql sql = new Sql();
            sql.Append(" update CrmCustomer set BankInfo=@0 where Code=@1", bankInfo, customerCode);
            return _dao.Execute(sql.SQL, sql.Arguments);
        }
    }
}