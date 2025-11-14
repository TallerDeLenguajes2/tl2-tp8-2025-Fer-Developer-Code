using Interfaces;
using Repositories;
using Services;
//using MVC.Services;
var builder = WebApplication.CreateBuilder(args);
// --- 2. CONFIGURAR SERVICIOS DE SESIÓN ---
builder.Services.AddDistributedMemoryCache(); // Añade un almacén de memoria para la sesión
builder.Services.AddHttpContextAccessor(); // Necesario para acceder a HttpContext desde otros servicios
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Duración de la sesión
    options.Cookie.HttpOnly = true; // Cookie no accesible por JavaScript
    options.Cookie.IsEssential = true; // Esencial para que la app funcione
});
// --- 3. REGISTRO DE INYECCIÓN DE DEPENDENCIAS (DI) ---
// (Le decimos a .NET qué clase entregar cuando se pida una interfaz)
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IPresupuestoRepository, PresupuestosRepository>();
builder.Services.AddScoped<IUserRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
// El TP10 nos pide registrar esto ahora, lo crearemos en la Fase 2
// builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
// --- 4. SERVICIOS DE MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
// --- 5. CONFIGURAR EL PIPELINE HTTP (EL ORDEN ES CRÍTICO) ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
//  ¡CLAVE! UseSession() DEBE ir ANTES de UseAuthorization y MapControllers
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
