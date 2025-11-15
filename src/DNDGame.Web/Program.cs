using DNDGame.Web.Components;
using Microsoft.EntityFrameworkCore;
using DNDGame.Infrastructure.Data;
using DNDGame.Infrastructure.Repositories;
using DNDGame.Infrastructure.Services;
using DNDGame.Core.Interfaces;
using DNDGame.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext
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
builder.Services.AddSingleton<IPresenceService, PresenceService>();

// Add memory cache for presence service
builder.Services.AddMemoryCache();

// Register LLM services
builder.Services.Configure<LlmSettings>(
    builder.Configuration.GetSection(LlmSettings.SectionName));
builder.Services.Configure<ContentModerationSettings>(
    builder.Configuration.GetSection(ContentModerationSettings.SectionName));

builder.Services.AddSingleton<ILlmProvider, OpenAiProvider>();
builder.Services.AddSingleton<IPromptTemplateService, PromptTemplateService>();
builder.Services.AddSingleton<IContentModerationService, ContentModerationService>();
builder.Services.AddScoped<ILlmDmService, LlmDmService>();

// Add HttpClient for API calls (if needed)
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
