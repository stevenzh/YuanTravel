using Lvy.Models;
using Lvy.Models.WeixinDB;
using Lvy.Trip.Dao.Weixin;
using PetaPoco;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Weixin
{
    public class MessageBiz : BaseBiz
    {
        private readonly MemberMessageDao _dao = new MemberMessageDao();

        public void AddMessage(MemberMessage model)
        {
            _dao.Insert(model);
        }

        public PagedList<MemberMessage> GetMessages(string OpenID, int pageIndex, int pageSize)
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxMemberMessages where OpenID=@0 ", Ansi(OpenID));

            var list = _dao.Pager(pageIndex, pageSize, sql.SQL, sql.Arguments);

            return list;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="OpenID"></param>
        /// <returns></returns>
        public IList<MemberMessage> GetMessages(string OpenID)
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxMemberMessages where OpenID=@0 ", Ansi(OpenID));

            var list = _dao.Query(sql.SQL, sql.Arguments).ToList();

            return list;
        }
    }
}