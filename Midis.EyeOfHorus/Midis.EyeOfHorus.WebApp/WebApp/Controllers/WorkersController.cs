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
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Controllers
{
    [Authorize(Roles = "client,admin")]
    public class WorkersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        public async Task<IActionResult> Index(int page = 1, string filteredName = null)
        {
            int pageSize = 10;

            IQueryable<Workers> source = db.Workers;
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (currentUser.ClientID != "admin")
                source = source.Where(p => p.ClientID.Equals(currentUser.ClientID));

            if (!String.IsNullOrEmpty(filteredName))
            {
                source = source.Where(p => p.FullName.Contains(filteredName));
            }

            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            WorkersIndexViewModel viewModel = new WorkersIndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                WorkersFilterViewModel = new WorkersFilterViewModel(filteredName),
                Workers = items
            };
            return View(viewModel);
        }

        public WorkersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkersViewModel wvm)
        {
            Workers worker = new Workers();
            if (wvm.ClientID != null)
            {
                worker = new Workers { FullName = wvm.FullName, ImageName = wvm.Avatar.FileName, ClientID = wvm.ClientID };
            }
            else
            {
                var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
                worker = new Workers { FullName = wvm.FullName, ImageName = wvm.Avatar.FileName, ClientID = currentUser.ClientID };
            }
           
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
            TempData["returnurl"] = Request.Headers["Referer"].ToString();
            if (id != null)
            {
                var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
                Workers worker = await db.Workers.FirstOrDefaultAsync(p => p.Id == id);
                if (currentUser.ClientID != "admin")
                {
                    if (currentUser.ClientID == worker.ClientID)
                        if (worker != null)
                            return View(worker);
                }
                else
                {
                    if (worker != null)
                        return View(worker);
                }
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(WorkersViewModel wvm)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            Workers worker = await db.Workers.FirstOrDefaultAsync(p => p.Id == wvm.Id);
            if (currentUser.ClientID != "admin")
            {
                if (currentUser.ClientID == worker.ClientID)
                {
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
                    return Redirect(TempData["returnurl"].ToString());
                }
            }
            else
            {
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
                return Redirect(TempData["returnurl"].ToString());
            }
            return NotFound();
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id != null)
            {
                var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
                Workers worker = await db.Workers.FirstOrDefaultAsync(p => p.Id == id);
                if (currentUser.ClientID != "admin")
                {
                   if (currentUser.ClientID == worker.ClientID)
                        if (worker != null)
                            return View(worker);
                }
                else
                {
                    if (worker != null)
                        return View(worker);
                }
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
                Workers worker = await db.Workers.FirstOrDefaultAsync(p => p.Id == id);
                if (currentUser.ClientID != "admin")
                {
                    if (currentUser.ClientID == worker.ClientID)
                        if (worker != null)
                        {
                            db.Workers.Remove(worker);
                            await db.SaveChangesAsync();
                            return RedirectToAction("Index");
                        }
                }
                else
                {
                    if (worker != null)
                    {
                        db.Workers.Remove(worker);
                        await db.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                }
            }
            return NotFound();
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
                Workers worker = await db.Workers.FirstOrDefaultAsync(p => p.Id == id);
                if (currentUser.ClientID != "admin")
                {
                    if (currentUser.ClientID == worker.ClientID)
                        if (worker != null)
                            return View(worker);
                }
                else
                {
                    if (worker != null)
                        return View(worker);
                }
            }
            return NotFound();
        }

        //public async Task<IActionResult> SendDataToServer()
        //{
        //    var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
        //    IQueryable<Workers> source = db.Workers;
        //    var data = source.Where(p => p.ClientID.Equals(currentUser.ClientID));
        //}

        [AcceptVerbs("Get", "Post")]
        public IActionResult DoesWorkerExist(string fullName, string previousFullName)
        {
            if (fullName == previousFullName)
            {
                return Json(true);
            }
            List<Workers> workers = db.Workers.Where(x => x.FullName == fullName).ToList();
            if (workers.Count > 0)
                return Json(false);
            return Json(true);
        }
    }
}