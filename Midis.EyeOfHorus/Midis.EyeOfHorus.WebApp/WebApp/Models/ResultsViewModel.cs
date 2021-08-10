using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midis.EyeOfHorus.WebApp.Models
{
    public class ResultsViewModel
    {
        public int Id { get; set; }
        public string Worker { get; set; }
        public IFormFile Image { get; set; }
    }
}
