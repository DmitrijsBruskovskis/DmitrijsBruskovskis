using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midis.EyeOfHorus.WebApp.Models
{
    public class UsersFilterViewModel
    {
        public UsersFilterViewModel(string filteredName, string filteredClientID, string filteredEmail)
        {
            FilteredName = filteredName;
            FilteredClientID = filteredClientID;
            FilteredEmail = filteredEmail;
        }
        public string FilteredName { get; set; }
        public string FilteredClientID { get; set; }
        public string FilteredEmail { get; set; }
    }
}
