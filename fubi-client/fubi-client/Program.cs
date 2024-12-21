using fubi_client.Utils.comunes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession();

// Inyección de dependencias
builder.Services.AddHttpClient();

builder.Services.AddScoped<IComunes, Comunes>();

var app = builder.Build();

app.UseExceptionHandler("/Error/MostrarError");
app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
