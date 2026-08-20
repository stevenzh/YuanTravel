using Lvy.API.Biz.Product;
using Lvy.APIVModels.Req;
using Lvy.APIVModels.Res;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lvy.NetCore.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LineController : ControllerBase
    {

        private readonly ILogger<LineController> _logger;

        public LineController(ILogger<LineController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取产品列表
        /// </summary>
        /// <param name="productReq"></param>
        /// <returns></returns>
        //[HttpGet]
        //public GetProductsResponse GetProductList(GetProductsRequest productReq)
        //{
        //    productReq.LineType = 0;
        //    productReq.OwnerCode = "";
        //    productReq.QueryDay = 3;  // int.Parse(ConfigurationManager.AppSettings["QueryDays"]);
        //    return new ProductBiz().SearchProducts(productReq);
        //}

        /// <summary>
        /// 获取团状态信息
        /// </summary>
        /// <param name="tourReq">请求</param>
        /// <returns></returns>
        [HttpGet]
        //[ApiActionFilter]
        public GetTourResponse GetTour(GetTourRequest tourReq)
        {
            tourReq.OwnerCode = "";
            GetTourResponse tourRes = new ProductBiz().GetTour(tourReq);
            return tourRes;
        }

    }

}
