using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midis.EyeOfHorus.WebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Remote("DoesUserExist", "Users", AdditionalFields = "previousUserName", ErrorMessage = "User already exist!")]
        public override string UserName { get; set; }
        public string ClientID { get; set; }
        public string CompanyName { get; set; }
    }
}