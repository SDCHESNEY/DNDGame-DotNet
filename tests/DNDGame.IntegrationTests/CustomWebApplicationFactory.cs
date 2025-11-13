using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using DNDGame.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DNDGame.IntegrationTests;

/// <summary>
/// Custom factory for creating test server with mocked LLM provider.
/// Uses in-memory database for isolation.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<ILlmProvider> MockLlmProvider { get; } = new();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all DbContext-related registrations to avoid provider conflicts
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DndGameContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbContextOptionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions));
            if (dbContextOptionsDescriptor != null)
            {
                services.Remove(dbContextOptionsDescriptor);
            }

            // Remove the DbContext registration itself
            var contextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DndGameContext));
            if (contextDescriptor != null)
            {
                services.Remove(contextDescriptor);
            }

            // Add in-memory database with unique name per test
            services.AddDbContext<DndGameContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
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
