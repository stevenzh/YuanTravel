using Lvy.Models.WeixinDB;
using Lvy.Trip.Dao.Weixin;
using PetaPoco;
using System;

namespace Lvy.Trip.Biz.Weixin
{
    /// <summary>
    /// 销售二维码
    /// </summary>
    public class QrBiz : BaseBiz
    {
        private readonly MemberQRDao _dao = new MemberQRDao();

        public int GetMaxQr()
        {
            int max = _dao.Single("select max(SceneID) as SceneID from WxMemberQRs ").SceneID;
            return max;
        }

        public void SaveQr(int id, int newq, string ticket, string employeeid, string ownerCode)
        {
            var q = new MemberQR
            {
                SceneID = newq,
                Ticket = ticket,
                EmployeeID = employeeid,
                CreatedDate = DateTime.Now,
                OwnerCode = ownerCode
            };
            int qrid = _dao.Insert(q).ToInt();

            new MemberBiz().UpdateUserQr(id, qrid);
        }

        public MemberQR getQrCode(int qrID)
        {
            return _dao.GetById(qrID);
        }

        public MemberQR getQrByEmployee(string employeeID)
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxMemberQRs where EmployeeID=@0 ", Ansi(employeeID));

            var entity = _dao.FirstOrDefault(sql.SQL, sql.Arguments);
            return entity;
        }
    }
}