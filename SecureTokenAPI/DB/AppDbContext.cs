using Microsoft.EntityFrameworkCore;
using SecureTokenAPI.Models;

namespace SecureTokenAPI.DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<UserToken> UserTokens { get; set; }
    }
}
