namespace Lvy.VModels.Finance
{
    /// <summary>
    /// 付款确认显示
    /// </summary>
    public class PayInConfirmVModel
    {
        public string OrderId { get; set; }

        public int OrderCount { get; set; }

        public decimal TotalYs { get; set; }

        public decimal TotalPaid { get; set; }

        public decimal TotalUnPaid { get; set; }

        public decimal CurrentPayment { get; set; }
    }
}