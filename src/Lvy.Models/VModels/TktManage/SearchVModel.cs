using Lvy.Models;
using System;

namespace Lvy.VModels.Ticket
{
    public class SearchVModel : BaseVModel
    {
        #region 查询条件

        public string Keyword { get; set; }

        public string DestId
        {
            get
            {
                if (Keyword.IsNullOrEmpty())
                    return "";

                string[] temps = Keyword.Split('-');
                if (temps[0] == "0")
                    return temps[1];
                return "";
            }
        }

        public string ProductId
        {
            get
            {
                if (Keyword.IsNullOrEmpty())
                    return "";
                string[] temps = Keyword.Split('-');
                if (temps[0] == "1")
                    return temps[1];
                return "";
            }
        }

        #endregion 查询条件

        public PagedList<TktProductVModel> Products { get; set; }
    }
}