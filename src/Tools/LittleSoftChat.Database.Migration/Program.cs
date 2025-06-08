using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LittleSoftChat.Database.Migration;

[VersionTableMetaData]
public class CustomVersionTableMetaData : IVersionTableMetaData
{
    public object ApplicationContext { get; set; } = null!;
    public bool OwnsSchema => false;
    public string SchemaName => "";
    public string TableName => "version_info";
    public string ColumnName => "version";
    public string UniqueIndexName => "uk_version_info_version";
    public string AppliedOnColumnName => "applied_on";
    public string DescriptionColumnName => "description";
}

class Program
{
    static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var serviceProvider = CreateServices(connectionString);
        using var scope = serviceProvider.CreateScope();
        
        var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        if (args.Length > 0 && args[0] == "down")
        {
            Console.WriteLine("Rolling back migrations...");
            migrationRunner.RollbackToVersion(0);
        }
        else
        {
            Console.WriteLine("Running migrations...");
            migrationRunner.MigrateUp();
        }

        Console.WriteLine("Migration completed!");
    }

    private static ServiceProvider CreateServices(string connectionString)
    {
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddMySql8()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(Program).Assembly).For.Migrations()
                .ScanIn(typeof(Program).Assembly).For.VersionTableMetaData())
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .AddSingleton<ILoggerFactory, LoggerFactory>()
            .BuildServiceProvider(false);
    }
}
