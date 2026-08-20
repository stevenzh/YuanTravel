using Lvy.Models.CrmDB;

namespace Lvy.Trip.Dao.Crm
{
    /// <summary>
    ///
    /// </summary>
    public class CustomerDao : YuanDbRepository<CrmCustomerModel> { }

    /// <summary>
    ///
    /// </summary>
    public class CustomerFileDao : YuanDbRepository<CustomerFileModel> { }

    /// <summary>
    ///
    /// </summary>
    public class CustomerHoldDao : YuanDbRepository<CustomerHoldModel> { }

    /// <summary>
    ///
    /// </summary>
    public class CustomerPolicyDao : YuanDbRepository<CustomerPolicyModel> { }

    /// <summary>
    ///
    /// </summary>
    public class CustomerRegistrationDao : YuanDbRepository<CustomerRegistrationModel> { }
}