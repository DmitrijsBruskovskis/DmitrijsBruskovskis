using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midis.EyeOfHorus.WebApp.Models
{
    public class ResultsSortViewModel
    {
        public ResultsSortState WorkerSort { get; private set; }
        public ResultsSortState CameraIdSort { get; private set; }
        public ResultsSortState FileNameSort { get; private set; }
        public ResultsSortState DateTimeSort { get; private set; }
        public ResultsSortState Current { get; private set; }

        public ResultsSortViewModel(ResultsSortState sortOrder)
        {
            WorkerSort = sortOrder == ResultsSortState.WorkerAsc ? ResultsSortState.WorkerDesc : ResultsSortState.WorkerAsc;
            CameraIdSort = sortOrder == ResultsSortState.CameraIdAsc ? ResultsSortState.CameraIdDesc : ResultsSortState.CameraIdAsc;
            FileNameSort = sortOrder == ResultsSortState.FileNameAsc ? ResultsSortState.FileNameDesc : ResultsSortState.FileNameAsc;
            DateTimeSort = sortOrder == ResultsSortState.DateTimeAsc ? ResultsSortState.DateTimeDesc : ResultsSortState.DateTimeAsc;
            Current = sortOrder;
        }
    }
}
