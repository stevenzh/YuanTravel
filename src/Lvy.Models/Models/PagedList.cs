using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lvy.Models
{
    public class PagedList<T> 
    {

        public PagedList()
        {
            PageSize = 10;
            PageIndex = 1;
        }

        public long PageIndex { get; set; }
        public long PageCount { get; set; }
        public long TotalCount { get; set; }
        public long PageSize { get; set; }
        public List<T> Items { get; set; }
        public object Context { get; set; }
    }
}
