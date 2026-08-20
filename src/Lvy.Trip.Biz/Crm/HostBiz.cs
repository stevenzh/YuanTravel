using Lvy.Models;
using Lvy.Models.CrmDB;
using Lvy.Trip.Dao.Crm;
using Lvy.VModels.Crm;
using PetaPoco;
using System;
using System.Linq;
using System.Transactions;

namespace Lvy.Trip.Biz.Crm
{
    public class HostBiz : BaseBiz
    {
        private static readonly SysPlatformDao _dao = new SysPlatformDao();
        private static readonly CustomerDao _customerDao = new CustomerDao();

        /// <summary>
        ///  获得一个对象
        /// </summary>
        /// <param name="cusCode"></param>
        /// <returns></returns>
        public SysPlatformModel GetPlatform(string cusCode)
        {
            string sql = "SELECT * FROM SysPlatform WHERE CustomerCode=@0";
            return _dao.Query<SysPlatformModel>(sql, cusCode).FirstOrDefault();
        }

        public SysPlatformModel GetHostBy(string code)
        {
            string sql = @"SELECT b.*, a.ShortName, a.Code, a.Remarks FROM SysPlatform b
INNER JOIN CrmCustomer a ON a.Code=b.CustomerCode
WHERE a.Code=@0";
            return _dao.Query<SysPlatformModel, CrmCustomerModel>(sql, code).FirstOrDefault();
        }

        /// <summary>
        ///  获取商户列表
        /// </summary>
        /// <returns></returns>
        public PagedList<SysPlatformModel> GetHostPagedList(HostVModel vModel)
        {
            Sql sql = new Sql();
            sql.Append(@"SELECT c.* FROM SysPlatform  c
LEFT JOIN CrmCustomer a ON c.CustomerCode = a.Code
WHERE c.IsValid=1 ");  // 商户

            if (!vModel.Customer.Name.IsNullOrEmpty())
                sql.Append(" AND c.Name LIKE @0", AnsiLike(vModel.Customer.Name));

            //sql.Append(" ORDER BY c.ModifiedTime DESC ");
            var list = _dao.Pager(vModel.Customers.PageIndex, vModel.Customers.PageSize, sql.SQL, sql.Arguments);

            return list;
        }

        /// <summary>
        /// 添加商户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int AddTrans(SysPlatformModel model)
        {
            using (var ts = _dao.GetTransaction())
            {
                _customerDao.Insert(model.CrmCustomer);
                _dao.Insert(model);
                ts.Complete();
            }

            return 1;
        }

        /// <summary>
        /// 更新客户资料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateTrans(SysPlatformModel model)
        {
            SysPlatformDao platformDao = new SysPlatformDao();
            using (var ts = new TransactionScope())
            {
                //_customerDao.Update(model.CrmCustomer);
                _dao.Update(model);
                ts.Complete();
            }
            return 1;
        }

        public void Update(SysPlatformModel platform)
        {
            throw new NotImplementedException();
        }
    }
}