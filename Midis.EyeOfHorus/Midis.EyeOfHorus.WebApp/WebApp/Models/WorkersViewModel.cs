using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midis.EyeOfHorus.WebApp.Models
{
    public class WorkersViewModel
    {
        public int Id { get; set; }       
        public string FullName { get; set; }
        public string ImageName { get; set; }
        public string ClientID { get; set; }
        public IFormFile Avatar { get; set; }
    }
}
