using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Data.Interfaces;

namespace WebApp.Controllers
{
    public class WorkerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}