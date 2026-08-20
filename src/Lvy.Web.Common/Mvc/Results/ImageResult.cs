using System;
using System.Web.Mvc;
using System.IO;
using System.Web;

namespace Lvy.Web.Common.Mvc.Results
{
    public class ImageResult : ActionResult
    {
        private MemoryStream _imgStream;
        private string _contentType;
        public ImageResult(MemoryStream imgStream, string contentType)
        {
            _imgStream = imgStream;
            _contentType = contentType;
        }


        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentException("context");
            if (_imgStream == null)
                throw new ArgumentException("imgStream is null");
            if (_contentType == null)
                throw new ArgumentException("contentType is null");

            HttpResponseBase response = context.HttpContext.Response;
            response.Clear();
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.ContentType = _contentType;
            _imgStream.WriteTo(response.OutputStream);
        }

    }


}
