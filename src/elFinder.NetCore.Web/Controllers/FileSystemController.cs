using System;
using System.Threading.Tasks;
using elFinder.NetCore.Drivers.FileSystem;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Web.Controllers
{
    [Route("file-system")]
    public class FileSystemController : Controller
    {
        [Route("/connector")]
        public async Task<IActionResult> Connector(string folder, string subFolder)
        {
            var connector = GetConnector(folder, subFolder);
            return await connector.ProcessAsync(Request);
        }

        [Route("thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector();
            return await connector.GetThumbnailAsync(HttpContext.Request, HttpContext.Response, hash);
        }

        private Connector GetConnector(string folder = "Files", string subFolder = "")
        {
            var driver = new FileSystemDriver();
            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);

            var root = new RootVolume(
                Startup.MapPath("~/" + folder),
                $"{uri.Scheme}://{uri.Authority}/" + folder + "/",
                $"{uri.Scheme}://{uri.Authority}/file-system/thumb/")
            {
                //IsReadOnly = !User.IsInRole("Administrators")
                IsReadOnly = false, // Can be readonly according to user's membership permission
                IsLocked = false, // If locked, files and directories cannot be deleted, renamed or moved
                Alias = "Root", // Beautiful name given to the root/home folder
                MaxUploadSizeInKb = 5120, // Limit imposed to user uploaded file <= 2048 KB
                //LockedFolders = new List<string>(new string[] { "Folder1" })
            };

            if (!string.IsNullOrEmpty(subFolder))
            {
                //root.StartDirectory = Startup.MapPath("~/" + folder + "/" + subFolder);
            }

            driver.AddRoot(root);

            return new Connector(driver)
            {
                // This allows support for the "onlyMimes" option on the client.
                MimeDetect = MimeDetectOption.Internal
            };
        }
    }
}