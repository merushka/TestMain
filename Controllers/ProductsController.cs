using Microsoft.AspNetCore.Mvc;
using LinqToDB;
using WebApplication.Data;
using WebApplication.Models;
using WebApplication.DTOs; 

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public ProductsController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] CreateProductDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Name))
                return BadRequest("Некорректные данные продукта.");

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Quantity = dto.Quantity
            };

            try
            {
                var newId = _context.InsertWithIdentity(product);
                product.Id = Convert.ToInt32(newId);

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при создании товара: {ex.Message}");
            }
        }

       
        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound($"Товар с id={id} не найден.");

            return Ok(product);
        }
    }
}
