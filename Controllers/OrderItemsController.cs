using Microsoft.AspNetCore.Mvc;
using System.Linq;
using LinqToDB;
using WebApplication.Data;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("orderitems")]
    public class OrderItemsController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public OrderItemsController(DatabaseContext context)
        {
            _context = context;
        }

        public class CreateSingleOrderItemDto
        {
            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }

        [HttpPost]
        public IActionResult CreateOrderItem([FromBody] CreateSingleOrderItemDto dto)
        {
            if (dto == null)
                return BadRequest("Некорректные данные позиции заказа.");

            var entity = new OrderItem
            {
                OrderId = dto.OrderId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Price = dto.Price
            };

            var newId = _context.InsertWithIdentity(entity);
            entity.Id = Convert.ToInt32(newId);

            return CreatedAtAction(nameof(GetOrderItem), new { id = entity.Id }, entity);
        }

        [HttpGet("{id}")]
        public IActionResult GetOrderItem(int id)
        {
            var orderItem = _context.OrderItems.FirstOrDefault(oi => oi.Id == id);
            if (orderItem == null)
                return NotFound();

            return Ok(orderItem);
        }
    }
}