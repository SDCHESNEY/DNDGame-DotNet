using Microsoft.EntityFrameworkCore;
using DNDGame.Infrastructure.Data;
using DNDGame.Infrastructure.Repositories;
using DNDGame.Core.Interfaces;
using DNDGame.Application.Services;
using DNDGame.Application.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DndGameContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

// Register application services
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<ISessionService, SessionService>();

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
