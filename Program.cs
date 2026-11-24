using ApiPaisesMVC_SQLServer.Models;
using ApiPaisesMVC_SQLServer.Repositories;
using ApiPaisesMVC_SQLServer.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

// Carga el archivo .env (busca hacia los directorios padre) antes de crear el builder.
// Las claves usan el formato de variables de entorno, ej.: RestCountries__ApiKey.
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Entity Framework Core + SQL Server.
// La cadena de conexión proviene de .env (ConnectionStrings__DatabaseConnection)
// o de User Secrets. NO se incrusta en el código.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));

// Repositorios (acceso a datos) inyectados por su interfaz.
builder.Services.AddScoped<IPaisRepository, PaisRepository>();
builder.Services.AddScoped<IReportePaisRepository, ReportePaisRepository>();
builder.Services.AddScoped<IUniversidadRepository, UniversidadRepository>();

// HttpClient para consumir la API pública REST Countries v5.
// La v5 exige autenticación: header Authorization: Bearer <api_key>.
// La clave y la URL base se leen de la configuración (RestCountries:*).
builder.Services.AddHttpClient<IPaisService, PaisApiService>(client =>
{
    var apiKey = builder.Configuration["RestCountries:ApiKey"];
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
});

// HttpClient para consumir la API pública de Universidades (Actividad Avanzada).
builder.Services.AddHttpClient<IUniversidadService, UniversidadApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pais}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
