using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace elFinder.NetCore.Web.Models
{
    public class UploadFileResponse
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int RetCode { get; set; }
    }
}
