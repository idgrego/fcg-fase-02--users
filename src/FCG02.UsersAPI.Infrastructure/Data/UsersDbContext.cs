using FCG02.UsersAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG02.UsersAPI.Infrastructure.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // aplica todas as configurações de fluent API deste assembly automaticamente
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
    }
}