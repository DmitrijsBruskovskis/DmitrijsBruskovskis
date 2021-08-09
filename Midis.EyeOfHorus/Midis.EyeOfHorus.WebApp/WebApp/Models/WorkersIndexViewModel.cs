using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace Midis.EyeOfHorus.WebApp.Models
{
    public class WorkersIndexViewModel
    {
        public IEnumerable<Workers> Workers { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public WorkersFilterViewModel WorkersFilterViewModel { get; set; }
    }
}