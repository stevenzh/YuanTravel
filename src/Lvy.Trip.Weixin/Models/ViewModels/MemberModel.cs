using Lvy.Models.WeixinDB;

namespace Lvy.Trip.Weixin.Models
{

    public partial class MemberCardModel
    {
        public string code { get; set; }
        //public string can_consume { get; set; }
        public string user_card_status { get; set; }
        public WeixinCard cardInfo { get; set; }
    }
}
