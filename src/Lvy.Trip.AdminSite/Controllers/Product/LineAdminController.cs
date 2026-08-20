using Arch.Common;
using Common.Logging;
using log4net.Repository.Hierarchy;
using Lvy.Models;
using Lvy.Models.ProductDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz.Base;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Biz.Site;
using Lvy.Visa.Biz;
using Lvy.Visa.Models;
using Lvy.VModels.Product;
using Lvy.Web.Common;
using Lvy.Web.Common.Cache;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Product
{
    /// <summary>
    /// 产品模块
    ///
    /// 权限设置  计调总监产：所有产品， 计调组长 本组所以产品 ，计调 本组所以产品
    /// </summary>
    public partial class LineAdminController : BaseController
    {
        private ILog logger = LogManager.GetLogger("LineAdminController");

        private readonly TpLineBiz _tpLineBiz = new TpLineBiz();
        private readonly TpLineRouteBiz _tpRouteBiz = new TpLineRouteBiz();
        private readonly TpLineBusPointBiz _tpLineBusPointBiz = new TpLineBusPointBiz();
        private readonly TpLineVisaBiz _lineVisaBiz = new TpLineVisaBiz();
        private readonly TpLineAdminBiz _adminBiz = new TpLineAdminBiz();
        private readonly BaseTagBiz baseTagBiz = new BaseTagBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();

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
            // 取得查询分页条件
            var q = (TpLineVModel)CacheContext.Current.Get(Consts.PageLineController + GlobalContext.Current.UserInfo.Code);
            if (q != null && vModel.FirstTime)
                vModel = q;

            vModel.OwnerCode = UserInfo.OwnerCode;
            vModel.CustomerCode = UserInfo.CustomerCode;

            if (vModel.LineList == null)
                vModel.LineList = new PagedList<TpLineModel>();

            //分组下拉框=数据初始化  查询职能为计调的分组信息.
            var teams = new List<SelectListItem>();
            if (GlobalContext.Current.IsSysAdmin || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调总监"))
            {
                teams = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else
            {
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 2 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                if (string.IsNullOrEmpty(vModel.CrmTeamId) && teams.Where(t => t.Value != "").Count() > 0)  // 默认部门赋值
                {
                    vModel.CrmTeamId = teams.Where(t => t.Value != "").FirstOrDefault().Value;
                }
            }
            ViewBag.AccountTeamBeans = teams;
            ViewData["ImportType"] = new List<KeyValueBean>
                                     {
                                         new KeyValueBean{Key="0",Value="公司录入"},
                                         new KeyValueBean{Key="1",Value="外社录入"}
                                     }.ToSelectListFor();

            // 保存查询分页条件
            CacheContext.Current.Add(Consts.PageLineController + GlobalContext.Current.UserInfo.Code, vModel, Consts.OutputCacheDuration2);

            _tpLineBiz.GetLineList(vModel);
            vModel.FirstTime = false;

            InitLineTypeSelectItems(vModel.LineType, vModel.LineScope);

            if (Request.IsAjaxRequest())
            {
                return PartialView("Line/UCLineList", vModel);
            }
            return View("Line/SearchLine", vModel);
        }

        /// <summary>
        /// 编辑线路专管员
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        public ActionResult EditLineAdmin(string lineId, string teamId)
        {
            var vModel = _adminBiz.GetEditLineAdminVModel(lineId, teamId);
            return PartialView("Line/UCEditLineAdmin", vModel);
        }

        /// <summary>
        /// 保存线路专管员
        /// </summary>
        /// <param name="lineId"></param>
        /// <param name="customer"></param>
        /// <param name="plat"></param>
        /// <returns></returns>
        public ContentResult SaveLineAdmin(string lineId, List<LineAdminVModel> customer, List<LineAdminVModel> plat)
        {
            var vModel = new EditLineAdminVModel
            {
                LineId = lineId,
                CustomerLineAdmin = customer ?? new List<LineAdminVModel>(),
                PlatLineAdmin = plat ?? new List<LineAdminVModel>()
            };
            return Content(_adminBiz.SaveLineAdmin(vModel));
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

            //分组下拉框=数据初始化  查询职能为计调的分组信息.
            string defaultTeam = "";

            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调") || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长"))
            {
                var currentTeams = _teamBiz.GetOpTeams(OwnerCode).Where(a => GlobalContext.Current.LoginUserTeams.Select(b => b.TeamID).Contains(a.TeamID)).ToList();
                if (currentTeams.Count == 0)
                {
                    ViewBag.AccountTeamBeans = currentTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                }
                else
                {
                    if (currentTeams.Select(b => b.TeamID).Contains(vModel.TpLine.TeamID))
                    {
                        ViewBag.AccountTeamBeans = currentTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName, vModel.TpLine.TeamID);
                        defaultTeam = vModel.TpLine.TeamID;
                    }
                }
            }
            else   // 计调总监 或 管理员
            {
                ViewBag.AccountTeamBeans = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName, vModel.TpLine.TeamID);
                defaultTeam = vModel.TpLine.TeamID;
            }
            if (defaultTeam.IsNullOrEmpty())
                ViewBag.LineBrandList = DictionaryTools.GetCachedBrandDict().Select(t => t.Value).ToSelectListFor(t => t.Code, t => t.Name);
            else
            {
                ViewBag.LineBrandList = DictionaryTools.GetCachedBrandDict().Select(t => t.Value).Where(t => t.TeamID == defaultTeam).ToSelectListFor(t => t.Code, t => t.Name);
                vModel.LockName = _teamBiz.GetTeam(defaultTeam).LockName;
            }

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
                        LineType = 1,//默认 跟团
                        LineScope = 4, // 默认 出境
                        TrafficType = 3,//默认 飞机
                        DepartDest = listOutCity[0].Key
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
                    OutCities = listOutCity,
                    TpLine = new TpLineModel
                    {
                        LineType = 1,//默认线路类型 跟团
                        LineScope = 4, // 默认 出境
                        TrafficType = 3,//默认交通类型 飞机
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

            //分组下拉框=数据初始化  查询职能为计调的分组信息.
            string defaultTeam = "";
            if (GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调") || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调组长"))
            {
                // 取得当前用户的计调组
                var currentTeams = _teamBiz.GetOpTeams(OwnerCode).Where(a => GlobalContext.Current.LoginUserTeams.Select(b => b.TeamID).Contains(a.TeamID)).ToList();
                if (currentTeams.Count == 0)
                {
                    // 这个不太合适
                    ViewBag.AccountTeamBeans = currentTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName);
                }
                else
                {
                    if (string.IsNullOrEmpty(id))   // 新增产品
                    {
                        // 默认第一个部门
                        ViewBag.AccountTeamBeans = currentTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName, currentTeams.FirstOrDefault().TeamID);
                        defaultTeam = currentTeams.FirstOrDefault().TeamID;
                    }
                    else if (currentTeams.Select(b => b.TeamID).Contains(vModel.TpLine.TeamID))
                    {
                        // 当前用户包含产品所在组 太棒了
                        ViewBag.AccountTeamBeans = currentTeams.ToSelectListFor(t => t.TeamID, v => v.TeamName, vModel.TpLine.TeamID);
                        defaultTeam = vModel.TpLine.TeamID;
                    }
                }
            }
            else    // 计调总监 或 管理员
            {
                if (string.IsNullOrEmpty(id))   // 新增产品
                {
                    ViewBag.AccountTeamBeans = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
                }
                else
                {
                    ViewBag.AccountTeamBeans = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName, vModel.TpLine.TeamID);
                    defaultTeam = vModel.TpLine.TeamID;
                }
            }

            if (defaultTeam.IsNullOrEmpty())
                ViewBag.LineBrandList = DictionaryTools.GetCachedBrandDict().Select(t => t.Value).ToSelectListFor(t => t.Code, t => t.Name);
            else
            {
                // 如果可以确定部门
                ViewBag.LineBrandList = DictionaryTools.GetCachedBrandDict().Select(t => t.Value).Where(t => t.TeamID == defaultTeam).ToSelectListFor(t => t.Code, t => t.Name);
                vModel.LockName = _teamBiz.GetTeam(defaultTeam).LockName;
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
        private void InitLineTypeSelectItems(string lineType, string lineScope)
        {
            ViewBag.LineTypeSelectItems = DictionaryTools.GetEnumsBy(Enums.LineTypeEnum).ToSelectListFor();
            ViewBag.LineScopeSelectItems = DictionaryTools.GetEnumsBy(Enums.LineScopeEnum).ToSelectListFor();
        }

        /// <summary>
        /// 初始化线路编辑所需信息
        /// </summary>
        private void InitEditLineData()
        {
            ViewBag.LineTypeRadioItems = DictionaryTools.GetEnumsBy(Enums.LineTypeEnum);
            ViewBag.LineScopeRadioItems = DictionaryTools.GetEnumsBy(Enums.LineScopeEnum);
            ViewBag.TrafficTypeRadioItems = DictionaryTools.GetEnumsBy(Enums.TrafficTypeEnum);
            ViewBag.ThemesList = baseTagBiz.GetTags(UserInfo.OwnerCode);
        }

        /// <summary>
        /// 获取当前同一customercode下所有账号
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        private List<KeyValueBean> GetAccountList(string customerCode)
        {
            var accountBiz = new AccountBiz();
            return accountBiz.GetAllAccountBeans(customerCode, UserInfo.OwnerCode);
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
        /// <param name="id">线路ID</param>
        /// <returns></returns>
        public ActionResult SelectBusPoint(string id)
        {
            SelectBusPointVModel vModel = new SelectBusPointVModel();
            var baseBusPointBiz = new BusPointBiz();
            //若LineI大于0，则是通过连接而来，否则为查询的情况
            if (!string.IsNullOrEmpty(id))
            {
                vModel.Line = _tpLineBiz.GetLineById(id);
                var baseBusPoints = baseBusPointBiz.GetBusPointByGroup(vModel.Line.DepartDest, vModel.GroupId, GlobalContext.Current.OwnerCode);
                vModel.BusPointList = _tpLineBusPointBiz.GetSelectBusPointsList(baseBusPoints, vModel);
            }

            vModel.GroupItems = baseBusPointBiz.GetGroupList(vModel.Line.DepartDest, GlobalContext.Current.OwnerCode);
            return View("BusPoint/SelectBusPoint", vModel);
        }

        public ActionResult UCBusPointList(SelectBusPointVModel vModel)
        {
            var baseBusPointBiz = new BusPointBiz();
            //若LineI大于0，则是通过连接而来，否则为查询的情况
            if (!string.IsNullOrEmpty(vModel.LineId))
            {
                vModel.Line = _tpLineBiz.GetLineById(vModel.LineId);
                var baseBusPoints = baseBusPointBiz.GetBusPointByGroup(vModel.Line.DepartDest, vModel.GroupId, UserInfo.OwnerCode);
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
            try
            {
                _tpLineBusPointBiz.SaveBusPoint(vModel.BusPointList, UserInfo.Code, UserInfo.OwnerCode);
                return Json(new { Code = "1", Message = "" });
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return Json(new { Code = "1", Message = ex.Message });
            }
        }

        #endregion 上车点

        #region 成本规则模板

        /// <summary>
        /// 显示成本规则模板
        /// </summary>
        /// <returns></returns>
        public ActionResult ShowCostRules(string Id)
        {
            TpLineCostRuleBiz costRuleBiz = new TpLineCostRuleBiz();
            LineCostRuleVModel vModel = new LineCostRuleVModel();
            vModel.CostModels = costRuleBiz.GetByLineId(Id);

            vModel.TpLine = _tpLineBiz.GetLineById(Id);
            string ownerCode = GlobalContext.Current.OwnerCode;

            ViewBag.Suppliers = new CustomerBiz().GetSupplierList(ownerCode).Select(a => new KeyValueBean()
            {
                Key = a.Code,
                Value = a.Name
            });
            return View("CostRule/ShowCostRules", vModel);
        }

        public ActionResult SaveCostRule(LineCostRuleVModel vModel)
        {
            TpLineCostRuleBiz costRuleBiz = new TpLineCostRuleBiz();
            //delete all by lineId
            costRuleBiz.DeleteCostByLineId(vModel.TpLine.LineId);
            // insert
            if (vModel.CostModels != null)
                costRuleBiz.InsertBatch(vModel.CostModels.Where(a => a.IsValid == 1).ToList());

            return SaveResult("1", Url.Action("ShowCostRules", new { id = vModel.TpLine.LineId }));
        }

        public ActionResult AddRowCost(int rowIndex, string lineId)
        {
            ViewBag.RowIndex = rowIndex;
            LineCostRuleVModel vModel = new LineCostRuleVModel();

           vModel.TpLine = _tpLineBiz.GetLineById(lineId);
            string ownerCode = GlobalContext.Current.OwnerCode;

            ViewBag.Suppliers = new CustomerBiz().GetSupplierList(ownerCode).Select(a => new KeyValueBean()
            {
                Key = a.Code,
                Value = a.Name
            });
            return PartialView("CostRule/UCRowCost", vModel);
        }

        #endregion 成本规则模板

        #region 上传行程信息

        public ActionResult UCAddFile(string id)
        {
            AddLineVModel vModel = new AddLineVModel();
            if (!string.IsNullOrEmpty(id))
            {
                vModel.LineFileVModel.LineFileList = _tpLineBiz.GetLineFileList(id);
                vModel.TpLine = _tpLineBiz.GetLineById(id);
            }

            // 词典中找到 线路相关类型
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

            return PartialView("File/UCFileList", vModel1);
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