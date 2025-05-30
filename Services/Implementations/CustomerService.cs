using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using MyApiRestDapperSQL.Models.Entities;
using MyApiRestDapperSQL.Services.Interfaces;

namespace MyApiRestDapperSQL.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly SqlConnection _dbConnection;
        public CustomerService(SqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<Customer>> GetAll()
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            // Ejecutar el procedimiento almacenado directamente sin parámetros de cursor
            var customers = await connection.QueryAsync<Customer>(
                sql: "dbo.GetAllCustomers", // Nombre del SP sin prefijo de paquete
                commandType: CommandType.StoredProcedure
            );

            return customers.ToList();
        }

        public async Task<Customer> GetById(int id)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);


            var parameters = new DynamicParameters();
            parameters.Add("@p_customer_id", id);

            var customer = await connection.QueryFirstOrDefaultAsync<Customer>(
                sql: "dbo.GetByIdCustomer",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            return customer;
        }

        public async Task<int> Add(Customer customer)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var parameters = new DynamicParameters();
            // Parámetros de entrada
            parameters.Add("@p_email_address", customer.EMAIL_ADDRESS);
            parameters.Add("@p_full_name", customer.FULL_NAME);
            // Parámetro de salida
            parameters.Add("@p_customer_id", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                sql: "dbo.AddCustomer",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            // Obtener el ID generado
            customer.CUSTOMER_ID = parameters.Get<int>("@p_customer_id");

            return customer.CUSTOMER_ID;
        }

        public async Task Update(Customer customer)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var parameters = new DynamicParameters();
            // Parámetros de entrada
            parameters.Add("@p_customer_id", customer.CUSTOMER_ID, DbType.Int32);
            parameters.Add("@p_email_address", customer.EMAIL_ADDRESS, DbType.String);
            parameters.Add("@p_full_name", customer.FULL_NAME, DbType.String);
            // Parámetro de salida
            parameters.Add("@p_rows_updated", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                sql: "dbo.UpdateCustomer",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            int rowsUpdated = parameters.Get<int>("@p_rows_updated");

            if (rowsUpdated != 1)
            {
                throw new Exception($"La actualización no se completó correctamente. Filas afectadas: {rowsUpdated}");
            }


        }

        public async Task Delete(int id)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var parameters = new DynamicParameters();
            // Parámetro de entrada
            parameters.Add("@p_customer_id", id, DbType.Int32);
            // Parámetro de salida
            parameters.Add("@p_rows_deleted", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                sql: "dbo.DeleteCustomer",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            int rowsDeleted = parameters.Get<int>("@p_rows_deleted");

            if (rowsDeleted < 1)
            {
                throw new Exception($"No se encontró ningún cliente con ID {id}");
            }
        }




    }
}