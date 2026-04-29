using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityService.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS;Initial Catalog=OnlineLearningManagementSystemDB_Identity;Integrated Security=True;Pooling=False;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Name=vscode-mssql;Application Intent=ReadWrite;Command Timeout=30");

        return new AppDbContext(optionsBuilder.Options);
    }
}
