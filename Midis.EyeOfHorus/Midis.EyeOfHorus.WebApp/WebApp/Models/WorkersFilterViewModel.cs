using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midis.EyeOfHorus.WebApp.Models
{
    public class WorkersFilterViewModel
    {
        public WorkersFilterViewModel(string filteredName)
        {
            FilteredName = filteredName;
        }
        public string FilteredName { get; set; }
    }
}
