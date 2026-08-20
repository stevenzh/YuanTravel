using Lvy.Models.BaseDB;
using Lvy.NetCore.API.Models;
using Lvy.Trip.Biz;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lvy.NetCore.API
{
    [Route("")]
    [ApiController]
    public class DestinationController : ControllerBase
    {

        /// <summary>
        /// 商户功能-AJAX获得目的地
        /// </summary>
        /// <param name="fromCity"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        [HttpGet("Dest/GetDest")]
        public IEnumerable<BaseDestinationModel> GetDest(string fromCity, string term)
        {
            List<BaseDestinationModel> dest = null;
            if (string.IsNullOrEmpty(term))
            {
                dest = DictionaryBiz.GetCacheDests().Where(a => a.Level == 20 || a.Level == 15 || a.Level == 10)
                        .OrderByDescending(a => a.ClickCnt).Take(12).ToList();
            }

            if (!string.IsNullOrEmpty(term))
            {
                term = term.ToUpper();
                dest = DictionaryBiz.GetCacheDests()
                    .Where(
                        a =>
                        ((a.PinYin != null && a.PinYin.ToUpper().Contains(term)) ||
                        (a.JPinYin != null && a.JPinYin.ToUpper().Contains(term))
                        || (a.Name != null && a.Name.Contains(term)))
                        && (a.Level == 20 || a.Level == 10 || a.Level == 15)).Take(12).ToList();
            }

            return dest;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="fromCity"></param>
        /// <param name="term"></param>
        /// <param name="inChina">false 全部 true 仅国内</param>
        /// <returns></returns>
        [HttpGet("Dest/GetDestSelect2")]
        public Select2Model GetDestSelect2(string fromCity, string term, bool inChina = false)
        {
            List<BaseDestinationModel> dest = null;
            if (string.IsNullOrEmpty(term))
            {
                if (inChina)
                {
                    dest = DictionaryBiz.GetCacheDests().Where(a => a.IsChina == 1 && a.Level > 5)
                            .OrderByDescending(a => a.ClickCnt).Take(12).ToList();
                }
                else
                {
                    dest = DictionaryBiz.GetCacheDests().OrderByDescending(a => a.ClickCnt).Take(12).ToList();
                }
            }

            if (!string.IsNullOrEmpty(term))
            {
                term = term.ToUpper();
                if (inChina)
                {
                    dest = DictionaryBiz.GetCacheDests()
                        .Where(
                            a =>
                             a.IsChina == 1 &&
                            ((a.PinYin != null && a.PinYin.ToUpper().Contains(term)) ||
                            (a.JPinYin != null && a.JPinYin.ToUpper().Contains(term))
                            || (a.Name != null && a.Name.Contains(term)))
                            && (a.Level == 20 || a.Level == 10 || a.Level == 15)).Take(12).ToList();
                }
                else
                {
                    dest = DictionaryBiz.GetCacheDests()
                            .Where(
                                a =>
                                (a.PinYin != null && a.PinYin.ToUpper().Contains(term)) ||
                                (a.JPinYin != null && a.JPinYin.ToUpper().Contains(term))
                                || (a.Name != null && a.Name.Contains(term))).Take(12).ToList();
                }
            }
            var model = new Select2Model
            {
                incomplete_results = "false",
                items = dest,
                total_count = dest.Count
            };

            return model;
        }

        [HttpGet("Dest/GetAreaList")]
        public AreaListModel GetAreaList(string term, int pageIndex, int pageSize)
        {
            var allist = DictionaryBiz.GetCacheDests().Where(a => a.Name.Contains(term)).ToList();
            var list = allist.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var model = new AreaListModel
            {
                List = list,
                ReturnMsg = "0000",
                TotalCount = allist.Count()
            };

            return model;
        }
    }
}
