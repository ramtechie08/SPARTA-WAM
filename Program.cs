using Microsoft.EntityFrameworkCore;
using FluentValidation;
using SPARTA_WAM.Data;
using SPARTA_WAM.Data.Connections;
using SPARTA_WAM.Data.Repositories;
using SPARTA_WAM.Data.Resilience;
using SPARTA_WAM.Data.Services;
using SPARTA_WAM.Models;
using SPARTA_WAM.Services;
using SPARTA_WAM.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration
builder.Services.AddDbContext<SpartaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SpartaDatabase"),
        sqlServerOptions => sqlServerOptions
            .EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

// Register Database Services
builder.Services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IResiliencePolicyProvider, ResiliencePolicyProvider>();
builder.Services.AddScoped<ISqlExecutionService, SqlExecutionService>();
builder.Services.AddScoped<IWalkAwayMarginRepository, WalkAwayMarginRepository>();
builder.Services.AddScoped<IWamDatabaseService, WamDatabaseService>();

// Register Business Services
builder.Services.AddScoped<IExcelFileProcessor, ExcelFileProcessor>();
builder.Services.AddScoped<IValidator<WalkAwayMarginRequest>, WalkAwayMarginValidator>();
builder.Services.AddScoped<IInputValidationService, InputValidationService>();
builder.Services.AddScoped<ISqlScriptGenerator, SqlScriptGenerator>();
builder.Services.AddScoped<IWamProcessingService, WamProcessingService>();

// Add Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SpartaDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
