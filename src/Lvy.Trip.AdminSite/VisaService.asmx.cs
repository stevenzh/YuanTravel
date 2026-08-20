using Lvy.Trip.Biz.Site;
using Lvy.Visa.Biz;
using Lvy.Visa.Models.API;
using Lvy.Web.Common;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;

namespace Lvy.Trip.AdminSite
{
    /// <summary>
    /// VisaService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://www.sh-cct.cn/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。
    // [System.Web.Script.Services.ScriptService]
    public class VisaService : System.Web.Services.WebService
    {
        /// <summary>
        /// 获取团签产品列表（度假）
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public GetVisaAreaResponse QueryTourVisaProductList()
        {
            SearchProductBiz productService = new SearchProductBiz();
            List<VisaAreaData> areaList = new List<VisaAreaData>();
            GetVisaAreaResponse result = new GetVisaAreaResponse();

            var query = productService.QueryVisaProductList("", 0).Where(a => a.VType == 2).ToList();
            foreach (var model in query.OrderBy(a => a.Continent).GroupBy(a => a.Continent))
            {
                VisaAreaData visaAreaData = new VisaAreaData();
                visaAreaData.VisaAreaCode = model.Key;
                visaAreaData.VisaAreaName = DictionaryTools.GetEnumValue(Enums.ContinentEnum, model.Key.ToString());
                var tempList = from a in query.Where(a => a.Continent == model.Key)
                               orderby a.VisaCountry
                               select new VisaInformationData
                               {
                                   InformationId = a.InformationId,
                                   InformationCode = a.InformationCode,
                                   VType = a.VType,
                                   InformationName = a.InformationName,
                                   VisaType = a.VisaType,
                                   VisaTypeName = "团队签证",
                                   InterviewType = a.InterviewType,
                                   Continent = a.Continent,
                                   ContinentName = visaAreaData.VisaAreaName,
                                   VisaCountry = a.VisaCountry,
                                   VisaArea = a.VisaArea,
                                   SellPrice = a.SellPrice,
                                   AcceptedTime = a.AcceptedTime,
                                   AcceptedRange = a.AcceptedRange,
                                   VisaExpiryDate = a.VisaExpiryDate,
                                   StayDays = a.StayDays,
                                   EnterCount = a.EnterCount,
                                   IsDanBao = a.IsDanBao,
                                   WarmTips = a.WarmTips,
                                   AdvanceDays = a.AdvanceDays,
                                   VisaIssuePlace = a.VisaIssuePlace,
                                   VisaIssuePlaceName = a.VisaIssuePlaceName,
                                   IsHurry = a.IsHurry,
                                   IsHurryName = (a.IsHurry != null && a.IsHurry == 1) ? "可以" : "不可以"
                               };
                visaAreaData.VisaProductList = tempList.ToList();

                areaList.Add(visaAreaData);
            }
            result.AreaList = areaList;

            return result;
        }

        /// <summary>
        /// 获取个签产品列表（度假）
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public GetVisaResponse QuerySingleVisaProductList()
        {
            GetVisaResponse result = new GetVisaResponse();
            SearchProductBiz productService = new SearchProductBiz();
            var query = from a in productService.QueryVisaProductList("", 0)
                        where a.VType == 1
                        orderby a.VisaCountry
                        select new VisaInformationData
                        {
                            InformationId = a.InformationId,
                            InformationCode = a.InformationCode,
                            VType = a.VType,
                            InformationName = a.InformationName,
                            VisaType = a.VisaType,
                            VisaTypeName = DictionaryTools.GetEnumValue(Enums.VisaTypeEnum, a.VisaType.ToString()),
                            InterviewType = a.InterviewType,
                            Continent = a.Continent,
                            ContinentName = DictionaryTools.GetEnumValue(Enums.ContinentEnum, a.Continent.ToString()),
                            VisaCountry = a.VisaCountry,
                            VisaArea = a.VisaArea,
                            SellPrice = a.SellPrice,
                            ImgUrl = a.ImgUrl,
                            AcceptedTime = a.AcceptedTime,
                            AcceptedRange = a.AcceptedRange,
                            VisaExpiryDate = a.VisaExpiryDate,
                            StayDays = a.StayDays,
                            EnterCount = a.EnterCount,
                            IsDanBao = a.IsDanBao,
                            BookingNodes = a.BookingNodes,
                            WarmTips = a.WarmTips,
                            PayTimeLimit = a.PayTimeLimit,
                            AdvanceDays = a.AdvanceDays,
                            LivePassportArea = a.LivePassportArea,
                            VisaIssuePlace = a.VisaIssuePlace,
                            VisaIssuePlaceName = a.VisaIssuePlaceName,
                            SupplierCode = a.SupplierCode,
                            IsHurry = a.IsHurry,
                            IsHurryName = (a.IsHurry != null && a.IsHurry == 1) ? "可以" : "不可以"
                        };
            result.ProductList = query.ToList();
            return result;
        }

        /// <summary>
        /// 查询所有签证产品列表
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public GetVisaResponse QueryVisaProducts()
        {
            GetVisaResponse result = new GetVisaResponse();
            SearchProductBiz productService = new SearchProductBiz();
            var query = from a in productService.QueryVisaProductList("", 0)
                        orderby a.VisaCountry
                        select new VisaInformationData
                        {
                            InformationId = a.InformationId,
                            InformationCode = a.InformationCode,
                            VType = a.VType,
                            InformationName = a.InformationName,
                            VisaType = a.VisaType,
                            VisaTypeName = DictionaryTools.GetEnumValue(Enums.VisaTypeEnum, a.VisaType.ToString()),
                            InterviewType = a.InterviewType,
                            Continent = a.Continent,
                            ContinentName = DictionaryTools.GetEnumValue(Enums.ContinentEnum, a.Continent.ToString()),
                            VisaCountry = a.VisaCountry,
                            VisaArea = a.VisaArea,
                            SellPrice = a.SellPrice,
                            ImgUrl = a.ImgUrl,
                            AcceptedTime = a.AcceptedTime,
                            AcceptedRange = a.AcceptedRange,
                            VisaExpiryDate = a.VisaExpiryDate,
                            StayDays = a.StayDays,
                            EnterCount = a.EnterCount,
                            IsDanBao = a.IsDanBao,
                            BookingNodes = a.BookingNodes,
                            WarmTips = a.WarmTips,
                            PayTimeLimit = a.PayTimeLimit,
                            AdvanceDays = a.AdvanceDays,
                            LivePassportArea = a.LivePassportArea,
                            VisaIssuePlace = a.VisaIssuePlace,
                            VisaIssuePlaceName = a.VisaIssuePlaceName,
                            SupplierCode = a.SupplierCode,
                            IsHurry = a.IsHurry,
                            IsHurryName = (a.IsHurry != null && a.IsHurry == 1) ? "可以" : "不可以"
                        };
            result.ProductList = query.ToList();
            return result;
        }

        /// <summary>
        /// 所有个签签证产品（自由行）
        /// </summary>
        /// <param name="countryName"></param>
        /// <param name="visaType"></param>
        /// <param name="productName"></param>
        /// <returns></returns>
        [WebMethod]
        public GetVisaResponse SearchVisaProductList(string countryName, int visaType, string productName)
        {
            GetVisaResponse result = new GetVisaResponse();
            SearchProductBiz productService = new SearchProductBiz();
            var query = from a in productService.QueryVisaProductList(countryName, visaType, productName)
                        select new VisaInformationData
                        {
                            InformationId = a.InformationId,
                            InformationCode = a.InformationCode,
                            VType = a.VType,
                            InformationName = a.InformationName,
                            VisaType = a.VisaType,
                            InterviewType = a.InterviewType,
                            Continent = a.Continent,
                            VisaCountry = a.VisaCountry,
                            VisaArea = a.VisaArea,
                            SellPrice = a.SellPrice,
                            ImgUrl = a.ImgUrl,
                            AcceptedTime = a.AcceptedTime,
                            AcceptedRange = a.AcceptedRange,
                            VisaExpiryDate = a.VisaExpiryDate,
                            StayDays = a.StayDays,
                            EnterCount = a.EnterCount,
                            IsDanBao = a.IsDanBao,
                            BookingNodes = a.BookingNodes,
                            WarmTips = a.WarmTips,
                            PayTimeLimit = a.PayTimeLimit,
                            AdvanceDays = a.AdvanceDays,
                            LivePassportArea = a.LivePassportArea,
                            VisaIssuePlace = a.VisaIssuePlace,
                            VisaIssuePlaceName = a.VisaIssuePlaceName,
                            SupplierCode = a.SupplierCode,
                            IsHurry = a.IsHurry,
                            IsHurryName = (a.IsHurry != null && a.IsHurry == 1) ? "可以" : "不可以"
                        };
            result.ProductList = query.ToList();
            return result;
        }

        /// <summary>
        /// 根据编码获取签证产品
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [WebMethod]
        public VisaInformationData GetVisaProductInfo(string code)
        {
            SearchProductBiz productService = new SearchProductBiz();
            var model = productService.GetVisaProductInfo(code);
            if (null != model)
            {
                var pModel = new VisaInformationData
                {
                    InformationId = model.InformationId,
                    InformationCode = model.InformationCode,
                    VType = model.VType,
                    InformationName = model.InformationName,
                    VisaType = model.VisaType,
                    InterviewType = model.InterviewType,
                    Continent = model.Continent,
                    VisaCountry = model.VisaCountry,
                    VisaArea = model.VisaArea,
                    SellPrice = model.SellPrice,
                    ImgUrl = model.ImgUrl,
                    AcceptedTime = model.AcceptedTime,
                    AcceptedRange = model.AcceptedRange,
                    VisaExpiryDate = model.VisaExpiryDate,
                    StayDays = model.StayDays,
                    EnterCount = model.EnterCount,
                    IsDanBao = model.IsDanBao,
                    BookingNodes = model.BookingNodes,
                    WarmTips = model.WarmTips,
                    PayTimeLimit = model.PayTimeLimit,
                    AdvanceDays = model.AdvanceDays,
                    LivePassportArea = model.LivePassportArea,
                    VisaIssuePlace = model.VisaIssuePlace,
                    VisaIssuePlaceName = model.VisaIssuePlaceName,
                    SupplierCode = model.SupplierCode,
                    IsHurry = model.IsHurry,
                    IsHurryName = (model.IsHurry != null && model.IsHurry == 1) ? "可以" : "不可以"
                };
                return pModel;
            }
            return null;
        }

        /// <summary>
        /// 获取产品的分类数据列表
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [WebMethod]
        public IList<VisaCategoryData> QueryVisaCatagoryDataList(string code)
        {
            List<VisaCategoryData> categoryList = new List<VisaCategoryData>();
            ProductBiz productService = new ProductBiz();
            var categoryModels = productService.GetCategroyList(code);
            if (null != categoryModels)
            {
                var fileDataModels = productService.GetVisaMaterialFileList(code);
                var dataModels = productService.GetVisaDataList(code);

                foreach (var categoryModel in categoryModels)
                {
                    var categoryObj = new VisaCategoryData
                    {
                        CategoryCode = categoryModel.CategoryCode,
                        CategoryId = categoryModel.CategoryId,
                        CategoryName = categoryModel.CategoryName,
                        InformationCode = categoryModel.InformationCode
                    };
                    categoryObj.MaterialDataList = (from a in dataModels
                                                    where a.CategoryCode == categoryModel.CategoryCode
                                                    select new VisaMaterialData
                                                    {
                                                        CategoryCode = a.CategoryCode,
                                                        DataCode = a.DataCode,
                                                        DataCount = a.DataCount,
                                                        DataExplain = a.DataExplain,
                                                        DataId = a.DataId,
                                                        DataName = a.DataName,
                                                        InformationCode = a.InformationCode,
                                                        IsBack = a.IsBack,
                                                        IsNeed = a.IsNeed,
                                                        IsOriginal = a.IsOriginal,
                                                        IsTemplate = a.IsTemplate
                                                    }).ToList();

                    foreach (var dataModel in categoryObj.MaterialDataList)
                    {
                        dataModel.MeterialFilesList = (from a in fileDataModels
                                                       where a.DataCode == dataModel.DataCode
                                                       select new VisaMaterialFilesData
                                                       {
                                                           DataCode = a.DataCode,
                                                           InformationCode = a.InformationCode,
                                                           FileName = a.FileName,
                                                           FileUrl = a.FileUrl
                                                       }).ToList();
                    }
                    categoryList.Add(categoryObj);
                }
            }
            return categoryList;
        }

        /// <summary>
        /// 查询签证产品列表
        /// </summary>
        /// <param name="pCodeArrayStr">产品编号，多个用，隔开</param>
        /// <returns></returns>
        [WebMethod]
        public List<VisaInformationData> SearchVisaProductListByCode(string pCodeArrayStr)
        {
            SearchProductBiz productService = new SearchProductBiz();
            var query = from a in productService.QueryVisaProductList(pCodeArrayStr)
                        select new VisaInformationData
                        {
                            InformationId = a.InformationId,
                            InformationCode = a.InformationCode,
                            VType = a.VType,
                            InformationName = a.InformationName,
                            VisaType = a.VisaType,
                            InterviewType = a.InterviewType,
                            Continent = a.Continent,
                            VisaCountry = a.VisaCountry,
                            VisaArea = a.VisaArea,
                            SellPrice = a.SellPrice,
                            ImgUrl = a.ImgUrl,
                            AcceptedTime = a.AcceptedTime,
                            AcceptedRange = a.AcceptedRange,
                            VisaExpiryDate = a.VisaExpiryDate,
                            StayDays = a.StayDays,
                            EnterCount = a.EnterCount,
                            IsDanBao = a.IsDanBao,
                            BookingNodes = a.BookingNodes,
                            WarmTips = a.WarmTips,
                            PayTimeLimit = a.PayTimeLimit,
                            AdvanceDays = a.AdvanceDays,
                            LivePassportArea = a.LivePassportArea,
                            VisaIssuePlace = a.VisaIssuePlace,
                            VisaIssuePlaceName = a.VisaIssuePlaceName,
                            SupplierCode = a.SupplierCode,
                            IsHurry = a.IsHurry,
                            IsHurryName = (a.IsHurry != null && a.IsHurry == 1) ? "可以" : "不可以"
                        };
            return query.ToList();
        }

        /// <summary>
        /// 获取签证国家列表数据
        /// </summary>
        /// <param name="vtype"></param>
        /// <returns></returns>
        [WebMethod]
        public List<VisaInformationData> SearchVisaListByVtype(int vtype)
        {
            SearchProductBiz productService = new SearchProductBiz();
            var query = from a in productService.QueryVisaCountryList(vtype, true)
                        where a.VType == vtype
                        orderby a.VisaCountry
                        select new VisaInformationData
                        {
                            VType = a.VType,
                            Continent = a.Continent,
                            VisaCountry = a.VisaCountry,
                            VisaCountryParentStr = a.VisaCountryParentStr,
                            // CountryProValue = productService.GetProductListByCountry(a.VisaCountryParentStr, vtype)
                        };

            return query.ToList();
        }
    }
}