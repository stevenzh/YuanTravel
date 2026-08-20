using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Net;
using Common.Logging;
using Newtonsoft.Json;
using Arch.Common;
using Arch.Common.Utils;
using Lvy.Web.Common;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Web.Common.FileUpload;
using Lvy.Models;
using Lvy.VModels.Order;
using Lvy.VModels.Contract;
using Lvy.Trip.Biz.Order;
using Lvy.Trip.Biz.Product;
using Lvy.Trip.Biz.Contract;
using Lvy.Trip.Biz;

namespace Lvy.Trip.AdminSite.Controllers.Op
{

    /// <summary>
    /// 旅游合同
    /// 查询列表，添加，编辑，审核 
    /// </summary>
    public class ContractController : BaseController
    {

        private OrderBiz _biz = new OrderBiz();
        private TpLineTourPlanBiz tourPlanBiz = new TpLineTourPlanBiz();
        private ContractInfoBiz _contractBiz = new ContractInfoBiz();
        private static readonly ILog logger = LogManager.GetLogger(typeof(ContractController));

        // GET: Contract
        [LvyAuth]
        public ActionResult Index(ConditionVModel vModel)
        {
            //根据条件获取对应的订单列表信息
            vModel.PagedList = _contractBiz.GetPageList(vModel);
            if (Request.IsAjaxRequest())
                return PartialView("UCSearchContract", vModel);
            return View(vModel);
        }


        [LvyAuth]
        public ActionResult CreateContract(string orderCode)
        {
            ViewBag.Title = "合同申报";
            ViewBag.OrderCode = orderCode;
            var vModel = new OrderEditVModel();
            vModel.Order = _biz.GetOrderLineTourist(orderCode);
            vModel.Travellers = vModel.Order.TravellerModels;//游客列表
            vModel.Travellers2 = vModel.Travellers.Where(a => a.State == 2).ToList(); // 有效
            vModel.TourPlan = tourPlanBiz.GetTourById(vModel.Order.TourId);
            ViewBag.Contract = _contractBiz.GetContractDetails(orderCode);
            return View(vModel);
        }

        [LvyAuth]
        public ActionResult Additions(AdditionsVModel vModel)
        {
            if (vModel == null) vModel = new AdditionsVModel();
            vModel.List = _contractBiz.GetAdditionsList(vModel.content);
            if (Request.IsAjaxRequest())
                return PartialView("UCSearchAdditions", vModel);
            return View(vModel);
        }

        public ActionResult CreateAdditions(int? id)
        {
            ViewBag.Id = id;
            ContractAdditions additions = new ContractAdditions();
            if (id != null)
            {
                additions = _contractBiz.GetAdditionsDetails(Convert.ToInt32(id));
            }
            return View(additions);
        }

        [HttpPost]
        public ActionResult CreateAdditions(ContractAdditions model)
        {
            try
            {
                model.createTime = DateTime.Now;
                model.createBy = GlobalContext.Current.UserInfo.Code;
                bool result = _contractBiz.SaveUpdateAdditions(model);
                if (result)
                {
                    return Json(new { code = "0", msg = "保存成功" });
                }
                return Json(new { code = "100", msg = "保存失败" });
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return Json(new { code = "100", msg = "服务器异常，请稍后再试" });
            }
        }

        [HttpPost]
        public ActionResult DelAdditons(string id)
        {
            bool result = _contractBiz.DeleteAdditions(Convert.ToInt32(id));
            if (result)
            {
                return Json(new { code = "0", msg = "删除成功" });
            }
            return Json(new { code = "100", msg = "删除失败" });
        }

        /// <summary>
        /// 创建合同
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveContract()
        {
            try
            {
                ContractVModel model = new ContractVModel();
                //读取上传的信息
                string reqBody = new StreamReader(HttpContext.Request.InputStream).ReadToEnd();
                model = JsonConvert.DeserializeObject<ContractVModel>(reqBody);
                bool result = _contractBiz.SaveUpdateContract(model, GlobalContext.Current.UserInfo);
                if (!result)
                    return Json(new { code = "100", msg = "合同保存失败" });
                return Json(new { code = "0", msg = "合同保存成功" });
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return Json(new { code = "100", msg = "服务器异常，请稍后重试" });
            }
        }

        [HttpPost]
        public ActionResult ApplyContract(string orderCode)
        {
            try
            {
                LogBiz.WriteOrderLog(UserInfo.OwnerCode, orderCode, "", GlobalContext.Current.UserInfo.Code, "发起合同申报", 0);

                //读取上传的信息
                string reqBody = new StreamReader(HttpContext.Request.InputStream).ReadToEnd();
                ContractApiModel model = new ContractApiModel();
                model = JsonConvert.DeserializeObject<ContractApiModel>(reqBody);
                model.callbackURL = AppSetting.Get("receiveContractMsgUrl");//合同消息回调地址
                ReqHeader reqHeader = GetReqHeader();
                string url = reqHeader.econtractUrl + "/v1/econtract/apply/" + model.contractNumber + "";
                byte[] param = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
                //创建一个新的HttpWebRequest对象。
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.ContentType = "application/json;charset=utf-8";
                req.Headers.Add("x-12301-source", reqHeader.appId);
                req.Headers.Add("x-12301-key", reqHeader.appId);
                req.Headers.Add("x-12301-version", reqHeader.econtractVersion);
                req.Headers.Add("x-12301-timestamp", reqHeader.timestamp);
                req.Headers.Add("x-12301-signature", reqHeader.signature);
                req.Method = "POST";
                req.ContentLength = param.Length;
                string text = "";
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(param, 0, param.Length);
                    reqStream.Close();
                    HttpWebResponse response2 = (HttpWebResponse)req.GetResponse();
                    StreamReader sr2 = new StreamReader(response2.GetResponseStream(), Encoding.UTF8);
                    text = sr2.ReadToEnd();
                }

                return Json(new { code = "0", data = text });
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return Json(new { code = "100", msg = "服务器异常，请稍后再试" });
            }

        }


        /// <summary>
        /// 成功申报后更新
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateContract()
        {
            try
            {
                ContractVModel model = new ContractVModel();
                //读取上传的信息
                string reqBody = new StreamReader(HttpContext.Request.InputStream).ReadToEnd();
                model = JsonConvert.DeserializeObject<ContractVModel>(reqBody);
                LogBiz.WriteOrderLog(UserInfo.OwnerCode, model.Contract.orderCode, "", GlobalContext.Current.UserInfo.Code, "合同申报成功", 0);

                bool result = _contractBiz.UpdateContractByOrderCode(model);
                if (!result)
                    return Json(new { code = "100", msg = "合同修改失败" });
                return Json(new { code = "0", msg = "合同修改成功" });
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return Json(new { code = "100", msg = "服务器异常，请稍后重试" });
            }
        }

        [HttpPost]
        public PartialViewResult UCAdditions(string content)
        {
            var list = _contractBiz.GetAdditionsList(content);
            return PartialView("UCAdditions", list);
        }

        public ActionResult Show()
        {
            return View();
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="TourId"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadContractFile(string orderCode)
        {
            try
            {
                string file_name = "";
                HttpPostedFileBase file = Request.Files["ContractFileName"];
                if (file == null || file.ContentLength <= 0)
                    return Json(new { code = "100", msg = "请选择需要上传的文件" });

                file_name = file.FileName;
                string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(file.FileName);
                string[] arryType = new string[] { ".pdf", ".doc" };
                if (!arryType.Contains(Path.GetExtension(file.FileName).ToLower()))
                {
                    return Json(new { code = "100", msg = "只允许上传.pdf|.doc格式的附件" });
                }
                if (file.ContentLength > 10240000)
                {
                    return Json(new { code = "100", msg = "文件不能超过10M" });
                }
                UploadFileRequest request = new UploadFileRequest();
                request.FileName = filename;
                request.FileStream = Toolkit.Image.StreamToBytes(file.InputStream);
                // 所属客户code\文件类型
                request.VirtualPath = string.Format(@"order\{0}\contract", orderCode);

                UploadServiceClient client = new UploadServiceClient();
                UploadFileResponse response = client.UploadFile(request);
                ContractFiles fileInfo = new ContractFiles();
                fileInfo.createTime = DateTime.Now;
                fileInfo.fileName = file_name;
                fileInfo.filePath = response.FilePath + response.FileName;
                return Json(new { code = "0", data = fileInfo });
            }
            catch (Exception)
            {
                return Json(new { code = "100", msg = "服务器异常，请稍后重试" });
            }
        }

        /// <summary>
        /// 合同状态更新消息回调接口
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public string ReceiveContractStateMsg()
        {
            try
            {
                //读取上传的信息
                string reqBody = new StreamReader(HttpContext.Request.InputStream).ReadToEnd();
                logger.Info("回调消息参数:" + reqBody);
                ReceiveMsg model = JsonConvert.DeserializeObject<ReceiveMsg>(reqBody);
                if (model == null)
                {
                    logger.Info("消息参数转换失败");
                    return "fail";
                }
                //获取合同信息
                ContractInfo contractInfo = _contractBiz.GetContractInfo(model.data.contractNumber);
                if (contractInfo == null)
                {
                    logger.Info("没有查询到合同信息");
                    return "fail";
                }
                Dictionary<string, string> asciiDic = new Dictionary<string, string>();
                asciiDic.Add("content", model.data.content);
                asciiDic.Add("state", model.data.state);
                asciiDic.Add("contractNumber", model.data.contractNumber);
                asciiDic.Add("signatoryIDNumber", model.data.signatoryIDNumber);
                asciiDic.Add("signatoryPhone", model.data.signatoryPhone);
                Dictionary<string, string> _asciiDic = new Dictionary<string, string>();
                string[] arrKeys = asciiDic.Keys.ToArray();
                Array.Sort(arrKeys, string.CompareOrdinal);
                foreach (var key in arrKeys)
                {
                    string value = asciiDic[key];
                    _asciiDic.Add(key, value);
                }
                string signStr = contractInfo.transactorPhone + contractInfo.licenseCode;
                foreach (var item in _asciiDic)
                {
                    signStr += item.Value;
                }
                logger.Info("签名字符串:" + signStr);
                string sign = new SecurityTools().ToMD5Encrypt(signStr);
                if (sign != model.data.sign)
                {
                    logger.Info("签名验证失败");
                    return "fail";
                }
                bool result = _contractBiz.UpdateContractState(model.data.contractNumber, model.data.state, model.data.content);
                if (result) return "success";
                logger.Info("合同状态更新失败");
                return "fail";
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                return "fail";
            }
        }


        private ReqHeader GetReqHeader()
        {
            ReqHeader header = new ReqHeader();
            header.appId = AppSetting.Get("appId");
            header.signKey = AppSetting.Get("signKey");
            header.econtractUrl = AppSetting.Get("econtractUrl");
            header.econtractVersion = AppSetting.Get("econtractVersion");
            header.timestamp = GetTimestamp();
            string signStr = new SecurityTools().ToMD5Encrypt(header.signKey + header.timestamp);
            string signBase64Str = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(signStr));
            header.signature = signBase64Str;
            return header;
        }

        private string GetTimestamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime curTime = DateTime.Now;
            long unixTime = (long)System.Math.Round((curTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime.ToString();
        }
    }
}