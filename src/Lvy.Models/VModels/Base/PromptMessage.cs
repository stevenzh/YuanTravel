namespace Lvy.VModels.Base
{
    public class PromptMessage
    {
        public string State { get; set; }

        public string OrderCount { get; set; }

        public string TktOrderCount { get; set; }

        public string NoAuditCustomerCount { get; set; }

        ///// <summary>
        ///// 请求时间 (yyyy/MM/dd hh:mm:ss)
        ///// </summary>
        //public string RequestTime { get; set; }
    }
}