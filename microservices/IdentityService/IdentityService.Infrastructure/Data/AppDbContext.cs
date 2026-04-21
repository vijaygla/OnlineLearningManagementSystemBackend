using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}