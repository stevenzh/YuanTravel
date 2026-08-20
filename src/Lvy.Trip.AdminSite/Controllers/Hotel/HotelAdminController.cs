using Arch.Common;
using Common.Logging;
using Lvy.Models.HotelDB;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Trip.Biz;
using Lvy.Trip.Biz.Crm;
using Lvy.VModels;
using Lvy.VModels.Hotel;
using Lvy.Web.Common;
using Lvy.Web.Common.FileUpload;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Hotel
{
    [LvyAuth]
    public class HotelAdminController : BaseController
    {
        private ILog logger = LogManager.GetLogger("HotelAdminController");

        public readonly HotelBiz _biz = new HotelBiz();
        private readonly TeamBiz _teamBiz = new TeamBiz();
        private readonly CustomerBiz customerBiz = new CustomerBiz();
        private readonly DictionaryBiz _commonBiz = new DictionaryBiz();

        // GET: Hotel
        public ActionResult Search(HotelVModel vModel)
        {
            vModel.OwnerCode = GlobalContext.Current.OwnerCode;
            vModel.Hotels = _biz.GetPagedList(vModel);

            var teams = new List<SelectListItem>();
            if (GlobalContext.Current.IsSysAdmin || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调总监"))
            {
                teams = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else
            {
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 2 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            ViewBag.AccountTeamBeans = teams;
            ViewBag.HotelLevels = DictionaryTools.GetEnumsBy(Enums.HotelLevelEnum).ToSelectListFor();

            return View(vModel);
        }

        /// <summary>
        /// 添加酒店初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            InitPage();

            HotelModel model = new HotelModel();
            model.CountryCode = "1";
            return View(model);
        }

        /// <summary>
        /// 添加酒店 - 保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(HotelModel model, string ImgPath)
        {
            try
            {
                model.OwnerCode = GlobalContext.Current.OwnerCode;
                model.CreatedBy = GlobalContext.Current.UserInfo.Code;
                model.CreatedTime = DateTime.Now;
                model.IsValid = 1;
                model.HotelState = 2;
                var code = _biz.InsertHotel(model);

                // 图片处理
                string[] p = ImgPath.Split(',').Where(t => t.IsNullOrEmpty() == false).ToArray();
                if (p.Length > 0)
                {
                    foreach (var pp in p)
                    {
                        // 保存图片  从临时文件夹发送到文件服务器
                        var path = System.Web.HttpContext.Current.Server.MapPath("\\uploads\\temp\\" + pp);
                        if (!System.IO.File.Exists(path))
                        {
                            continue;
                        }
                        string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(path);
                        StreamReader sr = new StreamReader(path);
                        UploadFileRequest request = new UploadFileRequest();
                        request.FileName = filename;
                        request.FileStream = Toolkit.Image.StreamToBytes(sr.BaseStream);
                        // 所属客户code\文件类型
                        request.VirtualPath = @"{0}\{1}".With("hotel", model.HotelCode);

                        UploadServiceClient client = new UploadServiceClient();
                        UploadFileResponse response = client.UploadFile(request);

                        // 保存图片记录
                        var model1 = new HotelFileModel();
                        model1.HotelCode = code;
                        model1.FileSize = 0;
                        model1.FilePath = response.FilePath + response.FileName; // 服务器文件路径
                        model1.IsValid = 1;
                        model1.ModifiedBy = GlobalContext.Current.UserInfo.Code;
                        model1.ModifiedTime = DateTime.Now;
                        model1.Type = "31"; //  固定值 31 酒店图片  32 房间图片
                        model1.FileName = "文档插图";
                        _biz.AddPhoto(model1);
                    }
                }

                return Json(new { Code = "1", HotelCode = code });
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return Json(new { Code = "0", Message = ex.Message });
            }
        }

        /// <summary>
        /// 编辑酒店 初始
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(string id)
        {
            HotelModel model = _biz.GetByCode(id);
            //model.FileList = _biz.GetFileList(id);

            ViewBag.CityList = _commonBiz.GetChildList(model.ProvinceCode).ToSelectListFor(v => v.Id.ToString(), v => v.Name, model.CityCode);
            ViewBag.CountyList = _commonBiz.GetChildList(model.CityCode).ToSelectListFor(v => v.Id.ToString(), v => v.Name, model.CityAreaCode);
            InitPage();

            return View(model);
        }

        /// <summary>
        /// 更新酒店 - 保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="Facility"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(HotelModel model, string[] Facility)
        {
            try
            {
                HotelModel entity = _biz.GetByCode(model.HotelCode);

                entity.HotelName = model.HotelName;
                entity.Introduction = model.Introduction;
                entity.CityCode = model.CityCode;
                entity.CityAreaCode = model.CityAreaCode;
                entity.Facility = string.Join(",", Facility);
                entity.RankCode = model.RankCode;

                _biz.Update(entity);

                return Json(new { Code = "1", Message = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { Code = "0", Message = ex.Message });
            }
        }

        [LvyAuth]
        public string ChangeOnline(string id)
        {
            return Convert.ToString(_biz.UpdateHotelState(id, 3));
        }

        [LvyAuth]
        public string ChangeOffline(string id)
        {
            return Convert.ToString(_biz.UpdateHotelState(id, 2));
        }

        // GET: Hotel/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Hotel/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        protected override void InitPage()
        {
            //分组下拉框=数据初始化  查询职能为计调的分组信息.
            var teams = new List<SelectListItem>();
            if (GlobalContext.Current.IsSysAdmin || GlobalContext.Current.LoginUserRoles.Any(role => role.Name == "计调总监"))
            {
                teams = _teamBiz.GetOpTeams(OwnerCode).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            else
            {
                teams = GlobalContext.Current.LoginUserTeams.Where(t => t.DepartCode == 2 || t.DepartCode == 1).ToSelectListFor(t => t.TeamID, v => v.TeamName);
            }
            ViewBag.AccountTeamBeans = teams;
            ViewBag.HotelLevels = DictionaryTools.GetEnumsBy(Enums.HotelLevelEnum).ToSelectListFor();
            ViewBag.ProvinceList = _commonBiz.GetProvinceList().ToSelectListFor(v => v.Id.ToString(), v => v.Name);
            ViewBag.HotelServices = DictionaryTools.GetEnumsBy(Enums.HotelServiceEnum).ToSelectListForNoDefualt();
        }

        public ActionResult PhotoView(string hotelCode)
        {
            HotelModel model = _biz.GetByCode(hotelCode);

            if (!hotelCode.IsNullOrEmpty())
                model.FileList = _biz.GetFileList(hotelCode);
            return PartialView("UCPhotoView", model);
        }

        public ActionResult SetPrimaryImage(int id, string hotelCode)
        {
            HotelFileModel fmodel = _biz.GetHotelFileModel(id);
            int i = _biz.SetPrimaryPic(hotelCode, fmodel.FilePath);

            // 重复 A
            HotelModel model = _biz.GetByCode(hotelCode);
            model.FileList = _biz.GetFileList(hotelCode);
            return PartialView("UCPhotoView", model);
        }

        [HttpPost]
        public ActionResult UploadPhoto(HotelFileModel model)
        {
            int fileSize = 0;
            var path = ToUploadPhoto("UploadFile", ref fileSize, model.HotelCode);
            if (string.IsNullOrEmpty(path))
                return Content("0");

            model.FileSize = fileSize;
            model.FilePath = path;
            model.IsValid = 1;
            model.ModifiedBy = GlobalContext.Current.UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            _biz.AddPhoto(model);
            return Content(model.FileID.ToString());
        }

        public ActionResult DeletePicture(int id)
        {
            _biz.DeleteFile(id);

            return Json(new CommonJsonResult { Code = "1", Message = "" });
        }

        public ActionResult UploadDocPhoto(HotelFileModel model)
        {
            int fileSize = 0;
            var path = ToUploadPhoto("UploadFile", ref fileSize, model.HotelCode);
            if (string.IsNullOrEmpty(path))
                return Content("0");

            model.FileSize = fileSize;
            model.FilePath = path;
            model.IsValid = 1;
            model.ModifiedBy = GlobalContext.Current.UserInfo.Code;
            model.ModifiedTime = DateTime.Now;
            model.FileName = "文档插图";
            _biz.AddPhoto(model);

            return Content(AppSetting.Get("UploadFileRoot") + path);
        }

        private string ToUploadPhoto(string fileName, ref int fileSize, string hotelCode)
        {
            HttpPostedFileBase file = Request.Files[fileName];
            if (file == null || file.ContentLength <= 0)
                return string.Empty;
            // 字节换算成K
            fileSize = file.ContentLength / 1024;
            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);

            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
            // 所属客户code\文件类型
            request.VirtualPath = @"{0}\{1}".With("hotel", hotelCode);

            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);

            return response.FilePath + response.FileName;
        }

        public ActionResult SaveUploadedFile()
        {
            bool isSavedSuccessfully = true;
            string fName = "";
            try
            {
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    //Save file content goes here
                    //fName = file.FileName;
                    if (file.InputStream.Length > 8388608)   // 1M
                    {
                        return Json(new { Message = "The file is too big. <1M." });
                    }

                    if (file != null && file.ContentLength > 0)
                    {
                        var originalDirectory = new DirectoryInfo(string.Format("{0}uploads", Server.MapPath(@"\")));

                        string pathString = Path.Combine(originalDirectory.ToString(), "temp");

                        string fileExtension = Path.GetExtension(file.FileName);
                        fName = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4) + fileExtension);

                        bool isExists = Directory.Exists(pathString);

                        if (!isExists)
                            Directory.CreateDirectory(pathString);

                        var path = string.Format("{0}\\{1}", pathString, fName);
                        file.SaveAs(path);
                    }
                }
            }
            catch (Exception ex)
            {
                isSavedSuccessfully = false;
            }

            if (isSavedSuccessfully)
            {
                return Json(new { Message = fName }); ;
            }
            else
            {
                return Json(new { Message = "Error in saving file" });
            }
        }

        /// <summary>
        /// 房型管理
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Stock(string id)
        {
            var model = _biz.GetByCode(id);
            model.RoomList = _biz.GetRooms(id);

            if (Request.IsAjaxRequest())
                return PartialView("UCStock", model);

            return View(model);
        }

        /// <summary>
        /// 添加房型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult RoomCreate(string id)
        {
            ViewBag.RoomFacilities = DictionaryTools.GetEnumsBy(Enums.RoomFacilityEnum).ToSelectListForNoDefualt();
            HotelRoomModel model = new HotelRoomModel();
            model.HotelCode = id;
            model.BedList = new List<HotelRoomBedModel>();
            model.BedList.Add(new HotelRoomBedModel { BedName = "", BedType = 1, BedNum = 1 });
            return View(model);
        }

        /// <summary>
        /// 更新房型
        /// </summary>
        /// <param name="model"></param>
        /// <param name="Facility"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RoomCreate(HotelRoomModel model, string[] Facility, string ImgPath)
        {
            try
            {
                model.RoomFacility = string.Join(",", Facility);
                _biz.AddRoom(model);

                var nn = model.BedList.Where(t => t.BedID == 0 && t.BedType != 0).ToList();
                foreach (var item in nn)
                {
                    item.RoomID = model.RoomID;
                    _biz.AddBed(item);
                }

                // 图片处理
                string[] p = ImgPath.Split(',').Where(t => t.IsNullOrEmpty() == false).ToArray();
                if (p.Length > 0)
                {
                    foreach (var pp in p)
                    {
                        // 保存图片  从临时文件夹发送到文件服务器
                        var path = System.Web.HttpContext.Current.Server.MapPath("\\uploads\\temp\\" + pp);
                        if (!System.IO.File.Exists(path))
                        {
                            continue;
                        }
                        string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(path);
                        StreamReader sr = new StreamReader(path);
                        UploadFileRequest request = new UploadFileRequest();
                        request.FileName = filename;
                        request.FileStream = Toolkit.Image.StreamToBytes(sr.BaseStream);
                        // 所属客户code\文件类型
                        request.VirtualPath = @"{0}\{1}".With("hotel", model.HotelCode);

                        UploadServiceClient client = new UploadServiceClient();
                        UploadFileResponse response = client.UploadFile(request);

                        // 保存图片记录
                        var model1 = new HotelFileModel();
                        model1.HotelCode = model.HotelCode;
                        model1.KeyId = model.RoomID;
                        model1.FileSize = 0;
                        model1.FilePath = response.FilePath + response.FileName; // 服务器文件路径
                        model1.IsValid = 1;
                        model1.ModifiedBy = GlobalContext.Current.UserInfo.Code;
                        model1.ModifiedTime = DateTime.Now;
                        model1.FileName = "文档插图";
                        model1.Type = "32";  //  固定值 31 酒店图片  32 房间图片
                        _biz.AddPhoto(model1);
                    }
                }

                return Json(new { Code = "1", Message = "" });
            }
            catch (Exception ex)
            {
                return Json(new { Code = "0", Message = ex.Message });
            }
        }

        public ActionResult RoomPhotoView(string hotelCode, int roomId)
        {
            HotelModel model = _biz.GetByCode(hotelCode);
            if (!hotelCode.IsNullOrEmpty())
                model.FileList = _biz.GetFileList(hotelCode, roomId);
            return PartialView("UCRoomPhotoView", model.FileList);
        }

        /// <summary>
        /// 添加床型
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public ActionResult AddRowBed(int rowIndex, int roomId)
        {
            HotelRoomBedModel model = new HotelRoomBedModel();
            model.RoomID = roomId;
            ViewBag.RowIndex = rowIndex;
            return View("UCRowBed", model);
        }

        /// <summary>
        /// 编辑房型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult RoomEdit(int id)
        {
            ViewBag.RoomFacilities = DictionaryTools.GetEnumsBy(Enums.RoomFacilityEnum).ToSelectListForNoDefualt();
            HotelRoomModel model = _biz.GetRoomByID(id);
            model.BedList = _biz.GetBedByRoomID(id);
            if (model.BedList.Count == 0)
            {
                model.BedList.Add(new HotelRoomBedModel { BedID = 0, BedType = 1, RoomID = id });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult RoomEdit(HotelRoomModel model, string[] Facility)
        {
            try
            {
                model.RoomFacility = string.Join(",", Facility);

                var entity = _biz.GetRoomByID(model.RoomID);
                entity.RoomName = model.RoomName;
                entity.AddBed = model.AddBed;
                entity.AddBedInfo = model.AddBedInfo;

                _biz.UpdateRoom(entity);

                // 床型
                var beds = _biz.GetBedByRoomID(model.RoomID); // 原有

                // 删除原有
                var dd = model.BedList.Where(m => m.BedID != 0).Select(m => m.BedID);
                _biz.delBeds(beds.Select(m => m.BedID).Except(dd).ToArray());

                // 更新现有
                var ll = model.BedList.Where(t => t.BedID != 0).ToList();
                foreach (var item in ll)
                {
                    _biz.UpdateBed(item);
                }

                // 添加新的
                var nn = model.BedList.Where(t => t.BedID == 0 && t.BedType != 0).ToList();
                foreach (var item in nn)
                {
                    _biz.AddBed(item);
                }

                return Json(new { Code = "1", Message = "" });
            }
            catch (Exception ex)
            {
                return Json(new { Code = "0", Message = ex.Message });
            }
        }

        public ActionResult RoomDelete(string id)
        {
            return View();
        }

        /// <summary>
        /// 房型库存
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hotelCode"></param>
        /// <returns></returns>
        public ActionResult RoomStock(int id, string hotelCode)
        {
            RoomVModel model = new RoomVModel();
            model.RoomStock.HotelCode = hotelCode;
            model.RoomStock.RoomID = Convert.ToInt32(id);
            return View(model);
        }

        /// <summary>
        /// 房型库存
        /// </summary>
        /// <param name="id">房型ID</param>
        /// <returns></returns>
        public ActionResult GetCalendar(int id)
        {
            var plans = _biz.GetRoomStock(id);
            var rr = (from ss in plans
                      select new
                      {
                          title = ss.MarketPrice.ToString("￥00") + "\n间数:" + ss.Quota,
                          start = ss.CheckInDate.ToDateFormat(),
                          backgroundColor = "#66cc99",
                          extendedProps = ss
                      }).ToList();

            //{
            //  title: 'Click for Google',
            //  start: new Date(y, m, 28),
            //  end: new Date(y, m, 29),
            //  url: 'http://google.com/',
            //  backgroundColor: '#3c8dbc', //Primary (light-blue)
            //  borderColor: '#3c8dbc' //Primary (light-blue)
            //}
            return Json(rr, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 更新放行库存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult UpdateStock(RoomVModel model)
        {
            try
            {
                var dates = model.SelectedDays.Split(',');
                foreach (var d in dates)
                {
                    if (!string.IsNullOrEmpty(d))
                    {
                        _biz.UpdateRoomStock(new HotelStockModel
                        {
                            HotelCode = model.RoomStock.HotelCode,
                            RoomID = model.RoomStock.RoomID,
                            MarketPrice = model.RoomStock.MarketPrice,
                            SettlePrice = model.RoomStock.SettlePrice,
                            CheckInDate = d.ToDateTime(),
                            Quota = model.RoomStock.Quota
                        });
                    }
                }

                return Json(new { Code = "1", Message = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { Code = "0", Message = ex.Message });
            }
        }
    }
}