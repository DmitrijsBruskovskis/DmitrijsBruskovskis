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

namespace WebApp.Controllers
{
    public class ResultsController : Controller
    {
        private readonly PostGreSqlDbContext db;
        public async Task<IActionResult> Index(int page = 1, string filteredName = null, int? filteredCameraId = null)
        {
            int pageSize = 20;

            IQueryable<Results> source = db.InfoAboutFaces;
            if (!String.IsNullOrEmpty(filteredName))
            {
                source = source.Where(p => p.Worker.Contains(filteredName));
            }
            if ((filteredCameraId != null))
            {
                source = source.Where(p => p.CameraID.Equals(filteredCameraId));
            }

            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);

            ResultsIndexViewModel viewModel = new ResultsIndexViewModel
            {
                PageViewModel = pageViewModel,
                Results = items
            };
            return View(viewModel);
        }
        public ResultsController(PostGreSqlDbContext context)
        {
            db = context;
        }
    }
}
