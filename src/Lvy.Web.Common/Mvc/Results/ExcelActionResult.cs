using System.Web;
using System.Web.Mvc;

namespace Lvy.Web.Common.Mvc.Results
{
    public class ExcelActionResult : ActionResult
    {
        public byte[] Bytes { get; set; }
        public string FileName { get; set; }

        public ExcelActionResult(byte[] bytes, string fileName = "ExcelFile")
        {
            FileName = HttpUtility.UrlEncode(fileName) + ".xls";
            Bytes = bytes;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var fileResult = new FileContentResult(Bytes, "application/msexcel")
            {
                FileDownloadName = FileName
            };

            fileResult.ExecuteResult(context);
        }
    }
}