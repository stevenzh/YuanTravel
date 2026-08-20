using elFinder.NetCore.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Web.Controllers
{
    [Route("file-manager")]
    //[Authorize]
    public class FileManagerController : Controller
    {
        public IActionResult Index()
        {
            FileViewModel model = new FileViewModel { Folder="Files", SubFolder="" };
            return View(model);
        }
    }
}