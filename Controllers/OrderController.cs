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
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var orders = await _orderService.GetAll();
                return Ok(orders);
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
                var orders = await _orderService.GetById(id);
                if (orders == null)
                {
                    return NotFound($"Order with ID {id} not found.");
                }
                return Ok(orders);
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
        public async Task<IActionResult> Add([FromBody] OrderDTO order)
        {
            if (order == null)
            {
                return BadRequest("Order data is null.");
            }

            try
            {
                var newOrder = new Order
                {
                    ORDER_TMS = DateTime.Now,
                    CUSTOMER_ID = order.CustomerId,
                    ORDER_STATUS = order.OrderStatus,
                    STORE_ID = 1
                };

                newOrder.ORDER_ID = await _orderService.Add(newOrder);
                return CreatedAtAction(nameof(GetById), new { id = newOrder.ORDER_ID }, newOrder);
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
        public async Task<IActionResult> Update(int id, [FromBody] OrderDTO order)
        {
            if (order == null)
            {
                return BadRequest("Order data is null.");
            }

            if (id != order.OrderId)
            {
                return BadRequest("Order ID mismatch.");
            }

            try
            {
                var existingOrder = await _orderService.GetById(id);
                if (existingOrder == null)
                {
                    return NotFound($"Order with ID {id} not found.");
                }

                existingOrder.CUSTOMER_ID = order.CustomerId;
                existingOrder.ORDER_STATUS = order.OrderStatus;
                existingOrder.ORDER_TMS = DateTime.Now; // Update timestamp
                existingOrder.STORE_ID = 1; // Assuming STORE_ID is fixed for this example

                await _orderService.Update(existingOrder);
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
                var existingOrder = await _orderService.GetById(id);
                if (existingOrder == null)
                {
                    return NotFound($"Order with ID {id} not found.");
                }

                await _orderService.Delete(id);
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