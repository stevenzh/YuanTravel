using System.Web.Mvc;
using System.Collections;
using Arch.Common.IO;

namespace Lvy.Web.Common.Mvc.Results
{
    public class CsvActionResult : ActionResult
    {
        public IEnumerable ModelListing { get; set; }

        public CsvActionResult(IEnumerable modelListing)
        {
            ModelListing = modelListing;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            byte[] data = new CsvFileBuilder().AsBytes(ModelListing);

            var fileResult = new FileContentResult(data, "text/csv")
            {
                FileDownloadName = "CsvFile.csv"
            };

            fileResult.ExecuteResult(context);
        }
    }
}
