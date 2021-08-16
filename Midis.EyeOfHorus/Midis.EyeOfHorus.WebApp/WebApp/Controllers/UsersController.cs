using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Midis.EyeOfHorus.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Data;

namespace Midis.EyeOfHorus.WebApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext db;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            db = context;
        }
        public async Task<IActionResult> Index(int page = 1, string filteredName = null)
        {
            int pageSize = 10;

            IQueryable<ApplicationUser> source = db.Users;
            if (!String.IsNullOrEmpty(filteredName))
            {
                source = source.Where(p => p.UserName.Contains(filteredName));
            }

            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            UsersIndexViewModel viewModel = new UsersIndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                UsersFilterViewModel = new UsersFilterViewModel(filteredName),
                Users = items
            };
            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel uvm)
        {
            ApplicationUser user = new ApplicationUser 
            {
                UserName = uvm.UserName,
                Email = uvm.Email,
                EmailConfirmed = true,
                AccessFailedCount = 0,
                LockoutEnabled = false,
            };
            var result = await _userManager.CreateAsync(user, uvm.Password);
            //await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult DoesUserExist(string userName, string previousUserName)
        {
            if (userName == previousUserName)
            {
                return Json(true);
            }
            List<ApplicationUser> users = db.Users.Where(x => x.UserName == userName).ToList();
            if (users.Count > 0)
                return Json(false);
            return Json(true);
        }
    }
}
