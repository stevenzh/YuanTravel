using Lvy.Models.BaseDB;
using Lvy.Trip.Biz.Base;
using Lvy.VModels;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 平台功能-目的地管理
    /// </summary>
    public class DestinationController : BaseController
    {
        private DestinationBiz _biz = new DestinationBiz();

        public ActionResult Search()
        {
            return View();
        }

        /// <summary>
        /// 添加/编辑目的地
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            ViewBag.LevelList = DictionaryTools.GetEnumsBy(Enums.DestLevelEnum).ToSelectListFor();
            BaseDestinationModel model = new BaseDestinationModel();
            if (id == default(int))
            {
                model.ParentId = 0;
                model.IsChina = 0;
                model.ParentName = "";
            }
            else
                model = _biz.GetById(id);

            return View(model);
        }

        /// <summary>
        /// 添加子目的地-视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditChild(int id)
        {
            DestinationVModel vModel = new DestinationVModel();
            if (id != default(int))
            {
                BaseDestinationModel parent = _biz.GetById(id);
                vModel.DestModel = new BaseDestinationModel();
                vModel.DestModel.ParentId = id;
                vModel.DestModel.IsChina = parent.IsChina;
                vModel.DestModel.ParentName = parent.Name;
            }
            ViewBag.LevelList = DictionaryTools.GetEnumsBy(Enums.DestLevelEnum).ToSelectListFor();
            return View(vModel);
        }

        /// <summary>
        /// 添加子目的地-保存
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveChild(BaseDestinationModel formValues)
        {
            DestinationVModel vModel = new DestinationVModel();
            string result = "1";//返回数据Id
            if (formValues.Id == 0)
            {
                //add
                vModel.DestModel.IsValid = 1; // 默认 有效
                vModel.DestModel.ModifiedBy = UserInfo.Code;
                vModel.DestModel.ModifiedTime = DateTime.Now;
                vModel.DestModel.ParentId = Request.Params["DestModel.ParentId"].ToInt();
                vModel.DestModel.Name = Request.Params["DestModel.Name"];
                vModel.DestModel.PinYin = Request.Params["DestModel.PinYin"];
                vModel.DestModel.JPinYin = Request.Params["DestModel.JPinYin"];
                vModel.DestModel.IsChina = Request.Params["DestModel.IsChina"].ToInt();
                vModel.DestModel.Level = Request.Params["DestModel.Level"].ToInt();
                vModel.DestModel.RegionCode = Request.Params["DestModel.RegionCode"];

                _biz.Add(vModel.DestModel);
            }
            else
            {
                result = "0";
            }
            return Content(result);
        }

        /// <summary>
        /// 修改目的地-保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult UpdateDest(BaseDestinationModel model)
        {
            if (model.Id == default(int))
            {
                model.ModifiedTime = DateTime.Now;
                model.ModifiedBy = UserInfo.Code;
                model.IsValid = 1;
                _biz.Add(model);
            }
            else
            {
                var newObj = _biz.GetById(model.Id);
                newObj.Name = model.Name;
                newObj.PinYin = model.PinYin;
                newObj.JPinYin = model.JPinYin;
                newObj.IsChina = model.IsChina;
                newObj.Level = model.Level;
                newObj.ModifiedBy = UserInfo.Code;
                newObj.ModifiedTime = DateTime.Now;
                newObj.RegionCode = model.RegionCode;
                _biz.Update(newObj);
            }
            return Json(new { Code = "1", Message = "保存成功" });
        }

        /// <summary>
        /// 数据残缺整理
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateData()
        {
            int row = _biz.UpdateData();

            return Content(row.ToString()); ;
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetValidStateById(int id)
        {
            _biz.SetValidStateByDest(id);
            return Content("1");
        }

        #region 自定义函数

        /// <summary>
        /// 验证目的地是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public ActionResult CheckDesName(string name, int id = 0, int parentId = 0)
        {
            string result = "0";
            var model = new BaseDestinationModel();
            model = _biz.CheckDesName(name, id, parentId);
            result = model != null ? "1" : "0";
            return Content(result);
        }

        public ActionResult GetJsTreeData()
        {
            string parentID = "0";
            string treeJson = "";
            var list = _biz.GetAllDestination();
            if (list != null && list.Count() > 0)
            {
                treeJson = listToJson(list, "ParentId", parentID, "Id", "Name", "isParent").Substring(12);
            }
            return Content(treeJson);
        }

        public static string listToJson(List<BaseDestinationModel> list, string pField, string pValue, string kField, string TextField, string isParent, int level = 0)
        {
            StringBuilder sb = new StringBuilder();
            var tempModels = list.Where(a => a.ParentId == pValue.ToInt()).ToList();

            if (tempModels == null || tempModels.Count() < 1)
                return "";
            sb.Append(",\"children\":[");
            foreach (var item in tempModels)
            {
                string pcv = item.Id.ToString();
                sb.Append("{");
                sb.AppendFormat("\"text\":\"{0}\",", item.Name);
                sb.Append("\"data\":{");
                sb.AppendFormat("\"id\":\"{0}\",\"jpinyin\":\"{1}\",\"pinyin\":\"{2}\",\"level\":\"{3}\",\"RegionCode\":\"{4}\",\"ParentStr\":\"{5}\" ", item.Id, item.JPinYin, item.PinYin, item.LevelName, item.RegionCode, item.ParentStr);
                sb.Append("}");
                sb.Append(listToJson(list, pField, pcv, kField, TextField, isParent, 0).TrimEnd(','));
                sb.Append("},");
            }
            if (sb.ToString().EndsWith(","))
            {
                sb.Remove(sb.Length - 1, 1);
            }
            sb.Append("]");
            //sb.Append(",\"state\":{ ");
            //sb.AppendFormat(" \"opened\": {0}", (level > 15 ? "true" : "false"));
            //sb.Append(" }");

            return sb.ToString();
        }

        #endregion 自定义函数
    }
}