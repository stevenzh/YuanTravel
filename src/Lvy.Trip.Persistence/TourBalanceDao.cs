using Lvy.Models.OrderDB;
using Lvy.Models.TourDB;

namespace Lvy.Trip.Dao.Tour
{
    public class TpTourBalanceDao : YuanDbRepository<TpTourBalanceModel> { }

    public class TpTourCostDao : YuanDbRepository<TpTourCostModel> { }

    public class TpTourPaymentDao : YuanDbRepository<TpPaymentModel> { }

    public class TourFileDao : YuanDbRepository<TourFileModel> { }

    public class ViewInvoiceDao : YuanDbRepository<ViewInvoiceModel> { }
    
    public class ViewPayInDao : YuanDbRepository<ViewPayInModel> { }

}