using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using MyApiRestDapperSQL.Models.Entities;
using MyApiRestDapperSQL.Services.Interfaces;

namespace MyApiRestDapperSQL.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly SqlConnection _dbConnection;

        public OrderService(SqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<Order>> GetAll()
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var orders = await connection.QueryAsync<Order>(
                sql: "dbo.GetAllOrders",
                commandType: CommandType.StoredProcedure
            );

            return orders.ToList();
        }

        public async Task<Order> GetById(int orderId)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@p_order_id", orderId);

            var order = await connection.QueryFirstOrDefaultAsync<Order>(
                sql: "dbo.GetOrderById",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            return order;
        }

        public async Task<int> Add(Order order)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var parameters = new DynamicParameters();
            // Parámetros de entrada
            parameters.Add("@p_order_tms", order.ORDER_TMS);
            parameters.Add("@p_customer_id", order.CUSTOMER_ID);
            parameters.Add("@p_order_status", order.ORDER_STATUS);
            parameters.Add("@p_store_id", order.STORE_ID);
            // Parámetro de salida
            parameters.Add("@p_order_id", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                sql: "dbo.AddOrder",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            // Obtener el ID generado
            int newOrderId = parameters.Get<int>("@p_order_id");
            return newOrderId;
        }

        public async Task Update(Order order)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var parameters = new DynamicParameters();
            // Parámetros de entrada
            parameters.Add("@p_order_id", order.ORDER_ID);
            parameters.Add("@p_order_tms", order.ORDER_TMS);
            parameters.Add("@p_customer_id", order.CUSTOMER_ID);
            parameters.Add("@p_order_status", order.ORDER_STATUS);
            parameters.Add("@p_store_id", order.STORE_ID);
            // Parámetro de salida
            parameters.Add("@p_rows_updated", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                sql: "dbo.UpdateOrder",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            int rowsUpdated = parameters.Get<int>("@p_rows_updated");

            if (rowsUpdated != 1)
            {
                throw new Exception($"La actualización de la orden no se completó correctamente. Filas afectadas: {rowsUpdated}");
            }
        }

        public async Task Delete(int orderId)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var parameters = new DynamicParameters();
            // Parámetro de entrada
            parameters.Add("@p_order_id", orderId);
            // Parámetro de salida
            parameters.Add("@p_rows_deleted", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                sql: "dbo.DeleteOrder",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            int rowsDeleted = parameters.Get<int>("@p_rows_deleted");

            if (rowsDeleted < 1)
            {
                throw new Exception($"No se encontró ninguna orden con ID {orderId}");
            }
        }

    }
}