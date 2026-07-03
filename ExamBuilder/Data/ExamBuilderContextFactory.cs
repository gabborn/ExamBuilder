using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExamBuilder.Data;

public class ExamBuilderContextFactory : IDesignTimeDbContextFactory<ExamBuilderContext>
{
    public ExamBuilderContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION")
                               ?? "Data Source=ExamBuilder.db";
        var optionsBuilder = new DbContextOptionsBuilder<ExamBuilderContext>();
        if (connectionString.Contains("database.windows.net"))
            optionsBuilder.UseAzureSql(connectionString);
        else
            optionsBuilder.UseSqlite(connectionString);
        return new ExamBuilderContext(optionsBuilder.Options);
    }
}
