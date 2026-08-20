using Arch.Common;
using Common.Logging;
using Lvy.Models;
using Lvy.Models.ProductDB;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Biz.Site;
using Lvy.Trip.WebSite.Mvc.Attributes;
using Lvy.Visa.Biz;
using Lvy.Visa.Models;
using Lvy.VModels.Product;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    /// <summary>
    /// 线路产品管理
    /// 
    /// 后台线路管理复制于此
    /// </summary>
    [LvyAuth]
    public partial class LineAdminController : BaseController
    {
        private ILog logger = LogManager.GetLogger("LineAdminController");

        private readonly TpLineBiz _tpLineBiz = new TpLineBiz();
        private readonly TpLineRouteBiz _tpRouteBiz = new TpLineRouteBiz();
        private readonly TpLineBusPointBiz _tpLineBusPointBiz = new TpLineBusPointBiz();
        private readonly TpLineVisaBiz _lineVisaBiz = new TpLineVisaBiz();
        private readonly TpLineAdminBiz _adminBiz = new TpLineAdminBiz();
        private readonly BaseTagBiz baseTagBiz = new BaseTagBiz();
        private readonly AccountBiz accountBiz = new AccountBiz();
        private readonly ProductBiz _visaBiz = new ProductBiz();
        private readonly SearchProductBiz homeService = new SearchProductBiz();

        #region 线路列表

        /// <summary>
        /// 查询线路
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult SearchLine(TpLineVModel vModel)
        {
            vModel.OwnerCode = OwnerCode;
            vModel.CustomerCode = UserInfo.CustomerCode;

            if (vModel.LineList == null)
                vModel.LineList = new PagedList<TpLineModel>();

            vModel.IsImport = "1";  // 只查外部录入线路

            var result = _tpLineBiz.GetLineList(vModel);

            InitLineTypeSelectItems();

            if (Request.IsAjaxRequest())
            {
                return PartialView("Line/UCLineList", result);
            }
            return View("Line/SearchLine", result);
        }

        #endregion 线路列表

        #region 新增线路

        /// <summary>
        /// 复制线路
        ///     DESC: 与新增线路共用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CopyLine(string id)
        {
            InitEditLineData();
            var vModel = _tpLineBiz.GetAddLineVModel(id);

            // 其他列表
            vModel.OperationType = LineOperationType.CopyLine;
            vModel.OutCities = DictionaryTools.GetEnumsBy(Enums.OutCityEnum);
            vModel.AccountListBean = GetAccountList(GlobalContext.Current.UserInfo.CustomerCode);
            return View("Line/CopyLine", vModel);
        }

        /// <summary>
        /// 验证线路名称重复性
        /// </summary>
        /// <param name="lineName"></param>
        /// <returns></returns>
        public string CheckLineName(string lineName, string lineId)
        {
            var entity = _tpLineBiz.CheckLineName(lineName, lineId);
            if (entity != null)
                return "exsit";
            else
                return "ok";
        }

        /// <summary>
        /// 验证线路是否包含订单
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public ContentResult CheckOrdered(int lineId)
        {
            var orderBiz = new Biz.Order.OrderBiz();
            var orders = orderBiz.GetValidOrderByLineId(lineId);
            if (orders != null && orders.Count > 0)
            {
                return Content("1");
            }
            return Content("0");
        }

        /// <summary>
        /// 保存新增线路
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public string CopyLine(AddLineVModel model)
        {
            string newId = (model.OperationType == LineOperationType.AddLine) ? _tpLineBiz.AddLine(model, UserInfo) : _tpLineBiz.CopyLine(model, UserInfo);
            CheckLineFolder(newId);
            return newId.ToString(CultureInfo.InvariantCulture);
        }

        #endregion 新增线路

        public ActionResult AddLineInfo(string lineId)
        {
            if (string.IsNullOrEmpty(lineId))  // 新增产品
            {
                var listOutCity = DictionaryTools.GetEnumsBy(Enums.OutCityEnum);
                var vModel = new AddLineVModel
                {
                    OperationType = LineOperationType.AddLine,
                    OutCities = listOutCity,
                    TpLine = new TpLineModel
                    {
                        LineType = 1,//默认线路类型 跟团
                        LineScope = 4, //  出境
                        TrafficType = 3,//默认交通类型 飞机
                        DepartDest = listOutCity[0].Key,
                        TeamID = GlobalContext.Current.CustomerBy.ImportTeam   // 产品归属部门
                    },
                };

                return View("Line/AddLineInfo", vModel);
            }
            else           // 编辑产品
            {
                var vModel = _tpLineBiz.GetAddLineVModel(lineId);
                return View("Line/AddLineInfo", vModel);
            }
        }

        /// <summary>
        /// 编辑线路基本信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult UCLineInfo(string id)
        {
            InitEditLineData();
            var vModel = new AddLineVModel();

            if (string.IsNullOrEmpty(id))   // 新增产品
            {
                var listOutCity = DictionaryTools.GetEnumsBy(Enums.OutCityEnum);
                vModel = new AddLineVModel
                {
                    OperationType = LineOperationType.AddLine,
                    OutCities = InitOutCity(),
                    TpLine = new TpLineModel
                    {
                        LineType = 1,//默认线路类型 跟团
                        LineScope = 4, // 默认出境
                        TrafficType = 1,//默认交通类型
                        DepartDest = listOutCity[0].Key
                    },
                    //LineAdmin = new TpLineAdminModel { AccountCode = GlobalContext.Current.UserInfo.Code },
                    AccountListBean = GetAccountList(GlobalContext.Current.UserInfo.CustomerCode)
                };
            }
            else           // 编辑产品
            {
                vModel = _tpLineBiz.GetAddLineVModel(id);
                vModel.OperationType = LineOperationType.EditLine;
                vModel.OutCities = DictionaryTools.GetEnumsBy(Enums.OutCityEnum);
                vModel.AccountListBean = GetAccountList(GlobalContext.Current.UserInfo.CustomerCode);
            }
            return PartialView("Line/UCLineInfo", vModel);
        }

        public ActionResult SaveLineInfo(AddLineVModel vModel)
        {
            string LineId = vModel.TpLine.LineId;
            if (!string.IsNullOrEmpty(LineId))  // 编辑
            {
                _tpLineBiz.EditLine(vModel, UserInfo);
                CheckLineFolder(vModel.TpLine.LineId);
            }
            else
            {
                //新增
                LineId = _tpLineBiz.AddLine(vModel, UserInfo);
                CheckLineFolder(LineId);
            }
            return RedirectToAction("UCLineInfo", new { id = LineId });// PartialView("Line/UCLineInfo", vModel);//
        }

        /// <summary>
        /// 变更线路状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [LvyAuth]
        public string ChangeOnline(string id)
        {
            return Convert.ToString(_tpLineBiz.UpdateLineState(id, 3));
        }

        [LvyAuth]
        public string ChangeOffline(string id)
        {
            return Convert.ToString(_tpLineBiz.UpdateLineState(id, 2));
        }

        /// <summary>
        /// 设置线路有效性
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetLineValidState(string id)
        {
            var model = _tpLineBiz.GetLineById(id);
            model.IsValid = (model.IsValid == 0) ? 1 : 0;
            _tpLineBiz.UpdateLine(model);
            return RedirectToAction("SearchLine");
        }

        /// <summary>
        /// 设置外部录入产品 状态
        /// </summary>
        /// <param name="LineId"></param>
        /// <param name="ImportState"></param>
        /// <returns></returns>
        public ActionResult SetImportState(string LineId, int ImportState)
        {
            int row = _tpLineBiz.UpdateImportState(new TpLineModel { LineId = LineId, ImportState = ImportState });
            if (row > 0)
                return Json(new { Code = "1", Message = "设置成功" });
            else
                return Json(new { Code = "0", Message = "没有改变状态" });
        }

        /// <summary>
        /// 删除线路
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteLine(string id)
        {
            var model = _tpLineBiz.GetLineById(id);
            if (model.IsValid != 0)
            {
                model.IsValid = 0;
                _tpLineBiz.UpdateLine(model);
            }
            return RedirectToAction("SearchLine");
        }

        #region 线路私有方法

        /// <summary>
        /// 初始化【线路类型】下拉列表
        /// </summary>
        private void InitLineTypeSelectItems()
        {
            ViewBag.LineTypeItems = DictionaryTools.GetEnumsBy(Enums.LineTypeEnum).ToSelectListFor(a => a.Key, a => a.Value);
            ViewBag.LineScopeItems = DictionaryTools.GetEnumsBy(Enums.LineScopeEnum).ToSelectListFor(a => a.Key, a => a.Value);
        }

        /// <summary>
        /// 初始化线路编辑所需信息
        /// </summary>
        private void InitEditLineData()
        {
            ViewBag.LineTypeRadioItems = DictionaryTools.GetEnumsBy(Enums.LineTypeEnum);
            ViewBag.LineScopeRadioItems = DictionaryTools.GetEnumsBy(Enums.LineScopeEnum);
            ViewBag.TrafficTypeRadioItems = DictionaryTools.GetEnumsBy(Enums.TrafficTypeEnum);
            ViewBag.ThemesList = baseTagBiz.GetTags(GlobalContext.Current.OwnerCode);
        }

        /// <summary>
        /// 获取当前同一customercode下所有账号
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        private List<KeyValueBean> GetAccountList(string customerCode)
        {
            return accountBiz.GetAllAccountBeans(customerCode, OwnerCode);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        private string UploadLinePic(string fileName, int lineId)
        {
            HttpPostedFileBase file = Request.Files[fileName];
            if (file == null || file.ContentLength <= 0)
                return string.Empty;

            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            var request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            request.VirtualPath = @"line\" + lineId;
            UploadServiceClient client = new UploadServiceClient();

            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }

        /// <summary>
        /// 文件服务器创建路径
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        private string CheckLineFolder(string lineId)
        {
            var request = new UploadFileRequest();
            request.VirtualPath = @"line\" + lineId;
            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.CreateFolder(request);
            return response.FilePath + response.FileName;
        }

        #endregion 线路私有方法

        #region 上车点

        /// <summary>
        /// 选择上车点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SelectBusPoint(string id)
        {
            SelectBusPointVModel vModel = new SelectBusPointVModel();
            var baseBusPointBiz = new BusPointBiz();
            //若LineI大于0，则是通过连接而来，否则为查询的情况
            if (!string.IsNullOrEmpty(id))
            {
                vModel.Line = _tpLineBiz.GetLineById(id);
                var baseBusPoints = baseBusPointBiz.GetBusPointByGroup(vModel.Line.DepartDest, vModel.GroupId, OwnerCode);
                vModel.BusPointList = _tpLineBusPointBiz.GetSelectBusPointsList(baseBusPoints, vModel);
            }

            vModel.GroupItems = baseBusPointBiz.GetGroupList(vModel.Line.DepartDest, GlobalContext.Current.OwnerCode);
            return View("BusPoint/SelectBusPoint", vModel);
        }

        public ActionResult UCBusPointList(string id)
        {
            SelectBusPointVModel vModel = new SelectBusPointVModel();
            var baseBusPointBiz = new BusPointBiz();
            //若LineI大于0，则是通过连接而来，否则为查询的情况
            if (!string.IsNullOrEmpty(id))
            {
                vModel.Line = _tpLineBiz.GetLineById(id);
                var baseBusPoints = baseBusPointBiz.GetBusPointByGroup(vModel.Line.DepartDest, vModel.GroupId, OwnerCode);
                vModel.BusPointList = _tpLineBusPointBiz.GetSelectBusPointsList(baseBusPoints, vModel);
            }

            vModel.GroupItems = baseBusPointBiz.GetGroupList(vModel.Line.DepartDest, GlobalContext.Current.OwnerCode);

            return PartialView("BusPoint/UCBusPointList", vModel);
        }

        /// <summary>
        /// 保存上车点
        /// </summary>
        /// <param name="vModel"> </param>
        /// <returns></returns>
        public ActionResult SaveBusPoint(SelectBusPointVModel vModel, List<BusPointItemVModel> busPointList)
        {
            string lineId = vModel.Line.LineId;
            _tpLineBusPointBiz.SaveBusPoint(vModel.BusPointList, UserInfo.Code, OwnerCode);
            return RedirectToAction("SelectBusPoint", new { lineId = lineId });
        }

        #endregion 上车点

        #region 上传行程信息

        public ActionResult UCAddFile(string id)
        {
            AddLineVModel vModel = new AddLineVModel();
            if (!string.IsNullOrEmpty(id))
            {
                vModel.LineFileVModel.LineFileList = _tpLineBiz.GetLineFileList(id);
                vModel.TpLine = _tpLineBiz.GetLineById(id);
            }
            ViewBag.FileBusiList = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 2 && t.Key.StartsWith("1")).ToList();

            return PartialView("File/UCAddFile", vModel);
        }

        public ActionResult AddUpLoadFile(TpLineFileVModel vModel)
        {
            //添加新的附件信息
            string filename = "";
            string filenameExt = "";
            string FilePath = UploadLineFile(vModel.LineId, ref filename, ref filenameExt);
            TpLineFileModel model2 = new TpLineFileModel();
            model2.LineId = vModel.LineId;
            model2.FileName = filename;
            model2.FilePath = FilePath;
            model2.CreatedTime = DateTime.Now;
            model2.Note = vModel.fileNote;
            model2.SourceType = vModel.SourceType;
            model2.IsDel = 0;
            model2.MediaType = WebToolKit.GetFileMedia(filenameExt);
            _tpLineBiz.AddLineFile(model2);

            AddLineVModel vModel1 = new AddLineVModel();
            vModel1.LineFileVModel.LineFileList = _tpLineBiz.GetLineFileList(vModel.LineId);
            vModel1.TpLine = _tpLineBiz.GetLineById(vModel.LineId);
            ViewBag.FileBusiList = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 2 && t.Key.StartsWith("1")).ToList();

            return PartialView("Line/UCFileList", vModel1);
        }

        private string UploadLineFile(string lineId, ref string file_name, ref string file_ext)
        {
            HttpPostedFileBase file = Request.Files["lineFileName"];
            if (file == null || file.ContentLength <= 0)
                return string.Empty;

            file_name = file.FileName;
            file_ext = Path.GetExtension(file.FileName);
            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            // 所属客户code\文件类型
            request.VirtualPath = string.Format(@"line\{0}", lineId);

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }

        /// <summary>
        /// 删除附件文件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public ActionResult DeleteLineFile(int id, string lineId)
        {
            TpLineFileModel model = _tpLineBiz.GetLineFileModel(id);
            int i = _tpLineBiz.DeleteLineFile(id);

            // 重复 A
            AddLineVModel vModel = new AddLineVModel();
            vModel.LineFileVModel.LineFileList = _tpLineBiz.GetLineFileList(lineId);
            vModel.TpLine = _tpLineBiz.GetLineById(lineId);
            ViewBag.FileBusiList = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 2 && t.Key.StartsWith("1")).ToList();

            return PartialView("File/UCFileList", vModel);
        }

        /// <summary>
        /// 设置产品首图
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public ActionResult SetPrimaryImage(int id, string lineId)
        {
            TpLineFileModel model = _tpLineBiz.GetLineFileModel(id);
            int i = _tpLineBiz.SetPrimaryPic(lineId, model.FilePath);

            // 重复 A
            AddLineVModel vModel = new AddLineVModel();
            vModel.LineFileVModel.LineFileList = _tpLineBiz.GetLineFileList(lineId);
            vModel.TpLine = _tpLineBiz.GetLineById(lineId);
            ViewBag.FileBusiList = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 2 && t.Key.StartsWith("1")).ToList();

            return PartialView("File/UCFileList", vModel);
        }

        /// <summary>
        /// 下载购物行程单文件.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DownLoadFile(int id)
        {
            TpLineFileModel model = _tpLineBiz.GetLineFileModel(id);
            if (model == null)
                return null;
            try
            {
                WebRequest.Create(AppSetting.Get("UploadFileRoot") + model.FilePath);
            }
            catch (Exception ex)
            {
                logger.Error("File not Found.", ex);
                return null;
            }

            byte[] fileData;
            try
            {
                using (WebClient client = new WebClient())
                {
                    fileData = client.DownloadData(AppSetting.Get("UploadFileRoot") + model.FilePath);

                    return File(fileData, "application/octet-stream", Server.UrlEncode(model.FileName));
                }
            }
            catch (Exception ex)
            {
                logger.Error("File download failure..", ex);
                return null;
            }
        }

        /// <summary>
        /// 选择图片初始化
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SearchImages(string ParentStr, int ImagePagedIndex = 1)
        {
            try
            {
                AddLineVModel qmodel = new AddLineVModel();
                if (!ParentStr.IsNullOrEmpty())
                {
                    qmodel.PhotoInfoList = _tpLineBiz.SearchCityImages(ParentStr, ImagePagedIndex);
                }
                else
                {
                    qmodel.PhotoInfoList = new PagedList<Lvy.Models.BaseDB.PhotoInfoModel>();
                }
                if (Request.IsAjaxRequest())
                    return PartialView("ImageList", qmodel);

                return View("SelectImages", qmodel);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                throw ex;
            }
        }

        public ActionResult AddImage(TpLineFileVModel vModel)
        {
            //添加新的附件信息
            TpLineFileModel model = new TpLineFileModel();
            model.LineId = vModel.LineId;
            model.FileName = vModel.fileName;
            model.FilePath = vModel.FilePath;
            model.CreatedTime = DateTime.Now;
            model.Note = vModel.fileNote;
            model.SourceType = vModel.SourceType;
            model.IsDel = 0;
            model.MediaType = MediaType.image.ToString();
            model.PhotoId = vModel.PhotoId;

            _tpLineBiz.AddLineFile(model);

            AddLineVModel vModel1 = new AddLineVModel();
            vModel1.LineFileVModel.LineFileList = _tpLineBiz.GetLineFileList(vModel.LineId);
            vModel1.TpLine = _tpLineBiz.GetLineById(vModel.LineId);
            ViewBag.FileBusiList = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 2 && t.Key.StartsWith("1")).ToList();

            return PartialView("File/UCFileList", vModel1);
        }

        public ActionResult EditLineFile(int id)
        {
            var model = _tpLineBiz.GetLineFileModel(id);
            ViewData["FileBusiList"] = DictionaryTools.GetEnumsBy(Enums.FileBusinessEnum).Where(t => t.Key.Length == 2 && t.Key.StartsWith("1")).ToList();

            return View("File/EditLineFile", model);
        }

        public ActionResult SaveLineFile(TpLineFileModel visaModel)
        {
            try
            {
                int i = _tpLineBiz.UpdateLineFile(visaModel);
                if (i > 0)
                {
                    return Json(new { Code = 1, Message = "保存成功！" });
                }

                return Json(new { Code = 0, Message = "保存失败！" });
            }
            catch (Exception ex)
            {
                return Json(new { Code = 0, Message = "保存失败！" + ex.Message });
            }
        }

        #endregion 上传行程信息

        #region 预定须知

        /// <summary>
        /// 获取预定须知
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult UCLineYuDingXuZhi(string id)
        {
            AddLineVModel vModel = new AddLineVModel();
            if (!string.IsNullOrEmpty(id))
            {
                vModel = _tpLineBiz.GetAddLineVModel(id);
            }
            else
            {
                vModel.TpLine = new TpLineModel();
            }
            return PartialView("Line/UCLineYuDingXuZhi", vModel);
        }

        /// <summary>
        /// 保存预定须知信息
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult SaveYuDingXuZhi(AddLineVModel vModel)
        {
            _tpLineBiz.SaveYuDingXuZhi(vModel);

            // 重新获取
            vModel = _tpLineBiz.GetAddLineVModel(vModel.TpLine.LineId);
            return PartialView("Line/UCLineYuDingXuZhi", vModel);
        }

        #endregion 预定须知

        /// <summary>
        /// 获取出发地
        /// </summary>
        /// <param name="outCityCode"></param>
        /// <returns></returns>
        private List<KeyValueBean> InitOutCity(string outCityCode = null)
        {
            var cityNos = ConfigurationManager.AppSettings["OutCity"].Split('|');  // 支持多商户加的二次过滤
            var outCities = new List<KeyValueBean>();
            var outCitiesDb = DictionaryTools.GetEnumsBy(Enums.OutCityEnum);
            foreach (var cityNo in cityNos)
            {
                outCities.AddRange(outCitiesDb.Where(p => p.Key == cityNo));
            }
            return outCities;
        }

        #region 单页编辑

        /// <summary>
        /// 获取预定须知
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult UCLineDesc(string id)
        {
            AddLineVModel vModel = new AddLineVModel();
            if (!string.IsNullOrEmpty(id))
            {
                vModel = _tpLineBiz.GetAddLineVModel(id);
            }

            return PartialView("Line/UCLineDesc", vModel);
        }

        /// <summary>
        /// 保存预定须知信息
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult SaveLineDesc(AddLineVModel vModel)
        {
            _tpLineBiz.SaveLineDesc(vModel);

            // 重新获取
            vModel = _tpLineBiz.GetAddLineVModel(vModel.TpLine.LineId);
            return PartialView("Line/UCLineDesc", vModel);
        }

        #endregion 单页编辑

        #region 编辑行程

        public ActionResult UCLineRoute(string id)
        {
            // InitEditRouteData();
            //EditRouteVModel vModel = new EditRouteVModel
            //{
            //    LineId = id,
            //    TrafficType = _tpLineBiz.GetLineById(id).TrafficType,
            //    LineRoutes = _tpRouteBiz.GetRouteListByLineId(id)
            //};
            AddLineVModel vModel = new AddLineVModel();
            vModel.TpLine = new TpLineModel();
            vModel.TpLineRouteList = new List<TpLineRouteModel>();
            if (!string.IsNullOrEmpty(id))
            {
                vModel.TpLine = _tpLineBiz.GetLineById(id);

                vModel.TpLineRouteList = _tpRouteBiz.GetRouteListByLineId(id);

                //获取交通信息列表。
                var trafficList = _tpRouteBiz.GetTrafficListByLineId(id);

                foreach (var item in vModel.TpLineRouteList)
                {
                    item.LineTrafficList = new List<TpLineTrafficModel>();
                    var list = trafficList.Where(a => a.LineRouteId == item.Id).ToList();
                    if (list.Count > 0)
                    {
                        item.LineTrafficList.AddRange(list);
                    }
                    else
                    {
                        TpLineTrafficModel model = new TpLineTrafficModel();

                        item.LineTrafficList.Add(model);
                    }
                }
            }
            return PartialView("Line/UCLineRoute", vModel);
        }

        [ValidateInput(false)]
        public ActionResult SaveLineRoute(AddLineVModel vModel)
        {
            var tpLineRouteList = vModel.TpLineRouteList;

            try
            {
                int i = _tpRouteBiz.UpdateRoute(tpLineRouteList, UserInfo);
                if (i > 0)
                {
                    return Json(new { Code = 200, Message = "保存成功！" });
                }
                return Json(new { Code = 000, Message = "保存失败！" });
            }
            catch (Exception ex)
            {
                return Json(new { Code = 000, Message = "保存失败！" + ex.Message });
            }
        }

        #endregion 编辑行程

        #region 签证部分

        public ActionResult UCLineVisa(string id)
        {
            AddLineVModel vModel = new AddLineVModel();
            vModel.TpLine = new TpLineModel();
            vModel.TpLineRouteList = new List<TpLineRouteModel>();
            if (!string.IsNullOrEmpty(id))
            {
                vModel.TpLine = _tpLineBiz.GetLineById(id);
                //加载Visa列表信息。
                vModel.TpLineVisaList = _lineVisaBiz.GetTpLineVisaList(id);
            }

            return PartialView("Visa/UCLineVisa", vModel);
        }

        public ActionResult AddLineVisa(string LineId)
        {
            var model = new TpLineVisaModel();
            model.LineId = LineId;

            return View("Visa/AddLineVisa", model);
        }

        public ActionResult EditLineVisa(int id)
        {
            var model = _lineVisaBiz.GetById(id);
            var product = homeService.GetVisaProductInfo(model.ProductCode);
            model.VType = product.VType;
            ViewData["CountryList"] = homeService.QueryVisaCountryList(model.VType, false).ToSelectListFor(m => m.VisaCountryParentStr, m => m.VisaCountry);
            ViewData["ProductList"] = homeService.QueryVisaProductList(product.VisaCountryParentStr, model.VType).ToSelectListFor(m => m.InformationCode, m => m.InformationName);

            return View("Visa/EditLineVisa", model);
        }

        /// <summary>
        /// 获取国家列表信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetVisaCountry(int vtype)
        {
            var query = homeService.QueryVisaCountryList(vtype, false);
            return Json(new { row = query.Count(), list = query });
        }

        /// <summary>
        /// 获取国家下的签证产品信息
        /// </summary>
        /// <param name="vtype"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetVisaProduct(int vtype, string country)
        {
            var query = homeService.QueryVisaProductList(country, vtype);
            return Json(new { row = query.Count(), list = query });
        }

        [HttpPost]
        public ActionResult SaveLineVisa(TpLineVisaModel visaModel)
        {
            try
            {
                int i = _lineVisaBiz.SaveLineVisa(visaModel);
                if (i > 0)
                {
                    return Json(new { Code = 1, Message = "保存成功！" });
                }

                return Json(new { Code = 0, Message = "保存失败！" });
            }
            catch (Exception ex)
            {
                return Json(new { Code = 0, Message = "保存失败！" + ex.Message });
            }
        }

        public ActionResult DeleteLineVisa(int id)
        {
            try
            {
                int i = _lineVisaBiz.deleteLineVisa(id);

                return Json(new { Code = i, Messgae = "" });
            }
            catch (Exception ex)
            {
                return Json(new { Code = 0, Message = "删除失败!" + ex.Message });
            }
        }

        //public ActionResult GetVisaGroup(int continent, int vtype)
        //{
        //    var list = GetCachedVisaProduct().Select(t => t.Value).ToList();
        //    if (vtype != 0)
        //    {
        //        list = list.Where(t => t.VType == vtype).ToList();
        //    }
        //    if (continent != 0)
        //    {
        //        list = list.Where(t => t.Continent == continent).ToList();
        //    }
        //    var json = new { row = list.Count(), list = list };

        //    return Json(json);
        //}

        /// <summary>
        /// 获取账户的名称
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public VisaInformationModel GetCachedVisaPrduct(string code)
        {
            var dic = GetCachedVisaProduct();
            if (!dic.Keys.Contains(code))
            {
                dic = GetVisaProduct();
                CacheContext.Current.Add(Consts.VisaProduct, dic);
                if (!dic.Keys.Contains(code))
                    throw new Exception("没有对应的签证信息。code=" + code);
            }
            return dic[code];
        }

        /// <summary>
        /// 获取表的code 和 name
        /// key = code
        /// value = name
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, VisaInformationModel> GetCachedVisaProduct()
        {
            Dictionary<string, VisaInformationModel> dic = null;
            var obj = CacheContext.Current.Get(Consts.VisaProduct);

            if (obj == null)
            {
                dic = GetVisaProduct();
                CacheContext.Current.Add(Consts.VisaProduct, dic);
            }
            else
                dic = obj as Dictionary<string, VisaInformationModel>;

            return dic;
        }

        private Dictionary<string, VisaInformationModel> GetVisaProduct()
        {
            var dic = new Dictionary<string, VisaInformationModel>();
            var list = homeService.QueryVisaProductList("", 0);
            foreach (var item in list)
            {
                dic.Add(item.InformationCode, item);
            }
            return dic;
        }

        #endregion 签证部分
    }
}