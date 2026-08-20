using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Models.TourDB;
using Lvy.Visa.Models;
using Lvy.VModels;
using System;
using System.Collections.Generic;

namespace Lvy.Visa.VModels
{
    public class OrderQModel : BaseVModel
    {
        public OrderQModel()
        {
            this.FileList = new List<TourFileModel>();
            this.FileKeyList = new List<KeyValueBean>();

        }

        public string OrderCode { get; set; }
        public TpTourBalanceModel MasterOrder { get; set; }
        public VisaOrderModel OrderModel { get; set; }
        public VisaInformationModel ProductModel { get; set; }
        public IList<VisaOperationHistoryModel> HistoryList { get; set; }

        public IList<VisaApplicanterModel> TravellerList { get; set; }

        public IList<TpChildOrderModel> ChildOrderList { get; set; }
        public DateTime? FollowupDate { get; set; }

        public IList<TpOrderPayInModel> PayInList { get; set; }

        public List<TourFileModel> FileList { get; set; }

        public List<KeyValueBean> FileKeyList { get; set; }

    }
}