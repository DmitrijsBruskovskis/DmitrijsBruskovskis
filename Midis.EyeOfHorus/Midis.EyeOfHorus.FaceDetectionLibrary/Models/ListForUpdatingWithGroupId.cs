using System;
using System.Collections.Generic;
using System.Text;

namespace Midis.EyeOfHorus.FaceDetectionLibrary.Models
{
    public class ListForUpdatingWithGroupId
    {
        public List<List<WorkersForProcessing>> ListOfWorkerGroupsForUpdating { get; set; }
        public List<List<WorkersForProcessing>> UpdateSource { get; set; }
        public List<string> ListOfGroupId { get; set; }
    }
}
