using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using MyApiRestDapperSQL.Models.Entities;
using MyApiRestDapperSQL.Services.Interfaces;

namespace MyApiRestDapperSQL.Services.Implementations
{
    public class OrderItemService : IOrderItemService
    {
        private readonly SqlConnection _dbConnection;

        public OrderItemService(SqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<OrderItem> GetById(int orderId, int lineItemId)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@p_order_id", orderId);
            parameters.Add("@p_line_item_id", lineItemId);

            var item = await connection.QueryFirstOrDefaultAsync<OrderItem>(
                sql: "dbo.GetOrderItemById",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            return item;
        }

        public async Task<List<OrderItem>> GetAll()
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var items = await connection.QueryAsync<OrderItem>(
                sql: "dbo.GetAllOrderItems",
                commandType: CommandType.StoredProcedure
            );

            return items.ToList();
        }

        public async Task<int> Add(OrderItem item)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@p_order_id", item.ORDER_ID);
            parameters.Add("@p_line_item_id", item.LINE_ITEM_ID);
            parameters.Add("@p_product_id", item.PRODUCT_ID);
            parameters.Add("@p_unit_price", item.UNIT_PRICE);
            parameters.Add("@p_quantity", item.QUANTITY);
            parameters.Add("@p_shipment_id", item.SHIPMENT_ID);
            parameters.Add("@p_rows_inserted", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                sql: "dbo.AddOrderItem",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            return parameters.Get<int>("@p_rows_inserted");
        }

        public async Task Update(OrderItem item)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@p_order_id", item.ORDER_ID);
            parameters.Add("@p_line_item_id", item.LINE_ITEM_ID);
            parameters.Add("@p_product_id", item.PRODUCT_ID);
            parameters.Add("@p_unit_price", item.UNIT_PRICE);
            parameters.Add("@p_quantity", item.QUANTITY);
            parameters.Add("@p_shipment_id", item.SHIPMENT_ID);
            parameters.Add("@p_rows_updated", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                sql: "dbo.UpdateOrderItem",
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            int rowsUpdated = parameters.Get<int>("@p_rows_updated");

            if (rowsUpdated != 1)
            {
                throw new Exception($"La actualización de la orden no se completó correctamente. Filas afectadas: {rowsUpdated}");
            }
        }

        public async Task Delete(int orderId, int lineItemId)
        {
            await using var connection = new SqlConnection(_dbConnection.ConnectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@p_order_id", orderId);
            parameters.Add("@p_line_item_id", lineItemId);
            parameters.Add("@p_rows_deleted", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                sql: "dbo.DeleteOrderItem",
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