using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication.Data;
using WebApplication.DTOs;

namespace WebApplication.Controllers
{
    [Route("summary")]
    [ApiController]
    public class SummaryController : ControllerBase
    {
        private readonly DatabaseContext _db;

        public SummaryController(DatabaseContext db)
        {
            _db = db;
        }

        [HttpGet("sales/products/{id}")]
        public IActionResult GetProductSalesSummary(int id)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound($"❌ Продукт с ID={id} не найден");

            var orders = _db.Orders
                .Include(o => o.Customer)
                .Where(o => _db.OrderItems.Any(oi => oi.ProductId == id && oi.OrderId == o.Id))
                .Select(o => new
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Count = _db.OrderItems.Where(oi => oi.OrderId == o.Id && oi.ProductId == id).Sum(oi => oi.Quantity),
                    TotalPrice = _db.OrderItems.Where(oi => oi.OrderId == o.Id && oi.ProductId == id)
                                               .Sum(oi => oi.Quantity * oi.Price),
                    UserName = o.Customer.Name ?? "Unknown User"
                })
                .ToList();

            int totalOrdered = orders.Sum(o => o.Count);
            int leftCount = product.Quantity - totalOrdered;

            return Ok(new
            {
                LeftCount = leftCount >= 0 ? leftCount : 0,
                Orders = orders
            });
        }

     
        [HttpPost("sales/products")]
        public IActionResult GetSalesSummary([FromBody] SalesSummaryRequest request)
        {
            if (request.ProductIds == null || request.ProductIds.Count == 0)
                return BadRequest("❌ Ошибка: Список ProductIds пуст!");

            if (request.DateStart > request.DateEnd)
                return BadRequest("❌ Ошибка: Неверный период. DateStart не может быть больше DateEnd.");

            var productIds = request.ProductIds.Distinct().ToList();

            var orders = _db.Orders
                .Include(o => o.Customer)
                .Where(o => request.DateStart <= o.OrderDate && o.OrderDate <= request.DateEnd)
                .Where(o => _db.OrderItems.Any(oi => productIds.Contains(oi.ProductId) && oi.OrderId == o.Id))
                .ToList();

            var orderItems = _db.OrderItems
                .Where(oi => orders.Select(o => o.Id).Contains(oi.OrderId))
                .Where(oi => productIds.Contains(oi.ProductId))
                .ToList();

            var result = orders
                .Select(o => new
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    UserName = o.Customer?.Name ?? "Unknown User",
                    Count = orderItems.Where(oi => oi.OrderId == o.Id).Sum(oi => oi.Quantity),
                    TotalPrice = orderItems.Where(oi => oi.OrderId == o.Id).Sum(oi => oi.Quantity * oi.Price),
                    Products = orderItems
                        .Where(oi => oi.OrderId == o.Id)
                        .GroupBy(oi => oi.ProductId)
                        .Select(oiGroup => new
                        {
                            ProductId = oiGroup.Key,
                            Quantity = oiGroup.Sum(oi => oi.Quantity),
                            Price = oiGroup.First().Price
                        })
                        .ToList()
                })
                .ToList();

            return Ok(new { Orders = result });
        }

        [HttpPost("sales")]
        public IActionResult GetUserSalesSummary([FromBody] UserSalesSummaryRequest request)
        {
            if (request.DateStart > request.DateEnd)
                return BadRequest("❌ Ошибка: Неверный период. DateStart не может быть больше DateEnd.");

            var orders = _db.Orders
                .Include(o => o.Customer)
                .Where(o => o.OrderDate >= request.DateStart && o.OrderDate <= request.DateEnd)
                .ToList();

            var orderItems = _db.OrderItems
                .Where(oi => orders.Select(o => o.Id).Contains(oi.OrderId))
                .ToList();

            var users = orders
                .GroupBy(o => o.Customer)
                .Select(g => new
                {
                    Name = g.Key?.Name ?? "Unknown User",
                    Summ = g.Sum(o => orderItems.Where(oi => oi.OrderId == o.Id).Sum(oi => oi.Quantity * oi.Price)),
                    Count = g.Sum(o => orderItems.Where(oi => oi.OrderId == o.Id).Sum(oi => oi.Quantity)),
                    Orders = g.Select(o => new
                    {
                        OrderId = o.Id,
                        OrderDate = o.OrderDate,
                        Summ = orderItems.Where(oi => oi.OrderId == o.Id).Sum(oi => oi.Quantity * oi.Price),
                        Products = orderItems
                            .Where(oi => oi.OrderId == o.Id)
                            .GroupBy(oi => oi.ProductId)
                            .Select(oiGroup => new
                            {
                                ProductId = oiGroup.Key,
                                Count = oiGroup.Sum(oi => oi.Quantity),
                                Price = oiGroup.First().Price
                            })
                            .ToList()
                    }).ToList()
                })
                .Where(u => u.Orders.Any())
                .ToList();

            return Ok(new { Users = users });
        }
    }
}
