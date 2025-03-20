using Microsoft.EntityFrameworkCore;


namespace Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Credential> Credentials => Set<Credential>();
    }
}
