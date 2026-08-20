using Lvy.Models.BaseDB;
using Lvy.NetCore.API.Models;
using Lvy.Trip.Biz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lvy.NetCore.API.Controllers
{
    [ApiController]
    [Route("")]
    public class Select2Controller : ControllerBase
    {

        private readonly ILogger<Select2Controller> _logger;

        public Select2Controller(ILogger<Select2Controller> logger)
        {
            _logger = logger;
        }


        [HttpGet("Airline/GetAirlineSelect2")]
        public Select2Model GetAirlineSelect2(string term)
        {
            IList<BaseAirlineModel> list = DictionaryBiz.GetCachedAirlineDict().Values.ToList();

            if (string.IsNullOrEmpty(term))
            {
                list = list.OrderByDescending(a => a.Code).Take(12).ToList();
            }
            else
            {
                list = list.Where(a => a.ShortName.Contains(term) || a.Code == term.ToUpper()).Take(15).ToList();
            }
            var model = new Select2Model
            {
                incomplete_results = "false",
                items = list,
                total_count = list.Count
            };

            return model;
        }


        [HttpGet("Dest/GetPlaceSelect2")]
        public Select2Model GetPlaceSelect2(string term)
        {
            List<BasePlaceModel> dest = DictionaryBiz.GetCachePlaces();

            if (string.IsNullOrEmpty(term))
            {
                dest = dest.Take(12).ToList();
            }

            if (!string.IsNullOrEmpty(term))
            {
                term = term.ToUpper();
                dest = dest.Where(
                            a =>
                            (a.PinYin != null && a.PinYin.ToUpper().Contains(term)) ||
                            (a.JPinYin != null && a.JPinYin.ToUpper().Contains(term))
                            || (a.PlaceName != null && a.PlaceName.Contains(term))).Take(12).ToList();

            }
            var model = new Select2Model
            {
                incomplete_results = "false",
                items = dest,
                total_count = dest.Count
            };

            return model;
        }



        /// <summary>
        ///
        /// </summary>
        /// <param name="term"></param>
        /// <param name="inChina">包含中国</param>
        /// <returns></returns>
        [HttpGet("Dest/GetCountrySelect2")]
        public Select2Model GetCountrySelect2(string term)
        {
            List<BaseDestinationModel> dest = null;
            if (string.IsNullOrEmpty(term))
            {
                dest = DictionaryBiz.GetCacheDests().Where(a => a.Level == 5)
                        .OrderByDescending(a => a.ClickCnt).Take(12).ToList();
            }

            if (!string.IsNullOrEmpty(term))
            {
                term = term.ToUpper();

                dest = DictionaryBiz.GetCacheDests()
                        .Where(
                            a =>
                            a.Level == 5 &&
                            ((a.PinYin != null && a.PinYin.ToUpper().Contains(term)) ||
                            (a.JPinYin != null && a.JPinYin.ToUpper().Contains(term))
                            || (a.Name != null && a.Name.Contains(term)))).Take(12).ToList();
            }
            var model = new Select2Model
            {
                incomplete_results = "false",
                items = dest,
                total_count = dest.Count
            };

            return model;
        }
    }
}
