using Lvy.Models.WeixinDB;
using Lvy.Trip.Dao.Weixin;
using PetaPoco;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Weixin
{
    public class AddressBiz : BaseBiz
    {
        private readonly MemberAddressDao _dao = new MemberAddressDao();

        /// <summary>
        /// 保存客人地址
        /// </summary>
        /// <param name="model"></param>
        public void AddAdress(MemberAddress model)
        {
            _dao.Insert(model);
        }

        public List<MemberAddress> GetAddress(int MemberID)
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxMemberAddress where MemberID=@0 ", MemberID);
            var list = _dao.Query(sql.SQL, sql.Arguments).ToList();
            return list;
        }
    }
}