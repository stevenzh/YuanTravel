using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lvy.NetCore.API.Models
{
    public class Select2Model
    {

        public string incomplete_results { get; set; }
        public IEnumerable items { get; set; }

        public int total_count { get; set; }
    }
}
