using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midis.EyeOfHorus.WebApp.Models
{
    public class ResultsFilterViewModel
    {
        public ResultsFilterViewModel(string filteredName, int? filteredCameraId, string filteredDateTime)
        {
            FilteredName = filteredName;
            FilteredCameraId = filteredCameraId;
            FilteredDateTime = filteredDateTime;
        }
        public string FilteredName { get; set; }
        public int? FilteredCameraId { get; set; }
        public string FilteredDateTime { get; set; }
    }
}
