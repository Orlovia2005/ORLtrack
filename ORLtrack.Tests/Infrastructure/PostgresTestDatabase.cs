using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using plt.Models.Model;

namespace ORLtrack.Tests.Infrastructure;

internal sealed class PostgresTestDatabase : IAsyncDisposable
{
    private readonly string _adminConnectionString;
    private readonly string _databaseName;

    private PostgresTestDatabase(string adminConnectionString, string databaseName, string connectionString)
    {
        _adminConnectionString = adminConnectionString;
        _databaseName = databaseName;
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public static async Task<PostgresTestDatabase> CreateAsync()
    {
        var baseConnectionString = ResolveBaseConnectionString();
        var databaseName = $"orltrack_tests_{Guid.NewGuid():N}";

        await using (var connection = new NpgsqlConnection(baseConnectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = databaseName
        };

        var database = new PostgresTestDatabase(baseConnectionString, databaseName, builder.ConnectionString);
        await database.ApplyMigrationsAsync();
        return database;
    }

    public EducationDbContext CreateDbContext(int? authenticatedUserId = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString
            })
            .Build();

        var httpContext = new DefaultHttpContext
        {
            User = BuildPrincipal(authenticatedUserId)
        };

        var options = new DbContextOptionsBuilder<EducationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new EducationDbContext(options, new HttpContextAccessor { HttpContext = httpContext }, configuration);
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();

        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();

        await using (var terminateCommand = connection.CreateCommand())
        {
            terminateCommand.CommandText =
                $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{_databaseName}' AND pid <> pg_backend_pid();";
            await terminateCommand.ExecuteNonQueryAsync();
        }

        await using (var dropCommand = connection.CreateCommand())
        {
            dropCommand.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\"";
            await dropCommand.ExecuteNonQueryAsync();
        }
    }

    private async Task ApplyMigrationsAsync()
    {
        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    private static string ResolveBaseConnectionString()
    {
        var overrideConnection = Environment.GetEnvironmentVariable("ORLTRACK_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(overrideConnection))
        {
            return overrideConnection;
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(SolutionPaths.CombineFromRoot("plt", "appsettings.json"))
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Не удалось получить строку подключения для тестов.");
        }

        return connectionString;
    }

    private static ClaimsPrincipal BuildPrincipal(int? userId)
    {
        if (!userId.HasValue)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
            new Claim(ClaimTypes.Name, $"Teacher {userId.Value}"),
            new Claim("Id", userId.Value.ToString())
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }
}
