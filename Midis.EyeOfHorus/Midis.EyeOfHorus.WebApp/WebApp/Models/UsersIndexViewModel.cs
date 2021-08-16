using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midis.EyeOfHorus.WebApp.Models
{
    public class UsersIndexViewModel
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public UsersFilterViewModel UsersFilterViewModel { get; set; }
    }
}
