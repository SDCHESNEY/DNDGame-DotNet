using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using DNDGame.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DNDGame.IntegrationTests;

/// <summary>
/// Custom factory for creating test server with mocked LLM provider.
/// Uses SQLite in-memory database for isolation.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<ILlmProvider> MockLlmProvider { get; } = new();
    private SqliteConnection? _connection;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DndGameContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Create in-memory SQLite connection that stays open
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Add DbContext with SQLite in-memory database
            services.AddDbContext<DndGameContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Replace ILlmProvider with mock for predictable responses
            var llmDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ILlmProvider));
            if (llmDescriptor != null)
            {
                services.Remove(llmDescriptor);
            }
            services.AddSingleton(MockLlmProvider.Object);

            // Build service provider and create database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<DndGameContext>();

            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }

    public void SetupMockLlmResponse(string response, int tokensUsed = 50)
    {
        MockLlmProvider
            .Setup(p => p.CompleteAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((response, tokensUsed));
    }

    public void SetupMockLlmStream(params string[] chunks)
    {
        MockLlmProvider
            .Setup(p => p.StreamCompleteAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(chunks));
    }

    private static async IAsyncEnumerable<string> ToAsyncEnumerable(IEnumerable<string> items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }
}
