using MyApiRestDapperSQL.Models.Entities;

namespace MyApiRestDapperSQL.Services.Interfaces
{
    public interface IOrderItemService
    {
        Task<List<OrderItem>> GetAll();
        Task<OrderItem> GetById(int orderId, int lineItemId);
        Task<int> Add(OrderItem order);
        Task Update(OrderItem order);
        Task Delete(int orderId, int lineItemId);
    }
}