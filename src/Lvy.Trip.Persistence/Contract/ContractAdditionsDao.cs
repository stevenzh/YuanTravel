using Lvy.Models;

namespace Lvy.Trip.Dao.Contract
{
    public class ContractAdditionsDao : YuanDbRepository<ContractAdditions> { }

    public class ContractTouristDao : YuanDbRepository<ContractTourist> { }

    public class ContractShoppingDao : YuanDbRepository<ContractShopping> { }

    public class ContractPayItemDao : YuanDbRepository<ContractPayItem> { }

    public class ContractInfoDao : YuanDbRepository<ContractInfo> { }

    public class ContractFilesDao : YuanDbRepository<ContractFiles> { }
}