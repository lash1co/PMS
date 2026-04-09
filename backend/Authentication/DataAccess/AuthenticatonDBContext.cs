using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authentication.DataAccess
{
    public class AuthenticatonDBContext : DbContext
    {
        public AuthenticatonDBContext(DbContextOptions<AuthenticatonDBContext> options)
        : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
