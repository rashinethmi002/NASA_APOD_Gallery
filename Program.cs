using NASA_APOD_Gallery.Repositories;
using NASA_APOD_Gallery.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// NASA API HttpClient
builder.Services.AddHttpClient<NasaApodService>(client =>
{
    client.BaseAddress = new Uri("https://api.nasa.gov/");
    client.Timeout = TimeSpan.FromSeconds(100);

    client.DefaultRequestHeaders.Add(
        "User-Agent",
        "NASA-APOD-Gallery");
});

// Repository
builder.Services.AddScoped<ApodRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();