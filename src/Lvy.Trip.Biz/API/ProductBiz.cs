using System;
using System.Collections.Generic;
using System.Linq;
using PetaPoco;
using Lvy.APIVModels.Req;
using Lvy.APIVModels.Res;
using Lvy.Trip.Dao.Crm;
using Lvy.Trip.Dao.Product;
using Lvy.Web.Common;
using Lvy.Trip.Biz;

namespace Lvy.API.Biz.Product
{
    public class ProductBiz
    {
        private readonly TpLineDao _dao = new TpLineDao();

        /// <summary>
        /// 搜索产品 (一小时缓存)
        /// </summary>
        /// <param name="productReq"></param>
        /// <returns></returns>
        //public GetProductsResponse SearchProducts(GetProductsRequest productReq)
        //{
        //    GetProductsResponse productRes = new GetProductsResponse();

        //    if (CacheContext.Current.Get(Consts.Destination) == null)
        //    {
        //        var list = new DestinationDao().GetDests();
        //        CacheContext.Current.Add(Consts.Destination, list);
        //    }

        //    if (CacheContext.Current.Get(Consts.TourProduct) == null)
        //    {
        //        var outCitiesDb = DictionaryBiz.GetEnumsBy(Enums.OutCityEnum);//出发地
        //        //查询所有在有效期内的团计划
        //        Sql sql = new Sql();
        //        sql.Append(@"SELECT TpLine.Id AS LineId,TpLine.LineSpecial,TpLine.TrafficType,TpLine.LineState,TpLine.Themes,TpLine.CustomerCode,TpLine.CustomerName,");
        //        sql.Append(@"TpTourPlan.Id AS TourId, TpTourPlan.TourNo As TourName,TpTourPlan.OutDate,TpTourPlan.BookingLastDays,TpTourPlan.Price,TpTourPlan.Source,");
        //        sql.Append(@"TpQuota.PlanQuota,TpQuota.UseQuota,");
        //        sql.Append(@"TpTourPlan.SingleRoom,TpTourPlan.TourState,TpPrice.TeJiaFanLi");
        //        sql.Append(@" FROM TpLine INNER JOIN TpTourPlan ON TpTourPlan.LineId=TpLine.LineId");
        //        sql.Append(@" INNER JOIN TpTourQuotaMap ON TpTourQuotaMap.TourId=TpTourPlan.Id");
        //        sql.Append(@" INNER JOIN TpQuota ON TpQuota.Id=TpTourQuotaMap.QuotaId");
        //        sql.Append(@" INNER JOIN TpPrice ON TpPrice.TourId=TpTourPlan.Id AND TpPrice.IsStandard=1 ");
        //        sql.Append(@" WHERE TpLine.OwnerCode=@0", productReq.OwnerCode);
        //        sql.Append(@" AND TpLine.IsValid=1"); //有效
        //        sql.Append(@" AND TpLine.LineState=3"); //上线
        //        sql.Append(@" AND TpTourPlan.TourState=3"); //上线（团计划）
        //        sql.Append(@" AND TpTourPlan.BookingLastDays>=@0", DateTime.Today);
        //        sql.Append(@" AND TpTourPlan.BookingLastDays<=@0", DateTime.Today.AddDays(productReq.QueryDay - 1));
        //        if (productReq.LineType > 0)
        //            sql.Append(@" AND TpLine.LineType=@0", productReq.LineType);
        //        var tourPlanList =
        //            _dao.Query<TpTourModel>(sql.SQL, sql.Arguments).ToList();                    //获得当前有效的团计划列表

        //        //todo 
        //        tourPlanList.ForEach(item => item.TourState = (item.LineState == 3 && item.TourState == 3) == true ? 1 : 0);//更新当前团计划的可用状态
        //        tourPlanList.ForEach(item => item = GetPriceInfo(item));//获取每个团计划的价格信息

        //        //遍历取出所有的线路ID
        //        var groupTourModelList = tourPlanList.GroupBy(item => item.LineId).ToList();
        //        var groupLineIdArray = groupTourModelList.Select(o => o.Key).ToArray();

        //        Sql sqlProduct = new Sql();
        //        sqlProduct.Append("select LineId,TravelDays,MoveUpDays,DepartDest,ArriveDest As ArriveDestId,LineType,TrafficType,LineSpecial,")
        //            .Append("PriceContain,PriceNoContain,BookingNodes,Opdesc,FootNotes,Shopping,YingJiPhone")
        //            .Append(" From TpLine Where LineId IN (@0)", groupLineIdArray);

        //        //查询对应的行程安排
        //        Sql sqlLineRoute = new Sql();
        //        sqlLineRoute.Append("select Id as RouteId,LineId,Title,Days,Hotel,Catering,Contents From TpLineRoute Where LineId in(@0)", groupLineIdArray);

        //        var productList = _dao.Query<TpLineModel>(sqlProduct.SQL, sqlProduct.Arguments).ToList();
        //        //获得当前有效的团计划列表
        //        var lineRouteList = _dao.Query<TpLineRouteModel>(sqlLineRoute.SQL, sqlLineRoute.Arguments).ToList();
        //        //获取当前有效线路对应的行程列表

        //        for (int i = 0; i < productList.Count; i++)
        //        {
        //            //遍历产品获得对应的
        //            productList[i].Tours = tourPlanList.Where(item => item.LineId == productList[i].LineId).ToList();
        //            productList[i].Routes = lineRouteList.Where(item => item.LineId == productList[i].LineId).ToList();
        //            productList[i].DepartDest = outCitiesDb.Where(item => item.Key == productList[i].DepartDest).Select(o => o.Value).FirstOrDefault();
        //            productList[i].ArriveDest = DictionaryBiz.GetDestName(productList[i].ArriveDestId);
        //        }
        //        CacheContext.Current.Add(Consts.TourProduct, productList, Consts.OutputCacheDuration3);
        //    }

        //    productRes.Products = CacheContext.Current.Get(Consts.TourProduct) as List<TpLineModel>;
        //    return productRes;
        //}

        /// <summary>
        /// 获取团期状态
        /// </summary>
        /// <param name="tourRequest"></param>
        /// <returns></returns>
        public GetTourResponse GetTour(GetTourRequest tourRequest)
        {
            GetTourResponse tourRes = new GetTourResponse();
            Sql sb = new Sql();
            sb.Append(@"Select TpTourPlan.Id As TourId, TourNo As TourName,TpTourPlan.Price,TpTourPlan.TourState,TpTourPlan.Source,TpTourPlan.OwnerCode,
Tpline.Id As LineId,Tpline.IsValid,TpLine.LineState,
TpQuota.PlanQuota,TpQuota.HoldQuota,TpQuota.UseQuota,TpQuota.UsedQuota
From TpTourPlan
Inner Join TpLine On TpTourPlan.LineId=TpLine.LineId
Inner Join TpTourQuotaMap On TpTourQuotaMap.TourId=TpTourPlan.Id")
                .Append(" Inner Join TpQuota On TpQuota.Id=TpTourQuotaMap.QuotaId where TpTourPlan.Id=@0", tourRequest.TourId)
                .Append(" And TpTourPlan.OwnerCode=@0", tourRequest.OwnerCode); //缺少一个ownercode处理

            var model = new TpTourPlanDao().Query<TpTourStateModel>(sb.SQL, sb.Arguments).FirstOrDefault();
            if (model != null)
            {
                model.TourState = (model.LineState == 3 && model.TourState == 3) == true ? 1 : 0;
                //线路状态为3 上线，团计划为3代表有效 同时等于3即为有效
                tourRes.Tour = model;
            }
            else
            {
                tourRes.SetFailedResultCode("团计划不存在");
            }
            return tourRes;
        }

        /// <summary>
        /// 获取团计划价格
        /// </summary>
        /// <param name="tpLine"></param>
        /// <returns></returns>
        public TpTourModel GetPriceInfo(TpTourModel tpLine)
        {
            Sql sql = new Sql();

            sql.Append(@"SELECT tp.Id, tp.TourId, tp.PriceType, tp.PriceTypeName, tp.PriceRemark, tp.Price, tp.SettlePrice, tp.Cost, 
tp.IsStandard, tp.IsValid, tp.SuitNum, tp.ModifiedBy, tp.ModifiedTime,
ttp.Tips, ttp.ZiFei, ttp.SingleRoom, ttp.TeJiaFanLi
FROM TpPrice tp inner join TpTourPlan ttp on tp.TourId=ttp.TourId
WHERE tp.IsValid=1 AND tp.TourId=@0", tpLine.TourId);
            var priceList = _dao.Query<TpPrice>(sql.SQL, sql.Arguments);
            tpLine.PriceInfo = new PriceInfo();
            //成人价 1 儿童价 2 亲子价 3二大一小、4一大一小 老人价 5

            var priceStand = priceList.Where(item => item.IsStandard == 1).FirstOrDefault();
            if (priceStand != null)
            {
                tpLine.PriceInfo.StandPrice = priceList.Where(item => item.IsStandard == 1).FirstOrDefault().Price;
                tpLine.PriceInfo.StandClearinPrice = priceStand.Price - priceStand.SettlePrice;
            }

            var priceChild = priceList.Where(item => item.PriceType == 2).FirstOrDefault();
            if (priceChild != null)
            {
                tpLine.PriceInfo.KidsPrice = priceChild.Price;
                tpLine.PriceInfo.KidsClearinPrice = priceChild.Price - priceChild.SettlePrice;
            }

            var priceAged = priceList.Where(item => item.PriceType == 5).FirstOrDefault();
            if (priceAged != null)
            {
                tpLine.PriceInfo.AgedPrice = priceAged.Price;
                tpLine.PriceInfo.AgedClearingPrice = priceAged.Price - priceAged.SettlePrice;
            }

            return tpLine;
        }
    }
}
