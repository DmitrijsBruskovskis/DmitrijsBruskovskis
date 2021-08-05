using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace Midis.EyeOfHorus.WebApp.Data
{
    public class PostGreSqlDbContext : DbContext
    {
        public PostGreSqlDbContext(DbContextOptions options)
        : base(options) {}

        public DbSet<Results> InfoAboutFaces { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=admin");
    }
}
