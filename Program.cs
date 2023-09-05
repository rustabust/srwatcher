using Microsoft.EntityFrameworkCore;
using sr_watcher.BackgroundServices;
using sr_watcher.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// add session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(1800);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddTransient<RedditPollService>();

// in memory db
//builder.Services.AddDbContext<SrWatcherDataContext>(options => options.UseInMemoryDatabase("SrWatchserDatabase"), ServiceLifetime.Singleton);

// sqlite file based db scoped
builder.Services.AddDbContext<SrWatcherDataContext>(options => options.UseSqlite("Data Source=SrWatcher.db"), ServiceLifetime.Scoped);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession(); 

app.MapRazorPages();

app.Run();
