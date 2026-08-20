using Lvy.Models.TicketDB;

namespace Lvy.Trip.Dao.Ticket
{
    public class TktProductDao : YuanDbRepository<TktProductModel>
    {
        public TktProductModel GetByProductId(string productId)
        {
            return _repo.FirstOrDefault<TktProductModel>(" SELECT * FROM TktProduct WHERE ProductId=@0 ", productId);
        }

        public void UpdateQuota(TktProductModel m)
        {
            _repo.Execute(@"UPDATE TktProduct SET UsedQuota=@1, HoldQuota=@2,UsedQuota=@3,StartTime=@4,
EndTime=@5,LimitQuota=@6,BeginBuyTime=@7,LastDate=@8 WHERE ProductID=@0", m.ProductId, m.PlanQuota,
m.HoldQuota, m.UsedQuota, m.StartTime, m.EndTime, m.LimitQuota, m.BeginBuyTime, m.LastDate);
        }

        public void UpdateUsedQuota(TktProductModel model)
        {
            _repo.Execute("UPDATE TktProduct SET UsedQuota=@1 WHERE ProductID=@0 ", model.ProductId, model.UsedQuota);
        }
    }

    public class TktAdminDao : YuanDbRepository<TktAdminModel> { }

    public class TktPriceRuleDao : YuanDbRepository<TktPriceRuleModel> { }

    public class TktRulePriceMapDao : YuanDbRepository<TktRulePriceMapModel> { }

    public class TktPriceDao : YuanDbRepository<TktPriceModel> { }

    public class TktFileDao : YuanDbRepository<TktFileModel> { }

    public class TktOrdersDao : YuanDbRepository<TktOrdersModel> { }

    public class TktTaskOrderDao : YuanDbRepository<TktTaskOrderModel> { }

    public class TktCategoryDao : YuanDbRepository<TktCategoryModel> { }
}