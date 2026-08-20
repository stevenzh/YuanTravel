using Arch.Common;
using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Dao.Crm;
using Lvy.VModels.Crm;
using Lvy.Web.Common;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Lvy.Trip.Biz.Crm
{
    /// <summary>
    ///  账户处理模块
    ///
    /// </summary>
    public class AccountBiz : BaseBiz
    {
        private readonly AccountDao _dao = new AccountDao();
        // private readonly SysUserRoleMapDao _userRoleMapDao = new SysUserRoleMapDao();

        /// <summary>
        /// 得到一个账户对象
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public CrmAccountModel GetById(string code)
        {
            return _dao.GetById(code);
        }

        /// <summary>
        /// 根据登陆名和OwnerCode获取一个账户对象
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public CrmAccountModel GetByLoginName(string loginName)
        {
            Sql sql = new Sql();
            sql.Append(" select * from CrmAccount ")
                .Append(" where LoginName=@0 and IsValid=1", Ansi(loginName));

            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 用于验证EMail 是否重复
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public CrmAccountModel CheckContactEmail(string ownerCode, string email, string code)
        {
            Sql sql = new Sql();
            sql.Append(" select * from CrmAccount ")
                .Append(" where CustomerCode<>@0 and Email=@1 and IsValid=1", ownerCode, Ansi(email));

            if (!string.IsNullOrEmpty(code))
            {
                sql.Append(" and Code!=@0", code);
            }

            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        public CrmAccountModel GetAccountByName(string ownerCode, string name)
        {
            Sql sql = new Sql();
            sql.Append(" select * from CrmAccount ")
                .Append(" where OwnerCode=@0 and CustomerCode=@0 and Name=@1 and IsValid=1", ownerCode, Ansi(name));

            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 检验联系人 手机号重复 （公司之外）
        /// </summary>
        /// <param name="ownerCode"></param>
        /// <param name="mobile"></param>
        /// <param name="code">非当前用户</param>
        /// <returns></returns>
        public CrmAccountModel CheckContactMobile(string ownerCode, string mobile, string code)
        {
            Sql sql = new Sql();
            sql.Append(" select * from CrmAccount ")
                .Append(" where CustomerCode<>@0 and Mobile=@1 and IsValid=1", ownerCode, Ansi(mobile));

            if (!string.IsNullOrEmpty(code))
            {
                sql.Append(" and Code!=@0", code);
            }

            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 同客户的联系人不能重名
        /// </summary>
        /// <param name="customerName">客户名称</param>
        /// <param name="name">联系人姓名</param>
        /// <param name="code">联系人编号</param>
        /// <returns></returns>
        public CrmAccountModel CheckContactName(string customerName, string name, string code)
        {
            Sql sql = new Sql();
            sql.Append(" select ca.* from CrmAccount ca ")
                .Append(" inner join CrmCustomer cc on ca.CustomerCode=cc.Code ")
                .Append(" where cc.Name=@0 and ca.Name=@1 and ca.IsValid=1 ", customerName, Ansi(name));

            if (!string.IsNullOrEmpty(code))  // 非当前用户
            {
                sql.Append(" and ca.Code!=@0", code);
            }

            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 根据用户名和密码获取一个账户对象（普通用户登录）
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public CrmAccountModel GetByLogin(string oid, string loginName, string pwd)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT acc.*,cus.Name AS CustomerName FROM CrmAccount acc 
  INNER JOIN CrmCustomer cus ON acc.CustomerCode=cus.code AND cus.IsValid=1
WHERE acc.IsValid=1 AND acc.OwnerCode=@0 
AND (acc.LoginName=@1 OR acc.Email=@1) AND acc.Pwd=@2 ", Ansi(oid), Ansi(loginName), Ansi(pwd));

            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 分销商登录
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="loginName"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public CrmAccountModel AgentLogin(string oid, string loginName, string pwd)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT acc.*,cus.Name AS CustomerName FROM CrmAccount acc 
  INNER JOIN CrmCustomer cus ON acc.CustomerCode=cus.code AND cus.IsValid=1 AND cus.CustomerState=1
WHERE acc.IsValid=1 AND acc.OwnerCode=@0 AND acc.SalerState=1
AND acc.Email=@1 AND acc.Pwd=@2 ", Ansi(oid), Ansi(loginName), Ansi(pwd));

            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="newPwd"> </param>
        /// <returns></returns>
        public int ResetPwd(string accountCode, string newPwd = "888888")
        {
            newPwd = Toolkit.Security.ToEncrypt(newPwd);
            string sql = "set Pwd=@0 where Code=@1";
            return _dao.Update(sql, newPwd, Ansi(accountCode));
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<CrmAccountModel> GetPagedList(AccountVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append(@" SELECT ca.*, cc.Name AS CustomerName, Saler.Name AS SalerName
FROM CrmAccount ca
INNER JOIN CrmCustomer cc ON ca.CustomerCode = cc.Code
LEFT JOIN CrmAccount Saler ON ca.SalerCode = Saler.Code 
 WHERE ca.OwnerCode=@0 AND ca.IsValid=1 AND ca.AccountType!=1 ", vModel.OwnerCode);

            if (!vModel.Account.LoginName.IsNullOrEmpty())
                sql.Append(" AND ca.LoginName LIKE @0", AnsiLike(vModel.Account.LoginName));
            if (!vModel.Account.Name.IsNullOrEmpty())
                sql.Append(" AND ca.Name LIKE @0", AnsiLike(vModel.Account.Name));
            if (!vModel.Account.CustomerCode.IsNullOrEmpty())
                sql.Append(" AND ca.CustomerCode=@0", Ansi(vModel.Account.CustomerCode));
            if (!vModel.Account.CustomerName.IsNullOrEmpty())
                sql.Append(" AND cc.Name LIKE @0", AnsiLike(vModel.Account.CustomerName));
            if (!vModel.ContactNumber.IsNullOrEmpty())
                sql.Append(" AND (ca.Mobile LIKE @0 OR ca.Phone LIKE @1 )", AnsiLike(vModel.ContactNumber), AnsiLike(vModel.ContactNumber));
            if (vModel.Account.DepartCode > 0)
                sql.Append(" AND ca.DepartCode=@0", vModel.Account.DepartCode);


            if (vModel.IsEmployee == 1)  // 内部员工查询
            {
                sql.Append(" AND ca.CustomerCode=@0", Ansi(vModel.Account.OwnerCode));

                if (!vModel.CrmTeamId.IsNullOrEmpty())   // 联系人关联销售所属部门
                {
                    sql.Append(" AND ca.code IN (SELECT AccountCode FROM TeamAccountMap WHERE TeamID=@0)", Ansi(vModel.CrmTeamId));
                }
            }
            else        // 外部联系人查询
            {
                sql.Append(" and ca.CustomerCode<>@0", Ansi(vModel.Account.OwnerCode));
                if (!vModel.CrmTeamId.IsNullOrEmpty())   // 联系人关联销售所属部门
                {
                    sql.Append(" AND ca.TeamID=@0", Ansi(vModel.CrmTeamId));
                }
            }

            if (!vModel.SalesCode.IsNullOrEmpty())                  // 联系人关联销售
            {
                sql.Append(" AND ca.SalerCode=@0", Ansi(vModel.SalesCode));
            }
            //sql.Append(" ORDER BY ca.CustomerCode ");
            try
            {
                var sortBy = vModel.SortCollection[vModel.SortKey];
                if (sortBy != null)
                    sql.Append(" ORDER BY " + sortBy.Key);
            }
            catch { }

            return _dao.Pager(vModel.Accounts.PageIndex, vModel.Accounts.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 更新账户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(CrmAccountModel model)
        {
            return _dao.Update(model);
        }

        public int UpdateFromSite(CrmAccountModel model)
        {
            return _dao.Update("SET Email=@1, Sex=@2 WHERE Code=@0 ", model.Code, model.Email, model.Sex);
        }

        /// <summary>
        /// 更新账户信息
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public int UpdateTrans(AccountEditVModel vModel)
        {
            using (var ts = _dao.GetTransaction())
            {
                //保存账户基本信息
                _dao.Update(vModel.Account);
                //保存关联角色
                _dao.DelRoleMapByAccountCode(vModel.Account.Code);
                _dao.InsertBatchUserRoleMap(vModel.Account.Code, vModel.SelectedRoleIds);
                //保存组信息
                _dao.DelTeamMapByAccountCode(vModel.Account.Code);
                _dao.InsertBatchUserTeamMap(vModel.Account.Code, vModel.SelectedTeamIds);

                //保存关联目的地
                //_dao.DelDestMapByAccountCode(vModel.Account.Code);
                //_dao.InsertBatchUserDestMap(vModel.Account.Code, vModel.SelectedDestIds);

                if (vModel.AsyncCustomer == 1)
                {
                    _dao.UpdateCustomerTeam(vModel.Account.Code, vModel.SalesTeam);
                    _dao.UpdateContactTeam(vModel.Account.Code, vModel.SalesTeam);
                }

                ts.Complete();
            }
            return 1;
        }

        /// <summary>
        /// 添加账户
        /// 保存账户基本信息
        /// 保存关联目的地  保存关联角色
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public string AddTrans(AccountEditVModel vModel)
        {
            using (var ts = _dao.GetTransaction())
            {
                //保存账户基本信息
                vModel.Account.Code = _dao.Insert(vModel.Account).ToString();
                //保存关联角色
                _dao.DelRoleMapByAccountCode(vModel.Account.Code);
                _dao.InsertBatchUserRoleMap(vModel.Account.Code, vModel.SelectedRoleIds);

                //保存组信息
                _dao.DelTeamMapByAccountCode(vModel.Account.Code);
                _dao.InsertBatchUserTeamMap(vModel.Account.Code, vModel.SelectedTeamIds);

                //保存关联目的地
                //_dao.DelDestMapByAccountCode(vModel.Account.Code);
                //_dao.InsertBatchUserDestMap(vModel.Account.Code, vModel.SelectedDestIds);

                ts.Complete();
            }
            return vModel.Account.Code;
        }

        /// <summary>
        /// 取得用户所在部门
        /// </summary>
        /// <param name="accountCode"></param>
        /// <returns></returns>
        public List<CrmTeamModel> GetTeamByAccountCode(string accountCode)
        {
            string sql = " select ct.* from CrmTeam ct, TeamAccountMap tam where ct.TeamID=tam.TeamID and AccountCode=@0";
            return _dao.Query<CrmTeamModel>(sql, Ansi(accountCode)).ToList();
        }

        #region 角色相关

        /// <summary>
        /// 取得所有有效的角色
        /// </summary>
        /// <returns></returns>
        public List<KeyValueBean> GetAllRoles(string ownerCode)
        {
            string sql = @"select Id as `Key` ,Name as Value from SysRole where IsValid=1 and OwnerCode=@0";
            return _dao.Query<KeyValueBean>(sql, Ansi(ownerCode)).ToList();
        }

        /// <summary>
        /// 根据账户code 获取关联的角色对象Ids
        /// </summary>
        /// <param name="accountCode"></param>
        /// <returns></returns>
        public string[] GetSelectedRoleIds(string accountCode)
        {
            string sql = "select RoleId from SysUserRoleMap where AccountCode=@0";
            return _dao.Query<string>(sql, Ansi(accountCode)).ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="accountCode"></param>
        /// <returns></returns>
        public IList<SysRoleModel> GetRoleByAccountCode(string accountCode)
        {
            var sql = new Sql();
            sql.Append(" select RoleId, Name from SysUserRoleMap, SysRole where SysRole.Id = SysUserRoleMap.RoleId and AccountCode=@0 ", Ansi(accountCode));
            return _dao.Query<SysRoleModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 批量添加角色
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="role"></param>
        public void AddRole(string accountCode, string[] role)
        {
            _dao.InsertBatchUserRoleMap(accountCode, role);
        }

        /// <summary>
        /// 批量删除角色
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="role"></param>
        public void DeleteRole(string accountCode, string[] role)
        {
            _dao.DeleteBatchUserRoleMap(accountCode, role);
        }

        #endregion 角色相关

        /// <summary>
        /// 根据账户code 获取关联的目的地Ids
        /// </summary>
        /// <param name="accountCode"></param>
        /// <returns></returns>
        public string[] GetSelectedDestIds(string accountCode)
        {
            string sql = "select DestId from SysUserDestMap where AccountCode=@0";
            return _dao.Query<string>(sql, Ansi(accountCode)).ToArray();
        }

        /// <summary>
        /// 取得所有有效的目的地
        /// </summary>
        /// <returns></returns>
        public List<KeyValueBean> GetAllDestBeans()
        {
            string sql = @"select Id as `Key`, Name as Value from BaseDestination where IsValid=1";
            return _dao.Query<KeyValueBean>(sql).ToList();
        }

        /// <summary>
        /// 通过ownercode取得客户信息列表
        /// </summary>
        /// <returns></returns>
        public List<KeyValueBean> GetAllCustomerBeans(string ownerCode)
        {
            string sql = @"select Code as `Key`, Name as Value from CrmCustomer where IsValid=1 and OwnerCode=@0";
            return _dao.Query<KeyValueBean>(sql, ownerCode).ToList();
        }

        /// <summary>
        /// 取得所有部门
        /// </summary>
        /// <returns></returns>
        public List<KeyValueBean> GetAllTeamBeans(string ownerCode)
        {
            return _dao.Query<KeyValueBean>("SELECT TeamID AS `Key`, TeamName AS Value FROM CrmTeam WHERE OwnerCode=@0 AND IsValid = 1 ORDER BY TeamName ", ownerCode).ToList();
        }

        /// <summary>
        /// 获取某个职能的用户
        /// 不推荐
        /// </summary>
        /// <param name="departCode"></param>
        /// <returns></returns>
        public List<CrmAccountModel> GetAllDepartAccount(string CustomerCode, string departCode)
        {
            return _dao.Fetch(@"select distinct ca.* from CrmAccount ca, TeamAccountMap tam, CrmTeam ct where tam.AccountCode = ca.Code and tam.TeamID = ct.TeamID and ca.OwnerCode=@0 and ct.DepartCode = @1 ", Ansi(CustomerCode), Ansi(departCode));
        }

        /// <summary>
        /// 获取账号的编码和名称集合
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public List<KeyValueBean> GetAllAccountBeans(string customerCode, string ownerCode)
        {
            string sql = @"SELECT Code AS `Key`, Name AS Value FROM CrmAccount WHERE IsValid=1 AND (CustomerCode=@0 OR CustomerCode=@1) ";

            return _dao.Query<KeyValueBean>(sql, Ansi(customerCode), ownerCode).ToList();
        }

        /// <summary>
        /// 获取账号
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public List<CrmAccountModel> GetAllAccount(string customerCode)
        {
            return _dao.Fetch(@"SELECT * FROM CrmAccount WHERE IsValid=1 AND CustomerCode=@0 ", Ansi(customerCode));
        }

        /// <summary>
        /// 获取用户对象(包含客户对象)
        /// </summary>
        /// <param name="accountCode"></param>
        /// <returns></returns>
        public CrmAccountModel GetAccountCustomer(string accountCode)
        {
            Sql sql = new Sql();
            sql.Append(" select acc.*,cus.* from CrmAccount acc ")
                .Append(" inner join CrmCustomer cus on acc.CustomerCode=cus.code ")
                .Append(@" where acc.code=@0 ", Ansi(accountCode));

            return _dao.Query<CrmAccountModel, CrmCustomerModel>(sql.SQL, sql.Arguments).FirstOrDefault();
        }

        public List<CrmAccountModel> GetAccountByOpenID(string openID)
        {
            Sql sql = new Sql();
            sql.Append(" select acc.*,cus.* from CrmAccount acc ")
                .Append(" inner join CrmCustomer cus on acc.CustomerCode=cus.code ")
                .Append(@" where acc.OpenID=@0 ", Ansi(openID));

            return _dao.Query<CrmAccountModel, CrmCustomerModel>(sql.SQL, sql.Arguments).ToList();
        }

        /// <summary>
        /// 根据账号列表获取账号信息
        /// </summary>
        /// <param name="accountCodes"></param>
        /// <returns></returns>
        public List<CrmAccountModel> GetAccountByCode(List<string> accountCodes)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT * FROM CrmAccount WHERE code IN ( @0", accountCodes[0]);
            for (int i = 1, len = accountCodes.Count; i < len; i++)
            {
                sql.Append(@", @0", accountCodes[i]);
            }
            sql.Append(@")");
            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 客户注册
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string Register(CrmAccountModel model, string url)
        {
            var platform = new SysPlatformDao().GetByUrl(url);

            #region customer

            var cus = new CrmCustomerModel();
            cus.Code = model.CustomerCode;
            cus.Name = model.Customer.Name;
            cus.FastCode = "";
            cus.Head = model.Customer.Head;
            cus.Mobile = model.Customer.Mobile;
            cus.Phone = model.Customer.Phone;
            cus.RebateInBill = true;
            cus.Address = model.Customer.Address;
            cus.CreditLine = 0;
            cus.PaymentType = 1; //月结
            cus.IsDistributors = true;//分销商
            cus.IsSupplier = false;
            cus.IsOwner = false;
            cus.Remarks = model.Customer.Remarks;
            cus.IsValid = 0;
            cus.CustomerState = 0; // 待审核
            cus.OwnerCode = platform.CustomerCode;
            cus.ModifiedBy = "";
            cus.ModifiedTime = DateTime.Now;

            #endregion customer

            #region account

            var account = new CrmAccountModel();
            account.Code = DBTools.GetSeqNo("CrmAccount");
            account.LoginName = model.LoginName;
            account.Pwd = Toolkit.Security.ToEncrypt(model.Pwd);
            account.Name = model.Customer.Head;
            account.Mobile = model.Customer.Mobile;
            account.Phone = model.Customer.Phone;
            account.DepartCode = 0;
            account.CustomerCode = cus.Code;
            account.ModifiedBy = account.Code;
            account.ModifiedTime = DateTime.Now;
            account.AccountType = 3; // 普通员工
            account.IsValid = 0;  // 待审核
            account.OwnerCode = platform.CustomerCode;

            #endregion account

            var cusDao = new CustomerDao();
            using (var ts = new TransactionScope())
            {
                cusDao.Insert(cus);
                _dao.Insert(account);

                ts.Complete();
            }
            return cus.Code;
        }

        #region 注册信息

        /// <summary>
        ///  添加客户注册信息
        /// </summary>
        public void AddCustomerRegistration(CustomerRegistrationModel model)
        {
            new CustomerRegistrationDao().Insert(model);
        }

        /// <summary>
        /// 取得客户注册信息
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public CustomerRegistrationModel GetCustomerRegistration(string customerCode)
        {
            string sql = " select * from CustomerContract where customerCode=@0";

            return new CustomerRegistrationDao().FirstOrDefault(sql, customerCode);
        }

        #endregion 注册信息

        #region 审核管理

        /// <summary>
        /// 审核账号
        /// 0:未审核
        /// 1:已审核
        /// 2：审核不通过
        ///
        /// return 100 成功
        /// 0：账号已存在！无法通过审核
        /// </summary>
        public int AuditAccount(AuditAccountEditVModel vModel, CrmAccountModel currentUser)
        {
            int customerState = vModel.Customer.CustomerState;
            var acc = GetById(vModel.Account.Code);
            if (vModel.Customer.CustomerState == 1 && GetByLoginName(vModel.Account.LoginName) != null)
                return 0;

            //===========账号信息================================
            acc.Name = vModel.Account.Name;
            acc.Sex = vModel.Account.Sex;
            acc.Mobile = vModel.Account.Mobile;
            acc.Email = vModel.Account.Email;
            acc.Phone = vModel.Account.Phone;
            acc.QQ = vModel.Account.QQ;
            acc.ModifiedBy = currentUser.Code;
            acc.ModifiedTime = DateTime.Now;

            //===========客户信息================================
            var cusDao = new CustomerDao();
            var cus = cusDao.GetById(acc.CustomerCode);
            cus.Name = vModel.Customer.Name;
            cus.FastCode = vModel.Customer.FastCode;
            cus.Head = vModel.Customer.Head;
            cus.Mobile = vModel.Customer.Mobile;
            cus.Phone = vModel.Customer.Phone;
            cus.Address = vModel.Customer.Address;
            cus.CreditLine = vModel.Customer.CreditLine;
            cus.PaymentType = vModel.Customer.PaymentType;
            cus.SalerCode = vModel.Customer.SalerCode;
            cus.Remarks = vModel.Customer.Remarks;
            cus.ModifiedBy = currentUser.Code;
            cus.ModifiedTime = DateTime.Now;

            if (customerState == 1)
            {
                acc.IsValid = 1;
                cus.CustomerState = 1;
                cus.IsValid = 1;
            }
            else if (customerState == 2)
            {
                acc.IsValid = 0;
                cus.IsValid = 0;
                cus.CustomerState = 2;
            }

            using (var ts = new TransactionScope())
            {
                cusDao.Update(cus);
                _dao.Update(acc);

                ts.Complete();
            }

            return 100;
        }

        /// <summary>
        /// 查询未审核，审核不通过的账号
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<CrmAccountModel> SearchNoAuditAccount(CrmAccountModel model, string ownerCode)
        {
            Sql sql = new Sql();

            sql.Append("select a.Code, a.loginName, b.* ");
            sql.Append(" from CrmAccount a ");
            sql.Append(" left join CrmCustomer b on a.CustomerCode=b.Code ");
            sql.Append(" where b.CustomerState=0 and b.OwnerCode=@0", ownerCode);

            var list = _dao.Query<CrmAccountModel, CrmCustomerModel>(sql.SQL, sql.Arguments);

            return list.ToList();
        }

        /// <summary>
        /// 获取未审核客户的数量
        /// </summary>
        /// <returns></returns>
        public int GetNoAuditCustomerCnt(string ownerCode)
        {
            string sql = "select count(1) from CrmCustomer where ownerCode=@0 and CustomerState=0";
            var value = _dao.ExecuteScalar<int>(sql, ownerCode);
            return value;
        }

        #endregion 审核管理

        #region Admin移植

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<CrmAccountModel> AdminGetPagedList(AccountVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append(@" select ca.*, cc.Name as CustomerName
                          from CrmAccount ca
                            inner join CrmCustomer cc on ca.CustomerCode = cc.Code ")
              .Append(" where ca.AccountType in (1,2) ");

            if (!vModel.Account.LoginName.IsNullOrEmpty())
                sql.Append(" and ca.LoginName like @0", AnsiLike(vModel.Account.LoginName));
            if (!vModel.Account.Name.IsNullOrEmpty())
                sql.Append(" and ca.Name like @0", AnsiLike(vModel.Account.Name));
            if (!vModel.Account.CustomerName.IsNullOrEmpty())
                sql.Append(" and ca.CustomerName like @0", AnsiLike(vModel.Account.CustomerName));
            if (!vModel.Account.Mobile.IsNullOrEmpty())
                sql.Append(" and ca.Mobile like @0", AnsiLike(vModel.Account.Mobile));
            if (vModel.Account.DepartCode > 0)
                sql.Append(" and ca.DepartCode=@0", vModel.Account.DepartCode);

            sql.Append(" order by ca.ModifiedTime desc ");
            return _dao.Pager(vModel.Accounts.PageIndex, vModel.Accounts.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 更新账户信息
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public int AdminUpdateTrans(AccountEditVModel vModel)
        {
            using (var ts = _dao.GetTransaction())
            {
                //保存账户基本信息
                _dao.Update(vModel.Account);

                ts.Complete();
            }
            return 1;
        }

        /// <summary>
        /// 添加账户
        /// 保存账户基本信息
        /// 保存关联目的地  保存关联角色
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public int AdminAddTrans(AccountEditVModel vModel)
        {
            using (var ts = _dao.GetTransaction())
            {
                //保存账户基本信息
                _dao.Insert(vModel.Account);

                ts.Complete();
            }
            return 1;
        }

        /// <summary>
        /// 通过ownercode取得客户信息列表
        /// </summary>
        /// <returns></returns>
        public List<KeyValueBean> AdminGetAllCustomerBeans()
        {
            string sql = @"SELECT Code AS `Key`, Name AS Value FROM CrmCustomer WHERE IsValid=1 AND IsOwner=1";
            return _dao.Query<KeyValueBean>(sql).ToList();
        }

        #endregion Admin移植

        #region 组

        /// <summary>
        /// 获取组里所有用户
        /// </summary>
        /// <param name="groupId">组别Id</param>
        /// <returns></returns>
        public List<CrmAccountModel> GetAccountByTeam(string ownerCode, string TeamID)
        {
            var sql = new Sql();
            sql.Append(@"SELECT ca.* FROM CrmAccount ca, TeamAccountMap tam
WHERE ca.Code=tam.AccountCode AND ca.IsValid=1 AND ca.OwnerCode=@0 AND tam.TeamID = @1", ownerCode, TeamID);

            //sql.Append(@" ORDER BY ca.IsValid DESC");

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        #endregion 组

        #region 联系人

        public int DeleteContact(string contactId)
        {
            Sql sql = new Sql();
            sql.Append(" UPDATE CrmAccount SET IsValid=0 WHERE Code=@0", contactId);
            return _dao.Execute(sql.SQL, sql.Arguments);
        }

        public void AddContact(CrmAccountModel model, string[] role)
        {
            _dao.Insert(model);
            AddRole(model.Code, role);
        }

        public int UpdateContact(CrmAccountModel model, string[] role)
        {
            int row = _dao.Update(model);

            var rr = GetSelectedRoleIds(model.Code);
            // 添加没有的
            AddRole(model.Code, role.Except(rr).ToArray());
            // 删除废弃的
            DeleteRole(model.Code, rr.Except(role).ToArray());

            return row;
        }

        /// <summary>
        /// 联系人 更新状态
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="customerCode"></param>
        /// <param name="state"></param>
        public int UpdateContactState(string teamId, string customerCode, int state)
        {
            Sql sql = new Sql();
            sql.Append(" UPDATE CrmAccount SET SalerState=@2 WHERE CustomerCode=@1 AND TeamId=@0", teamId, customerCode, state);
            return _dao.Execute(sql.SQL, sql.Arguments);
        }

        public int UpdateContactState(string contactCode, int state)
        {
            Sql sql = new Sql();
            sql.Append(" update CrmAccount set SalerState=@1 where Code=@0", contactCode, state);
            return _dao.Execute(sql.SQL, sql.Arguments);
        }

        public int MoveContact(string customerCode, string contactCode, string TeamId, string salerCode, int state)
        {
            Sql sql = new Sql();
            sql.Append(" update CrmAccount set TeamID=@1, SalerCode=@2, SalerState=@3 where Code=@0", contactCode, TeamId, salerCode, state);
            int row = _dao.Execute(sql.SQL, sql.Arguments);

            if (row > 0)
            {
                // 如果这个客户下没有其他联系人 那么连带客户一起转移
                var ll = _dao.Fetch(" select distinct TeamID, SalerCode from CrmAccount where CustomerCode=@0 ", customerCode);

                if (ll.Count() == 1)
                {
                    var item = ll.FirstOrDefault();
                    _dao.Execute(" UPDATE CrmCustomer SET TeamID=@1, SalerCode=@2 where CODE=@0", customerCode, item.TeamID, item.SalerCode);
                }
            }

            return row;
        }

        #endregion 联系人
    }
}