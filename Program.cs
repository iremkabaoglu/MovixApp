using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovixApp.Data;

var builder = WebApplication.CreateBuilder(args);

//  1) Veritaban� ba�lant�s� ekleniyor
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//  2) Identity servisleri ekleniyor
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>();

//  3) MVC ekleniyor
builder.Services.AddControllersWithViews();

//  4) HttpClient ekleniyor
builder.Services.AddHttpClient();

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();


var app = builder.Build();

// ================== Middlewares =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//  5) Authentication ve Authorization ekleniyor
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//  Identity sayfalar�n� eklemek i�in
app.MapRazorPages();

app.Run();
