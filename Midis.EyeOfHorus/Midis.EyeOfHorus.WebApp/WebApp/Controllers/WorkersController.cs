using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Midis.EyeOfHorus.WebApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
        public async Task<IActionResult> Create(WorkersViewModel wvm)
        {
            Workers worker = new Workers { FullName = wvm.FullName, ImageName = wvm.Avatar.FileName };
            if (wvm.Avatar != null)
            {
                byte[] imageData = null;
                using (var binaryReader = new BinaryReader(wvm.Avatar.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)wvm.Avatar.Length);
                }
                worker.Avatar = imageData;
            }
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
        public async Task<IActionResult> Edit(WorkersViewModel wvm)
        {
            Workers worker = await db.Workers.FirstOrDefaultAsync(p => p.Id == wvm.Id);
            worker.FullName = wvm.FullName;        
            if (wvm.Avatar != null)
            {
                byte[] imageData = null;
                using (var binaryReader = new BinaryReader(wvm.Avatar.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)wvm.Avatar.Length);
                }
                worker.Avatar = imageData;
                worker.ImageName = wvm.Avatar.FileName;
            }
            db.Workers.Update(worker);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                Workers worker = await db.Workers.FirstOrDefaultAsync(p => p.Id == id);
                if (worker != null)
                {
                    db.Workers.Remove(worker);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return NotFound();
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                Workers worker = await db.Workers.FirstOrDefaultAsync(p => p.Id == id);
                if (worker != null)
                    return View(worker);
            }
            return NotFound();
        }
    }
}