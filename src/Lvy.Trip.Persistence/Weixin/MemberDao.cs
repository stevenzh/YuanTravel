using Lvy.Models.WeixinDB;

namespace Lvy.Trip.Dao.Weixin
{
    /// <summary>
    ///
    /// </summary>
    public class MemberDao : WeixinDbRepository<Member> { }

    /// <summary>
    ///
    /// </summary>
    public class MemberAddressDao : WeixinDbRepository<MemberAddress> { }

    /// <summary>
    ///
    /// </summary>
    public class MemberLocationDao : WeixinDbRepository<MemberLocation> { }

    /// <summary>
    ///
    /// </summary>
    public class MemberMessageDao : WeixinDbRepository<MemberMessage> { }

    /// <summary>
    ///
    /// </summary>
    public class MemberQRDao : WeixinDbRepository<MemberQR> { }
}