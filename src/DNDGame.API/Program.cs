using Microsoft.EntityFrameworkCore;
using DNDGame.Infrastructure.Data;
using DNDGame.Infrastructure.Repositories;
using DNDGame.Infrastructure.Services;
using DNDGame.Core.Interfaces;
using DNDGame.Application.Services;
using DNDGame.Application.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DndGameContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

// Register repositories
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

// Register application services
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddSingleton<IDiceRoller, DiceRollerService>();
builder.Services.AddScoped<IRulesEngine, RulesEngineService>();
builder.Services.AddScoped<ICombatService, CombatService>();

// Register LLM services
builder.Services.Configure<LlmSettings>(
    builder.Configuration.GetSection(LlmSettings.SectionName));
builder.Services.Configure<ContentModerationSettings>(
    builder.Configuration.GetSection(ContentModerationSettings.SectionName));

builder.Services.AddSingleton<ILlmProvider, OpenAiProvider>();
builder.Services.AddSingleton<IPromptTemplateService, PromptTemplateService>();
builder.Services.AddSingleton<IContentModerationService, ContentModerationService>();
builder.Services.AddScoped<ILlmDmService, LlmDmService>();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateCharacterRequestValidator>();

// Add controllers with Problem Details
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
