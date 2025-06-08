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
    private const int MaxRetries = 10;
    private const int RetryDelaySeconds = 5;

    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        Console.WriteLine($"Attempting to connect to database with retry logic...");
        
        // 重試邏輯
        var retryCount = 0;
        while (retryCount < MaxRetries)
        {
            try
            {
                await RunMigrations(connectionString, args);
                Console.WriteLine("Migration completed successfully!");
                return;
            }
            catch (Exception ex) when (retryCount < MaxRetries - 1)
            {
                retryCount++;
                Console.WriteLine($"Migration attempt {retryCount} failed: {ex.Message}");
                Console.WriteLine($"Retrying in {RetryDelaySeconds} seconds... ({retryCount}/{MaxRetries})");
                await Task.Delay(TimeSpan.FromSeconds(RetryDelaySeconds));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration failed after {MaxRetries} attempts: {ex.Message}");
                throw;
            }
        }
    }

    private static async Task RunMigrations(string connectionString, string[] args)
    {
        // 測試連接
        using var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
        await connection.OpenAsync();
        await connection.CloseAsync();
        
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
