using MyApiRestDapperSQL.Models.Entities;

namespace MyApiRestDapperSQL.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> GetById(int id);
        Task<List<Order>> GetAll();
        Task<int> Add(Order order);
        Task Update(Order order);
        Task Delete(int id);
    }
}