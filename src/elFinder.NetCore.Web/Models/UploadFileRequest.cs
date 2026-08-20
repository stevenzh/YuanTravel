using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace elFinder.NetCore.Web.Models
{
    public class UploadFileRequest
    {
        public string FileName { get; set; }
        public byte[] FileStream { get; set; }
        public string VirtualPath { get; set; }
    }
}
