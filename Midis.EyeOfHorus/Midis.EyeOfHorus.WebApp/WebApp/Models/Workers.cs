using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class Workers
    {
        public int Id { get; set; }
        [Remote("DoesWorkerExist", "Workers", AdditionalFields = "previousFullName", ErrorMessage = "Worker already exist!")]
        public string FullName { get; set; }
        public string ImageName { get; set; }
        public byte[] Avatar { get; set; }
    }
}
