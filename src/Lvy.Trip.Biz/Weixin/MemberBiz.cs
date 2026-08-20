using log4net;
using Lvy.Models;
using Lvy.Models.WeixinDB;
using Lvy.Trip.Dao.Weixin;
using Lvy.VModels.Weixin;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.Trip.Biz.Weixin
{
    /// <summary>
    /// 微信绑定客户信息
    /// </summary>
    public class MemberBiz : BaseBiz
    {
        private ILog logger = LogManager.GetLogger(typeof(MemberBiz));
       
        private readonly MemberDao _dao = new MemberDao();
        public MessageBiz messageBiz = new MessageBiz();
        public AddressBiz addressBiz = new AddressBiz();

        /// <summary>
        /// 检查用户是否绑定
        /// </summary>
        /// <param name="openID"></param>
        /// <returns></returns>
        public int WeixinIsBind(string openID)
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxMembers where OpenID=@0 and IsValid=1", Ansi(openID));

            Member user = _dao.FirstOrDefault(sql.SQL, sql.Arguments);
            int bind = 0;
            if (user != null)
                bind = user.Binding;

            return bind;
        }

        /// <summary>
        /// 关注
        /// </summary>
        /// <param name="openID"></param>
        /// <param name="nike"></param>
        /// <param name="lang"></param>
        /// <param name="sex"></param>
        /// <param name="city"></param>
        /// <param name="province"></param>
        /// <param name="county"></param>
        /// <param name="imgurl"></param>
        /// <param name="subscribe_time"></param>
        /// <param name="qrscene_"></param>
        public void Subscribe(string ownerCode, string openID, string nike, string lang, int sex, string city, string province,
            string county, string imgurl, DateTime subscribe_time, string qrscene_)
        {
            logger.Info("Subscribe New,Nike: " + nike);
            Sql sql = new Sql();
            sql.Append(" select * from WxMembers where OpenID=@0 and IsValid=1", Ansi(openID));
            Member user = _dao.FirstOrDefault(sql.SQL, sql.Arguments);

            if (user != null)
            {
                user.OwnerCode = ownerCode;
                user.NickName = nike;
                user.Language = lang;
                user.Sex = sex;
                user.City = city;
                user.Province = province;
                user.Country = county;
                user.HeadImgUrl = imgurl;
                user.SubscribeTime = subscribe_time;
                user.Subscribe = "1";
                user.Binding = 0;
                user.Approved = 0;
                user.IsValid = 1;

                _dao.Update(user);
            }
            else
            {
                _dao.Insert(new Member
                {
                    OpenID = openID,
                    OwnerCode = ownerCode,
                    NickName = nike,
                    Language = lang,
                    Sex = sex,
                    City = city,
                    Province = province,
                    Country = county,
                    HeadImgUrl = imgurl,
                    SubscribeTime = subscribe_time,
                    Subscribe = "1",
                    Binding = 0,
                    Sales = "",
                    IsValid = 1
                });
            }
        }

        /// <summary>
        /// 退订
        /// </summary>
        /// <param name="p"></param>
        public void Unsubscribe(string openID)
        {
            Sql sql = new Sql();
            sql.Append(" set Subscribe='0', UnsubscribeTime=now() where OpenID=@0", Ansi(openID));

            _dao.Update(sql.SQL, sql.Arguments);
        }

        public void UnsubscribeAll()
        {
            Sql sql = new Sql();
            sql.Append(" set Subscribe='0' where Subscribe='1'");

            _dao.Update(sql.SQL, sql.Arguments);
        }

        public void UpdateMember(string ownerCode, string openID, string subscribe, string nike, int sex, string city,
            string province, string country, string imgurl, string lang, DateTime subscribeTime)
        {
            try
            {
                Sql sql = new Sql();
                sql.Append(" select * from WxMembers where OpenID=@0 and IsValid=1", Ansi(openID));
                var user = _dao.FirstOrDefault(sql.SQL, sql.Arguments);

                if (user != null)
                {
                    user.OwnerCode = ownerCode;
                    user.NickName = nike;
                    user.Sex = sex;
                    user.City = city;
                    user.Province = province;
                    user.Country = country;
                    user.HeadImgUrl = imgurl;
                    user.Subscribe = subscribe;
                    if (subscribeTime != DateTime.MinValue)
                        user.SubscribeTime = subscribeTime;
                    user.SyncDate = DateTime.Now;

                    _dao.Update(user);
                }
                else
                {
                    var user1 = new Member
                    {
                        OpenID = openID,
                        OwnerCode = ownerCode,
                        NickName = nike,
                        Language = lang,
                        Sex = sex,
                        City = city,
                        Province = province,
                        Country = country,
                        HeadImgUrl = imgurl,
                        Subscribe = subscribe,
                        SubscribeTime = DateTime.Now,
                        Binding = 0,
                        SyncDate = DateTime.Now,
                        IsEmployee = 0,
                        IsValid = 1
                    };
                    if (subscribeTime != DateTime.MinValue)
                        user1.SubscribeTime = subscribeTime;
                    _dao.Insert(user1);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "OpenID:" + openID, ex);
            }
        }

        public Member GetMemberByOpenID(string openID)
        {
            Sql sql = new Sql();
            sql.Append(@" select c.*, a.Value as SubscribeValue, b.Value as BindingValue from WxMembers c
                                     inner join BaseDictionaryDetail a on a.`Key`=c.Subscribe
                                     inner join BaseDictionaryDetail b on b.`Key`=c.Binding
                          where a.Name='WeixinSubscribeEnum' and b.Name='MemberBindingEnum' and c.OpenID=@0
                          order by c.SubscribeTime DESC ", Ansi(openID));

            var model = _dao.FirstOrDefault(sql.SQL, sql.Arguments);

            if (model != null)
            {
                model.Messages = messageBiz.GetMessages(model.OpenID);
                model.AddressList = addressBiz.GetAddress(model.MemberID);
            }

            return model;
        }

        public Member GetMemberByID(int memberID)
        {
            var model = _dao.GetById(memberID);
            if (model != null)
            {
                model.Messages = messageBiz.GetMessages(model.OpenID);
                // model.MessagePageList = GetMessages(model.OpenID, 1, 10);
            }
            return model;
        }

        public Member GetMemberByAccount(string accountCode)
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxMembers where EmployeeID=@0 ", Ansi(accountCode));
            var model = _dao.FirstOrDefault(sql.SQL, sql.Arguments);
            if (model != null)
            {
                model.Messages = messageBiz.GetMessages(model.OpenID);
            }
            return model;
        }

        public IList<Member> GetMember(MemberQModel model)
        {
            Sql sql = new Sql();
            sql.Append(@" select c.*, a.Contents as SubscribeValue, b.Contents as BindingValue from WxMembers c
                                     inner join Dictionaries a on a.KeyValue=c.Subscribe
                                     inner join Dictionaries b on b.KeyValue=c.Binding
                          where a.Identifier='WeixinSubscribeEnum' and b.Identifier='MemberBindingEnum' and c.OwnerCode=@0
                          order by c.SubscribeTime DESC ", Ansi(model.OwnerCode));

            if (!model.Name.IsNullOrEmpty())
                sql.Append(" and (c.NickName like @0 or c.RealName like @0) ", AnsiLike(model.Name));
            if (!model.Sales.IsNullOrEmpty())
                sql.Append(" and c.Sales = @0 ", Ansi(model.Sales));
            if (!model.OpenID.IsNullOrEmpty())
                sql.Append(" and c.OpenID = @0 ", Ansi(model.OpenID));
            if (!model.Binding.IsNullOrEmpty())
                sql.Append(" and c.Binding = @0 ", Ansi(model.Binding));
            if (!model.Approved.IsNullOrEmpty())
                sql.Append(" and c.Approved = @0 ", Ansi(model.Approved));
            //if (!model.Subscribe.IsNullOrEmpty())
            //    sql.Append(" and c.Subscribe = @0 ", Ansi(model.Subscribe));

            var list = _dao.Query(sql.SQL, sql.Arguments).ToList();
            return list;
        }

        public PagedList<Member> GetPageMember(MemberQModel model)
        {
            Sql sql = new Sql();
            sql.Append(@" select c.* from WxMembers c
                          where c.OwnerCode=@0 ", Ansi(model.OwnerCode));

            if (!model.Name.IsNullOrEmpty())
                sql.Append(" and (c.NickName like @0 or c.RealName like @0) ", AnsiLike(model.Name));
            if (!model.Sales.IsNullOrEmpty())
                sql.Append(" and c.Sales = @0 ", Ansi(model.Sales));
            if (!model.OpenID.IsNullOrEmpty())
                sql.Append(" and c.OpenID = @0 ", Ansi(model.OpenID));
            if (!model.Binding.IsNullOrEmpty())
                sql.Append(" and c.Binding = @0 ", Ansi(model.Binding));
            if (!model.Approved.IsNullOrEmpty())
                sql.Append(" and c.Approved = @0 ", Ansi(model.Approved));
            //if (!model.Subscribe.IsNullOrEmpty())
            //    sql.Append(" and c.Subscribe = @0 ", Ansi(model.Subscribe));
            if (!string.IsNullOrEmpty(model.Employee))
            {
                int s = Convert.ToInt32(model.Employee);
                if (s == 1)
                    sql.Append(" and c.EmployeeID is not null ");
                else
                    sql.Append(" and c.EmployeeID is null ");
            }
            sql.Append(" order by c.SubscribeTime DESC ");

            var list = _dao.Pager(model.MemberPageList.PageIndex, model.MemberPageList.PageSize, sql.SQL, sql.Arguments);
            return list;
        }

        public void SaveMember(Member model)
        {
            try
            {
                var user = _dao.GetById(model.MemberID);

                if (user != null)
                {
                    user.RealName = model.RealName;
                    user.Sales = model.Sales;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Approved = model.Approved;
                    user.Binding = model.Binding;
                    user.CustomerName = model.CustomerName;

                    user.HideShared = model.HideShared;
                    user.EmployeeID = model.EmployeeID;
                    user.LogoUrl = model.LogoUrl;

                    _dao.Update(user);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "MemberID:" + model.MemberID, ex);
            }
        }

        public int DelMember(int id)
        {
            var row = _dao.Delete(id);
            if (row > 0)
            {
                //context.MemberLocations.Where(t => t.OpenID == entity.OpenID).Delete();
                //context.MemberMessages.Where(t => t.OpenID == entity.OpenID).Delete();

                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 提交审核的客户
        /// </summary>
        /// <param name="salesman"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public List<Member> GetLastMember(string salesman, int rows)
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxMembers where Sales=@0 and Binding=1 and Approved=0 order by SubscribeTime DESC LIMIT 5 ", Ansi(salesman));
            var list = _dao.Query(sql.SQL, sql.Arguments).ToList();

            return list;
        }

        public int ApprovedMember(int id)
        {
            Sql sql = new Sql();
            sql.Append(" set Approved=1 where MemberID=@0", id);

            int row = _dao.Update(sql.SQL, sql.Arguments);

            if (row > 0)
            {
                return 1;
            }

            return 0;
        }

        public string GetOpenID(string sales)
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxMembers where RealName=@0 and EmployeeID is not null ", Ansi(sales));
            Member user = _dao.FirstOrDefault(sql.SQL, sql.Arguments);
            return (user == null) ? "" : user.OpenID;
        }

        //public List<StatData> MemberStat()
        //{
        //    var wdate = DateTime.Today.AddDays(-7);
        //    var query = from l in context.Members.Where(t => t.HostID == hostId && t.Subscribe == "1" && t.Binding == "1")
        //                group l by l.Sales into g
        //                select new StatData
        //                {
        //                    UserName = g.Key,
        //                    AllFans = g.Count(),
        //                    LastWeekFans = context.Members.Where(t => t.HostID == hostId && t.Subscribe == "1" && t.Sales == g.Key && t.SubscribeTime > wdate).Count()
        //                };
        //    return query.ToList();
        //}

        public Member GetUserByQr(int qrid)
        {
            Sql sql = new Sql();
            sql.Append(" select * from WxMembers where QrID=@0 ", qrid);
            return _dao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        public void UpdateUserQr(int memberID, int QrID)
        {
            Sql sql = new Sql();
            sql.Append(" set QrID=@1 where MemberID=@0", memberID, QrID);
            int row = _dao.Update(sql.SQL, sql.Arguments);
        }

        public void SendMessage(MemberMessage msg)
        {
            MessageBiz biz = new MessageBiz();
            biz.AddMessage(msg);

            // 客人最后消息时间
            Sql sql = new Sql();
            sql.Append(" set LastMessageTime=now() where OpenID=@0", msg.OpenID);
            int row = _dao.Update(sql.SQL, sql.Arguments);
        }
    }
}