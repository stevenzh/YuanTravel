namespace Lvy.Models
{

    /// <summary>
    /// 订单操作结果
    /// </summary>
    public enum OrderResultState
    {
        #region 订单错误编号 100 - 200

        Code100 = 100, // 成功
        Code101 = 101, // 座位已占
        Code102 = 102, // 座位已锁定
        Code110 = 110, // 可用名额小于预定名额
        Code199 = 199, // 未知异常
        #endregion

    }

    public enum OrderState
    {
        Code100 = 100, // 成功
        Code200 = 200, // 座位已占
        Code300 = 102, // 座位已锁定
        Code110 = 110, // 可用名额小于预定名额
    }

    /// <summary>
    /// 文件类型
    /// </summary>
    public enum MediaType { image, document, voice, video }

    /// <summary>
    /// 收款目的 定金|团款
    /// </summary>
    public enum PayInUse { charge, deposit }

    /// <summary>
    /// 附件类型
    /// </summary>
    public enum FileSourceType {
        Tourist = 1,        // 游客资料
        PayVoucher = 2,     // 缴款凭证
        Bill = 3,           // 账单
        ReturnBIll = 4,     // 回传账单
        Notice = 5,         // 出团通知
        Product = 11,       // 产品行程
        TourPayVoucher = 21,// 团付款凭证
    }
}
