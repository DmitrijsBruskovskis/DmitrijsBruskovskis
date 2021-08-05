using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace Midis.EyeOfHorus.WebApp.Models
{
    public class ResultsIndexViewModel
    {
        public IEnumerable<Results> Results { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public string FilteredName { get; set; }
        public string FilteredCameraID { get; set; }
    }
}
