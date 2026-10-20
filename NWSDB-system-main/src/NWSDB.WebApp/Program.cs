using NWSDB.Data;
using NWSDB.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages and Session Support
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Database Connection and Business Services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "Data Source=nwsdb_system.db";
builder.Services.AddNwsdbServices(connectionString);

// Add HttpClient for API communication
builder.Services.AddHttpClient("NwsdbApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000/");
});

var app = builder.Build();

// Ensure Database is initialized & seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NwsdbDbContext>();
    DbInitializer.Initialize(dbContext);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
