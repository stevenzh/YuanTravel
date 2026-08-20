using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using log4net;
using Senparc.Weixin.Helpers;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Containers;
using Lvy.Trip.Weixin.Models;
using Lvy.Models.WeixinDB;
using Lvy.Trip.Biz.Weixin;
using Lvy.Web.Common.Cache;

namespace Lvy.Trip.Weixin.Services
{
    public class WeixinService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(WeixinService));
        private string appId = WebConfigurationManager.AppSettings["WeixinAppId"];
        private string appSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"];
        private MemberBiz memberBiz = new MemberBiz();
        private CardBiz cardBiz = new CardBiz();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="openID"></param>
        /// <param name="content"></param>
        public void SaveWeixinMessage(string openID, string content, DateTime SendDate)
        {
            try
            {
                var member = memberBiz.GetMemberByOpenID(openID);

                // 模板发送消息
                if (member != null)
                {
                    member.LastMessageTime = SendDate;
                    if (content.Length > 10 && string.IsNullOrEmpty(member.EmployeeID))  // 内容长并且不是员工
                    {
                        string NickName = (string.IsNullOrEmpty(member.RealName)) ? member.NickName : member.RealName;
                        if (!string.IsNullOrEmpty(member.CustomerName))
                            NickName = string.Format("{0}({1})", NickName, member.CustomerName);

                        string toOpenID = "ok6cAuKoOc85PZtNdKlSurbNiaGQ"; // 张思涛
                        if (string.IsNullOrEmpty(member.SalesID)) // 负责销售
                        {
                            var employee = memberBiz.GetMemberByAccount(member.SalesID);
                            if (employee != null)
                                toOpenID = employee.OpenID;
                        }
                        else if (!string.IsNullOrEmpty(member.Sales)) // 负责销售
                        {
                            var employee = memberBiz.GetMemberByAccount(member.Sales);
                            if (employee != null)
                                toOpenID = employee.OpenID;
                        }

                        var accessToken = AccessTokenContainer.GetAccessToken(appId);
                        string url = "http://yuanwx.sh-cct.cn/memberadmin/details/" + openID;
                        var testData = new SendMessageData()
                        {
                            first = new TemplateDataItem("您好，您的客户有新的咨询消息。"),
                            keyword1 = new TemplateDataItem(NickName),
                            keyword2 = new TemplateDataItem(content),
                            remark = new TemplateDataItem("咨询时间：" + SendDate.ToString("yyyy-MM-dd HH:mm:ss"))
                        };

                        var result = TemplateApi.SendTemplateMessage(accessToken, toOpenID, "Rr82ARvQYJquaS-DPYFxhhNEGuu8y6jajPdSiwJsar8", url, testData);
                    }
                }

                MemberMessage msg = new MemberMessage();
                msg.OpenID = openID;
                msg.Content = content;
                msg.InOut = 0;
                msg.CreatedDate = DateTime.Now;
                msg.IsCallBack = "0";
                msg.MsgType = "Text";
                new MessageBiz().AddMessage(msg);
            }
            catch (Exception ex)
            {
                logger.Error("发送失败", ex);
            }
        }


        public IList<MemberCardModel> GetAppCard(string openid)
        {
            var CacheKey = "CacheKey=Weixin|Card|List:1002";
            var _getModel = CacheContext.Current.Get(CacheKey);
            var wxcards = new List<WeixinCard>();
            var accessToken = AccessTokenContainer.TryGetAccessToken(appId, appSecret);

            if (_getModel == null)
            {
                wxcards = cardBiz.GetCards();
                CacheContext.Current.Add(CacheKey, wxcards, Configs.cacheDateTime);
            }
            else
            {
                wxcards = ((List<WeixinCard>)_getModel);
            }

            List<MemberCardModel> mclist = new List<MemberCardModel>();
            var result = CardApi.GetCardList(accessToken, openid);
            foreach (var dd in result.card_list)
            {
                var ddd1 = CardApi.CardGet(accessToken, dd.code);
                var ddd = wxcards.Where(t => t.ID == dd.card_id).FirstOrDefault();
                if (ddd != null)
                {
                    mclist.Add(new MemberCardModel
                    {
                        code = dd.code,
                        //can_consume = ddd1.can_consume,
                        cardInfo = ddd
                    });
                }
            }

            return mclist;
        }


        public decimal GetCardDis(string cardid)
        {
            decimal dd = 0;
            var CacheKey = "CacheKey=Weixin|Card|List:1002";
            var _getModel = CacheContext.Current.Get(CacheKey);
            var wxcards = new List<WeixinCard>();
            var accessToken = AccessTokenContainer.TryGetAccessToken(appId, appSecret);

            if (_getModel == null)
            {
                wxcards = cardBiz.GetCards();
                CacheContext.Current.Add(CacheKey, wxcards, Configs.cacheDateTime);
            }
            else
            {
                wxcards = ((List<WeixinCard>)_getModel);
            }


            var ddd = wxcards.Where(t => t.ID == cardid).FirstOrDefault();
            if (ddd != null)
            {
                dd = ddd.ReduceCost.Value;
            }

            return dd;
        }

        public void UpdateWeixinCard()
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(appId, appSecret);
            var result1 = CardApi.CardBatchGet(accessToken, 0, 20, null);
            foreach (var card in result1.card_id_list)
            {
                var dbcard = cardBiz.GetById(card);
                var detail = CardApi.CardDetailGet(accessToken, card);
                string card_type = detail.card.card_type;

                if (card_type == "CASH")
                {
                    if (dbcard == null)
                    {
                        dbcard = new WeixinCard
                        {
                            ID = card,
                            CardType = detail.card.card_type,
                            LeastCost = detail.card.cash.least_cost / 100,
                            ReduceCost = detail.card.cash.reduce_cost / 100,
                            Description = detail.card.cash.base_info.description,
                            Status = detail.card.cash.base_info.status,
                            Title = detail.card.cash.base_info.title,
                            DateType = detail.card.cash.base_info.date_info.type
                        };
                        if (dbcard.DateType == "DATE_TYPE_FIX_TIME_RANGE")
                        {
                            dbcard.BeginTimestamp = DateTimeHelper.GetDateTimeFromXml(detail.card.cash.base_info.date_info.begin_timestamp);
                            dbcard.EndTimestamp = DateTimeHelper.GetDateTimeFromXml(detail.card.cash.base_info.date_info.end_timestamp);
                        }

                        cardBiz.AddCard(dbcard);
                    }
                    else
                    {
                        if (dbcard.DateType == "DATE_TYPE_FIX_TIME_RANGE")
                        {
                            dbcard.BeginTimestamp = DateTimeHelper.GetDateTimeFromXml(detail.card.cash.base_info.date_info.begin_timestamp);
                            dbcard.EndTimestamp = DateTimeHelper.GetDateTimeFromXml(detail.card.cash.base_info.date_info.end_timestamp);
                        }
                    }
                }
                else if (card_type == "GENERAL_COUPON")
                {
                    if (dbcard == null)
                    {
                        dbcard = new WeixinCard
                        {
                            ID = card,
                            CardType = detail.card.card_type,
                            Description = detail.card.general_coupon.default_detail,
                            Status = detail.card.general_coupon.base_info.status,
                            Title = detail.card.general_coupon.base_info.title,
                            DateType = detail.card.general_coupon.base_info.date_info.type,
                            // DATE_TYPE_FIX_TIME_RANGE
                            //begin_timestamp = detail.card.general_coupon.base_info.date_info.begin_timestamp,
                            //end_timestamp = detail.card.general_coupon.base_info.date_info.end_timestamp
                        };

                        cardBiz.AddCard(dbcard);
                    }
                    else
                    {

                    }
                }
            }
        }
    }
}