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
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;
        public OrderItemController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var orderItems = await _orderItemService.GetAll();
                return Ok(orderItems);
            }
            catch (DbException ex)
            {
                return StatusCode(500, "Database error: " + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{orderId}/{lineItemId}")]
        public async Task<IActionResult> GetById(int orderId, int lineItemId)
        {
            try
            {
                var orderItem = await _orderItemService.GetById(orderId, lineItemId);
                if (orderItem == null)
                {
                    return NotFound($"OrderItem with OrderID {orderId} and LineItemID {lineItemId} not found.");
                }
                return Ok(orderItem);
            }
            catch (DbException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] OrderItemDTO orderItemDto)
        {
            if (orderItemDto == null)
            {
                return BadRequest("OrderItem data is null.");
            }

            try
            {
                var newOrderItem = new OrderItem
                {
                    ORDER_ID = orderItemDto.OrderId,
                    LINE_ITEM_ID = orderItemDto.LineItemId,
                    PRODUCT_ID = orderItemDto.ProductId,
                    UNIT_PRICE = orderItemDto.UnitPrice,
                    QUANTITY = orderItemDto.Quantity,
                    SHIPMENT_ID = 1
                };

                int rowsInserted = await _orderItemService.Add(newOrderItem);
                
                if (rowsInserted < 1)
                {
                    return StatusCode(500, "Failed to insert the order item.");
                }

                return CreatedAtAction(
                    nameof(GetById), 
                    new { 
                        orderId = newOrderItem.ORDER_ID, 
                        lineItemId = newOrderItem.LINE_ITEM_ID 
                    },
                    newOrderItem
                );
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique key violation
            {
                return StatusCode(409, $"Duplicate key violation: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{orderId}/{lineItemId}")]
        public async Task<IActionResult> Update(int orderId, int lineItemId, [FromBody] OrderItemDTO orderItemDto)
        {
            if (orderItemDto == null)
            {
                return BadRequest("OrderItem data is null.");
            }

            // Validate ID consistency
            if (orderId != orderItemDto.OrderId || lineItemId != orderItemDto.LineItemId)
            {
                return BadRequest("OrderItem ID mismatch between route and body.");
            }

            try
            {
                var existingOrderItem = await _orderItemService.GetById(orderId, lineItemId);
                if (existingOrderItem == null)
                {
                    return NotFound($"OrderItem with OrderID {orderId} and LineItemID {lineItemId} not found.");
                }

                // Update properties
                existingOrderItem.PRODUCT_ID = orderItemDto.ProductId;
                existingOrderItem.UNIT_PRICE = orderItemDto.UnitPrice;
                existingOrderItem.QUANTITY = orderItemDto.Quantity;
                existingOrderItem.SHIPMENT_ID = 1; // Assuming SHIPMENT_ID is always 1 for simplicity

                await _orderItemService.Update(existingOrderItem);
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

        [HttpDelete("{orderId}/{lineItemId}")]
        public async Task<IActionResult> Delete(int orderId, int lineItemId)
        {
            try
            {
                var existingOrderItem = await _orderItemService.GetById(orderId, lineItemId);
                if (existingOrderItem == null)
                {
                    return NotFound($"OrderItem with OrderID {orderId} and LineItemID {lineItemId} not found.");
                }

                await _orderItemService.Delete(orderId, lineItemId);
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