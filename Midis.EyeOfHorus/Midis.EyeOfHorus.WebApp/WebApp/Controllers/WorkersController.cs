using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class WorkersController : Controller
    {
        private readonly ApplicationDbContext db;
        public async Task<IActionResult> Index()
        {
            return View(await db.Workers.ToListAsync());
        }

        public WorkersController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Workers worker)
        {
            db.Workers.Add(worker);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                Workers worker = await db.Workers.FirstOrDefaultAsync(p => p.Id == id);
                if (worker != null)
                    return View(worker);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Workers worker)
        {
            db.Workers.Update(worker);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                Workers worker = new Workers { Id = id.Value };
                db.Entry(worker).State = EntityState.Deleted;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return NotFound();
        }
    }
}