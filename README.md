# Tecnologias del aplicativo

esta aplicacion contiene las siguientes tecnologias:

- .NET8
- SqlServer
- Dapper
- API REST
- Inyeccion de dependencia
- DTO (Data Transfer Object)

# Crear Proyecto en Visual Studio Code
## 1. Comando crear Api .net8

Abrir terminal y ejecutar el siguiente comando:

```bash
dotnet new webapi -n MyApiProject
```

## 2. Configurar Dapper

1.- instalaremos los siguientes paquetes:

```bash
dotnet add package Dapper --version 2.1.28
dotnet add package Microsoft.Data.SqlClient --version 6.0.2
```

## 3. Crear archivo global.json .net8 (Opcional)

Para trabajar con una versión específica de .NET en nuestro proyecto, debemos crear un archivo `global.json` en la raiz del proyecto y especificaremos la versión del `SDK`. (esto se debe realizar solo si tenemos versiones superiores intaladas en nuestro equipo)

Ejemplo para trabajar en la version .NET8:

```bash
{
  "sdk": {
    "version": "8.0.409"
  }
}
```

Comando para listar SDKs
```bash
dotnet --list-sdks
```

## 4. Crear Modelo y Contexto

Esquema general del proyecto:

```bash
YourProject/
│
├── Controllers/         <- API Controllers
│   ├── CustomersController.cs
│   ├── OrderController.cs
│   ├── OrderItemController.cs
│
├── Models/              <- Entities and DTOs
│   ├── Entities/
│   │   ├── Customer.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   ├── DTOs/
│       ├── CustomerDTO.cs
│       ├── OrderDTO.cs
│       ├── OrderItemDTO.cs
│
├── Services/          <- Interfaces and implementations classes
│   ├── Interfaces/
│   │   ├── ICustomerService.cs
│   │   ├── IOrderService.cs
│   │   ├── IOrderItemService.cs
│   ├── Implementations/
│       ├── CustomerService.cs
│       ├── OrderService.cs
│       ├── OrderItemService.cs
│
│── appsettings.Development.json
│── appsettings.json
│── global.json
│── MyApiORACLE.csproj
│── Program.cs
```

1. Crear carpeta `/Models/Entities`.
2. Crear nuestros entidades `/Models/Entities/Customer.cs`, ejemplo:

```bash
namespace MiApiSqlServer.Models
{
    public class Customer
    {
        public int CUSTOMER_ID { get; set; }//PK
        public string EMAIL_ADDRESS { get; set; }
        public string FULL_NAME { get; set; }

    }
}
```

**Nota**: Es fundamental respetar la nomenclatura de las columnas tal como aparecen en la base de datos. Por ejemplo, si una columna se denomina `CUSTOMER_ID` en la base de datos, en nuestro modelo deberíamos nombrarla de manera consistente, como `CUSTOMER_ID`, para evitar inconvenientes al recuperar y mapear los datos.

3. Configurar JSON , sin convencion a cambios como el camelCase:

en la clase `program.cs` debemos agregar esta linea de codigo:

```bash
var builder = WebApplication.CreateBuilder(args);

// ✅ Agrega servicios para controladores
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Respeta el case exacto
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
    });
```


## 5. Configurar conexion SQl SERVER

1. Abre el archivo `appsettings.json` y agrega la cadena de conexión:

```bash
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=Customer_Orders;User Id=sa;Password=admin.2025;TrustServerCertificate=True;"
  },
```

2. Registra el contexto en `Program.cs`:

```bash

var builder = WebApplication.CreateBuilder(args);

// ✅ Agrega servicios para controladores
builder.Services.AddControllers();

//conexion a la base de datos SQL Server
builder.Services.AddScoped<SqlConnection>(_ => 
    new SqlConnection(
        builder.Configuration
        .GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

// Habilita el routing y los endpoints
app.UseRouting();
app.MapControllers();  // Mapea TODOS los controladores
```

## 6. Crear DTO

1. Crearemos la carpeta `/DTO` en `/Models`
2. Crearemos nuestros archivos DTO, ejemplo `CustomerDTO.cs`

```bash
namespace MiApiDapperSqlServer.DTO
{
    public class CustomerDTO
    {
        [Required]
        public int CustomerId { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }
        [Required]
        [MaxLength(50)]
        public string FullName { get; set; }
    }
}
```

**Nota**: En los DTO no es obligatorio mantener la misma nomenclatura que las columnas de la base de datos; sin embargo, se recomienda adoptar buenas prácticas en la convención de nombres. Por ejemplo, si la columna en la BD se llama CUSTOMER_ID, es conveniente nombrar la propiedad en el DTO como CustomerID.

## 7. Aplicar Servicios

1.Crearemos la carpetas `/Services` y dentro de ella `/Implementations` y `/Interfaces`.

2.Crear servicios específicos por entidad: Ejemplo Customer:

`/Interfaces`:

```bash
namespace MyApiRestDapperSqlServer.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<Customer> GetById(int id);
        Task<List<Customer>> GetAll();
        Task<int> Add(Customer customer);
        Task Update(Customer customer);
        Task Delete(int id);
    }
}
```

`/Implementations`

```bash
namespace MyApiRestDapperSqlServer.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly OracleConnection _dbConnection;

        public CustomerService(OracleConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }


        //Aqui agregar la implementaciones de los metodos de la intefaz

    }
}
```

## 8. Inyeccion de Dependencias

1. Registrar servicios: En `Program.cs`:

```bash

var builder = WebApplication.CreateBuilder(args);

// Registro de Servicios 
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();

var app = builder.Build();
```

## 9. Inyectar repositorios en los controladores

1. Crear clases api controller en la carpeta `/Controllers`
2. Usa el constructor de tus controladores para recibir las dependencias:

```bash
namespace MyApiRestDapperSqlServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        //Crear Metodos https

    }
}
```

3. usas tus DTOs: ejemplo metodo `HttpPost` con `try-catch`

```bash
[HttpPost]
public async Task<IActionResult> Add([FromBody] CustomerDTO customer)
{
    if (customer == null)
    {
        return BadRequest("Customer data is null.");
    }

    try
    {
        var newCustomer = new Customer
        {
            EMAIL_ADDRESS = customer.EmailAddress,
            FULL_NAME = customer.FullName
        };

        newCustomer.CUSTOMER_ID = await _customerService.Add(newCustomer);
        return CreatedAtAction(
            nameof(GetById), 
            new { 
                id = newCustomer.CUSTOMER_ID 
            }, 
            newCustomer);
    }
    catch (SqlException ex) when (ex.Number == 2627) // Violación de unique key
    {
        return StatusCode(409, $"Violación de unique key: {ex.Message}");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}
```

## 10. Configurar Cors

en nuestro archivo program.cs debemos agregar lo siguiente:

```bash
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

// Usar la política CORS antes de los controladores
app.UseCors(corsPolicy);
app.UseAuthorization();

app.Run();
```