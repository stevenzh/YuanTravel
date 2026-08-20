using Lvy.Models.WeixinDB;
using Lvy.Trip.Dao.Weixin;
using PetaPoco;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Weixin
{
    public class CardBiz : BaseBiz
    {
        private readonly WeixinCardDao _dao = new WeixinCardDao();

        public WeixinCard GetById(string code)
        {
            return _dao.GetById(code);
        }

        public void AddCard(WeixinCard model)
        {
            _dao.Insert(model);
        }

        public List<WeixinCard> GetCards()
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxCards ");
            var list = _dao.Query(sql.SQL, sql.Arguments).ToList();
            return list;
        }
    }
}