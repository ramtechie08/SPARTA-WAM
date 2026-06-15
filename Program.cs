using Microsoft.EntityFrameworkCore;
using FluentValidation;
using SPARTA_WAM.Models;
using SPARTA_WAM.Services;
using SPARTA_WAM.Validators;
using SPARTA_WAM.Data;
using SPARTA_WAM.Data.Connections;
using SPARTA_WAM.Data.Repositories;
using SPARTA_WAM.Data.Resilience;
using SPARTA_WAM.Data.Services;

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
builder.Services.AddScoped<ILaoFreezeRepository, LaoFreezeRepository>();
builder.Services.AddScoped<ILaoFreezeDatabaseService, LaoFreezeDatabaseService>();

// Register Business Services (WAM)
builder.Services.AddScoped<IExcelFileProcessor, ExcelFileProcessor>();
builder.Services.AddScoped<IValidator<WalkAwayMarginRequest>, WalkAwayMarginValidator>();
builder.Services.AddScoped<IInputValidationService, InputValidationService>();
builder.Services.AddScoped<ISqlScriptGenerator, SqlScriptGenerator>();
builder.Services.AddScoped<IWamProcessingService, WamProcessingService>();

// Register Business Services (LAO Freeze)
builder.Services.AddScoped<ILaoFreezeExcelProcessor, LaoFreezeExcelProcessor>();
builder.Services.AddScoped<IValidator<LaoFreezeRequest>, LaoFreezeValidator>();
builder.Services.AddScoped<IValidator<LaoProductFreezeRequest>, LaoProductFreezeValidator>();
builder.Services.AddScoped<ILaoFreezeInputValidationService, LaoFreezeInputValidationService>();
builder.Services.AddScoped<ILaoFreezeSqlScriptGenerator, LaoFreezeSqlScriptGenerator>();
builder.Services.AddScoped<ILaoFreezeProcessingService, LaoFreezeProcessingService>();

// Add Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

// Log environment information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
//logger.LogInformation($"Application starting in {environment} environment");
//logger.LogInformation($"Database configuration: {configProvider.GetDatabaseConfiguration().Environment}");

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<SpartaDbContext>();
        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

logger.LogInformation("SPARTA WAM application started successfully");
await app.RunAsync();
