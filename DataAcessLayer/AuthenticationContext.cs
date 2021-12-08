using AuthenticationAPI.Domain;
using Microsoft.EntityFrameworkCore;
namespace AuthenticationAPI.DataAcessLayer
{
    public class AuthenticationContext : DbContext
    {
        public AuthenticationContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
        }
        public DbSet<Register> registers { get; set; }
        public DbSet<Logs> logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new RegisterMapper(modelBuilder.Entity<Register>());
        }
    }
}