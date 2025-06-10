using MyApiRestDapperSQL.Models.Entities;

namespace MyApiRestDapperSQL.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<Customer> GetById(int id);
        Task<Customer> GetByName(string name);
        Task<List<Customer>> GetAll();
        Task<int> Add(Customer customer);
        Task Update(Customer customer);
        Task Delete(int id);
    }
}