using Microsoft.EntityFrameworkCore;
using Midis.EyeOfHorus.FaceDetectionLibrary.Models;

namespace Midis.EyeOfHorus.FaceDetectionLibrary
{
    public class ApplicationContext : DbContext
    {
        public DbSet<DatabaseInfoAboutFace> InfoAboutFaces { get; set; }
        public DbSet<DatabaseInfoAboutWorker> WorkersInAPI { get; set; }
        public DbSet<Client> Clients { get; set; }


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
