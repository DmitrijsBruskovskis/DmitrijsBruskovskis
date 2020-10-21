using Microsoft.EntityFrameworkCore;
using Midis.EyeOfHorus.FaceDetectionLibrary.Models;

namespace Midis.EyeOfHorus.FaceDetectionLibrary
{
    public class ApplicationContext : DbContext
    {
        public DbSet<DatabaseInfoAboutImage> InfoAboutImages { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=admin");
        }
    }
}
