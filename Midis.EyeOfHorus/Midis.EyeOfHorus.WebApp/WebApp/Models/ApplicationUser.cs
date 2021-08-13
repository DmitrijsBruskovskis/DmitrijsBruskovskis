using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midis.EyeOfHorus.WebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string ClientID { get; set; }
        public string CompanyName { get; set; }
    }
}