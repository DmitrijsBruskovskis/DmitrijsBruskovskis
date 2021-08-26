using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext db;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            db = context;
        }
        public async Task<IActionResult> Index(int page = 1, string filteredName = null, string filteredClientID = null, string filteredEmail = null)
        {
            int pageSize = 10;

            IQueryable<ApplicationUser> source = db.Users;
            if (!String.IsNullOrEmpty(filteredName))
            {
                source = source.Where(p => p.UserName.Contains(filteredName));
            }
            if (!String.IsNullOrEmpty(filteredClientID))
            {
                source = source.Where(p => p.ClientID.Contains(filteredClientID));
            }
            if (!String.IsNullOrEmpty(filteredEmail))
            {
                source = source.Where(p => p.Email.Contains(filteredEmail));
            }

            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            UsersIndexViewModel viewModel = new UsersIndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                UsersFilterViewModel = new UsersFilterViewModel(filteredName, filteredClientID, filteredEmail),
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
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser 
                {
                    UserName = uvm.UserName,
                    Email = uvm.Email,
                    ClientID = uvm.ClientID,
                    PhoneNumber = uvm.PhoneNumber,
                    EmailConfirmed = true,
                    AccessFailedCount = 0,
                    LockoutEnabled = false,
                };
                await _userManager.CreateAsync(user, uvm.Password);
                await _userManager.AddToRoleAsync(user, "client");
                return RedirectToAction("Index");
            }
            else
            {
                return View(uvm);
            }
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


        [AcceptVerbs("Get", "Post")]
        public IActionResult DoesEmailAlreadyUsed(string email, string previousEmail)
        {
            if (email == previousEmail)
            {
                return Json(true);
            }
            List<ApplicationUser> users = db.Users.Where(x => x.Email == email).ToList();
            if (users.Count > 0)
                return Json(false);
            return Json(true);
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            if (id != null)
            {
                ApplicationUser user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);

                UserViewModel userViewModel = new UserViewModel();
                userViewModel.Id = user.Id;
                userViewModel.AccessFailedCount = user.AccessFailedCount;
                userViewModel.LockoutEnabled = user.LockoutEnabled;
                userViewModel.LockoutEnd = user.LockoutEnd;
                userViewModel.NormalizedEmail = user.NormalizedEmail;
                userViewModel.NormalizedUserName = user.NormalizedUserName;
                userViewModel.PhoneNumber = user.PhoneNumber;
                userViewModel.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                userViewModel.SecurityStamp = user.SecurityStamp;
                userViewModel.TwoFactorEnabled = user.TwoFactorEnabled;
                userViewModel.UserName = user.UserName;
                userViewModel.ClientID = user.ClientID;
                userViewModel.CompanyName = user.CompanyName;
                userViewModel.ConcurrencyStamp = user.ConcurrencyStamp;
                userViewModel.Email = user.Email;
                userViewModel.EmailConfirmed = user.EmailConfirmed;

                if (userViewModel != null)
                    return View(userViewModel);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (id != null)
            {
                ApplicationUser user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                {
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return NotFound();
        }

        public async Task<IActionResult> Edit(string id)
        {
            TempData["returnurl"] = Request.Headers["Referer"].ToString();
            if (id != null)
            {
                ApplicationUser user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);

                UserViewModel userViewModel = new UserViewModel();
                userViewModel.Id = user.Id;
                userViewModel.AccessFailedCount = user.AccessFailedCount;
                userViewModel.LockoutEnabled = user.LockoutEnabled;
                userViewModel.LockoutEnd = user.LockoutEnd;
                userViewModel.NormalizedEmail = user.NormalizedEmail;
                userViewModel.NormalizedUserName = user.NormalizedUserName;
                userViewModel.PhoneNumber = user.PhoneNumber;
                userViewModel.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                userViewModel.SecurityStamp = user.SecurityStamp;
                userViewModel.TwoFactorEnabled = user.TwoFactorEnabled;
                userViewModel.UserName = user.UserName;
                userViewModel.ClientID = user.ClientID;
                userViewModel.CompanyName = user.CompanyName;
                userViewModel.ConcurrencyStamp = user.ConcurrencyStamp;
                userViewModel.Email = user.Email;
                userViewModel.EmailConfirmed = user.EmailConfirmed;

                if (userViewModel != null)
                    return View(userViewModel);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel uvm)
        {
            ApplicationUser user = await db.Users.FirstOrDefaultAsync(p => p.Id == uvm.Id);
            user.UserName = uvm.UserName;
            user.Email = uvm.Email;
            user.ClientID = uvm.ClientID;
            user.PhoneNumber = uvm.PhoneNumber;
 
            await db.SaveChangesAsync();
            return Redirect(TempData["returnurl"].ToString());
        }

        public async Task<IActionResult> Details(string id)
        {
            TempData["returnurl"] = Request.Headers["Referer"].ToString();
            if (id != null)
            {
                ApplicationUser user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);

                UserViewModel userViewModel = new UserViewModel();
                userViewModel.Id = user.Id;
                userViewModel.AccessFailedCount = user.AccessFailedCount;
                userViewModel.LockoutEnabled = user.LockoutEnabled;
                userViewModel.LockoutEnd = user.LockoutEnd;
                userViewModel.NormalizedEmail = user.NormalizedEmail;
                userViewModel.NormalizedUserName = user.NormalizedUserName;
                userViewModel.PhoneNumber = user.PhoneNumber;
                userViewModel.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                userViewModel.SecurityStamp = user.SecurityStamp;
                userViewModel.TwoFactorEnabled = user.TwoFactorEnabled;
                userViewModel.UserName = user.UserName;
                userViewModel.ClientID = user.ClientID;
                userViewModel.CompanyName = user.CompanyName;
                userViewModel.ConcurrencyStamp = user.ConcurrencyStamp;
                userViewModel.Email = user.Email;
                userViewModel.EmailConfirmed = user.EmailConfirmed;

                if (userViewModel != null)
                    return View(userViewModel);
            }
            return NotFound();
        }
    }
}
