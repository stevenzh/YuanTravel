using Lvy.Models.ProductDB;

namespace Lvy.Trip.Dao.Product
{
    /// <summary>
    /// 旅游线路
    /// </summary>
    public class TpLineDao : YuanDbRepository<TpLineModel>
    {
        public TpLineModel GetByLineId(string lineId)
        {
            return _repo.FirstOrDefault<TpLineModel>("select * from TpLine where LineId=@0", lineId);
        }
    }

    public class TpLineFileDao : YuanDbRepository<TpLineFileModel> { }

    public class TpLineRouteDao : YuanDbRepository<TpLineRouteModel> { }

    public class TpLineSuiteDao : YuanDbRepository<TpLineSuiteModel> { }

    public class TpLineTagMapDao : YuanDbRepository<TpLineTagMapModel> { }

    public class TpLineTrafficDao : YuanDbRepository<TpLineTrafficModel> { }

    public class TpLineVisaDao : YuanDbRepository<TpLineVisaModel> { }

    public class TpLineAdminDao : YuanDbRepository<TpLineAdminModel> { }

    public class TpLineBusPointDao : YuanDbRepository<TpLineBusPointModel> { }

    public class TpLineCostRuleDao : YuanDbRepository<TpLineCostRuleModel> { }

    /// <summary>
    /// 产品单元
    /// </summary>
    public class TpProductDao : YuanDbRepository<TpProductModel> { }
}