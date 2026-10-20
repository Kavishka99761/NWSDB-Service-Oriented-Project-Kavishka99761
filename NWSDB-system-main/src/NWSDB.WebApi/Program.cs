using Microsoft.OpenApi.Models;
using NWSDB.Data;
using NWSDB.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Register NWSDB Services & SQLite Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "Data Source=nwsdb_system.db";
builder.Services.AddNwsdbServices(connectionString);

// Add Swagger Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NWSDB Service-Oriented Web API",
        Version = "v1",
        Description = "National Water Supply and Drainage Board (NWSDB) SOC Application Service Interface. Exposes bill payment, meter usage, billing, third-party partner integration, and monitoring services."
    });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Partner API Key header for third-party operations (X-API-KEY)",
        Type = SecuritySchemeType.ApiKey,
        Name = "X-API-KEY",
        In = ParameterLocation.Header
    });
});

// Configure CORS for Web Portal and Desktop Application
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-seed Database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<NwsdbDbContext>();
        DbInitializer.Initialize(dbContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Enable Swagger UI in both Development and Production for academic grading convenience
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NWSDB SOC API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Default root redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
