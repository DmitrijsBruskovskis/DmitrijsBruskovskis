using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using Midis.EyeOfHorus.WebApp.Data;
using Midis.EyeOfHorus.WebApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Roles = "client,admin")]
    public class ResultsController : Controller
    {
        private readonly PostGreSqlDbContext db;
        public async Task<IActionResult> Index(string filteredName = null, int? filteredCameraId = null, string filteredDateTime = null, int page = 1, ResultsSortState sortOrder = ResultsSortState.CameraIdAsc)
        {
            int pageSize = 20;

            IQueryable<Results> results = db.InfoAboutFaces;
            if (!String.IsNullOrEmpty(filteredName))
            {
                results = results.Where(p => p.Worker.Contains(filteredName));
            }
            if ((filteredCameraId != null))
            {
                results = results.Where(p => p.CameraID.Equals(filteredCameraId));
            }
            if ((filteredDateTime != null))
            {
                results = results.Where(p => p.DateTime.Date == Convert.ToDateTime(filteredDateTime));
            }

            switch (sortOrder)
            {
                case ResultsSortState.CameraIdDesc:
                    results = results.OrderByDescending(s => s.CameraID);
                    break;
                case ResultsSortState.CameraIdAsc:
                    results = results.OrderBy(s => s.CameraID);
                    break;
                case ResultsSortState.WorkerAsc:
                    results = results.OrderBy(s => s.Worker);
                    break;
                case ResultsSortState.WorkerDesc:
                    results = results.OrderByDescending(s => s.Worker);
                    break;
                case ResultsSortState.FileNameAsc:
                    results = results.OrderBy(s => s.FileName);
                    break;
                case ResultsSortState.FileNameDesc:
                    results = results.OrderByDescending(s => s.FileName);
                    break;
                case ResultsSortState.DateTimeAsc:
                    results = results.OrderBy(s => s.DateTime);
                    break;
                case ResultsSortState.DateTimeDesc:
                    results = results.OrderByDescending(s => s.DateTime);
                    break;
                default:
                    results = results.OrderBy(s => s.CameraID);
                    break;
            }

            var count = await results.CountAsync();
            var items = await results.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();


            ResultsIndexViewModel viewModel = new ResultsIndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                ResultsSortViewModel = new ResultsSortViewModel(sortOrder),
                ResultsFilterViewModel = new ResultsFilterViewModel(filteredName, filteredCameraId, filteredDateTime),
                Results = items
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                Results result = await db.InfoAboutFaces.FirstOrDefaultAsync(p => p.Id == id);
                if (result != null)
                    return View(result);
            }
            return NotFound();
        }
        public ResultsController(PostGreSqlDbContext context)
        {
            db = context;
        }
    }
}
