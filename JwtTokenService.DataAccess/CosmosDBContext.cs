using JwtTokenService.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtTokenService.DataAccess
{
    public class CosmosDBContext : DbContext
    {
        public CosmosDBContext(DbContextOptions<CosmosDBContext> options): base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<User>(p =>
            {
                p.ToContainer("User");
                p.HasPartitionKey(x => x.Username);
                p.OwnsMany(o => o.Logins);
            });
        }
    }
}
