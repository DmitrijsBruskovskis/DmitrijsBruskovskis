using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace WebApp.Controllers
{
    public class ResultsController : Controller
    {
        private readonly ApplicationDbContext db;
        public async Task<IActionResult> Index()
        {
            return View(await db.Results.ToListAsync());
        }

        public ResultsController(ApplicationDbContext context)
        {
            db = context;
        }
    }
}
