using Microsoft.Data.SqlClient;
using MyApiRestDapperSQL.Services.Implementations;
using MyApiRestDapperSQL.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Agregar configuración de CORS al contenedor de servicios
// Define el nombre de la política CORS
var corsPolicy = "AllowAngularLocalhost";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Permite solicitudes de Angular
                  .AllowAnyHeader() // Permite cualquier cabecera
                  .AllowAnyMethod() // Permite cualquier método HTTP
                  .AllowCredentials(); // Permite credenciales (cookies, autenticación básica, etc.)
                  
        });
});

//conexion a la base de datos SQL Server
builder.Services.AddScoped<SqlConnection>(_ => 
    new SqlConnection(
        builder.Configuration
        .GetConnectionString("DefaultConnection")
    )
);

// ✅ Agrega servicios para controladores
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Respeta el case exacto
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
    });



// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Usar la política CORS antes de los controladores
app.UseCors(corsPolicy);
app.UseAuthorization();

// Habilita el routing y los endpoints
app.UseRouting();
app.MapControllers();  // Mapea TODOS los controladores

app.Run();


