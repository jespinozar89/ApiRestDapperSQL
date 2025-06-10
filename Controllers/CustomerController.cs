using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MyApiRestDapperSQL.Models.DTO;
using MyApiRestDapperSQL.Models.Entities;
using MyApiRestDapperSQL.Services.Interfaces;

namespace MyApiRestDapperSQL.Controllers
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var customers = await _customerService.GetAll();
                return Ok(customers);
            }
            catch (DbException ex)
            {
                // Log the exception
                return StatusCode(500, "Database error: " + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var customer = await _customerService.GetById(id);
                if (customer == null)
                {
                    return NotFound($"Customer with ID {id} not found.");
                }
                return Ok(customer);
            }
            catch (DbException ex) 
            {
                // Log the exception
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            try
            {
                var customer = await _customerService.GetByName(name);
                if (customer == null)
                {
                    return NotFound($"Customer with name {name} not found.");
                }
                return Ok(customer);
            }
            catch (DbException ex) 
            {
                // Log the exception
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

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
                return CreatedAtAction(nameof(GetById), new { id = newCustomer.CUSTOMER_ID }, newCustomer);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerDTO customer)
        {
            if (customer == null)
            {
                return BadRequest("Customer data is null.");
            }

            if (id != customer.CustomerId)
            {
                return BadRequest("Customer ID mismatch.");
            }

            try
            {
                var existingCustomer = await _customerService.GetById(id);
                if (existingCustomer == null)
                {
                    return NotFound($"Customer with ID {id} not found.");
                }

                existingCustomer.EMAIL_ADDRESS = customer.EmailAddress;
                existingCustomer.FULL_NAME = customer.FullName;

                await _customerService.Update(existingCustomer);
                return NoContent();
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingCustomer = await _customerService.GetById(id);
                if (existingCustomer == null)
                {
                    return NotFound($"Customer with ID {id} not found.");
                }

                await _customerService.Delete(id);
                return NoContent();
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
    }
}