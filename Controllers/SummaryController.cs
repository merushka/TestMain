﻿using Microsoft.AspNetCore.Mvc;
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
                .Where(o => _db.OrderItems.Any(oi => oi.ProductId == id && oi.OrderId == o.Id))
                .Join(_db.Customers, o => o.CustomerId, c => c.Id, (o, c) => new { o, c }) 
                .Select(joined => new
                {
                    OrderId = joined.o.Id,
                    Count = _db.OrderItems.Where(oi => oi.OrderId == joined.o.Id && oi.ProductId == id).Sum(oi => oi.Quantity),
                    TotalPrice = _db.OrderItems.Where(oi => oi.OrderId == joined.o.Id && oi.ProductId == id)
                                               .Sum(oi => oi.Quantity * oi.Price),
                    UserName = joined.c.Name 
                })
                .ToList();

            var summary = new
            {
                LeftCount = product.Quantity,
                Orders = orders
            };

            return Ok(summary);
        }

        [HttpPost("sales/products")]
        public IActionResult GetSalesSummary([FromBody] SalesSummaryRequest request)
        {
            if (request.ProductIds == null || request.ProductIds.Count == 0)
                return BadRequest("❌ Ошибка: Список ProductIds пуст!");

            if (request.DateStart > request.DateEnd)
                return BadRequest("❌ Ошибка: Неверный период. DateStart не может быть больше DateEnd.");

            var ordersList = _db.Orders
                .Where(o => request.DateStart <= o.OrderDate && o.OrderDate <= request.DateEnd)
                .Where(o => _db.OrderItems.Any(oi => request.ProductIds.Contains(oi.ProductId) && oi.OrderId == o.Id))
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.CustomerId
                })
                .ToList(); 

            var orderItems = _db.OrderItems
                .Where(oi => ordersList.Select(o => o.Id).Contains(oi.OrderId))
                .Where(oi => request.ProductIds.Contains(oi.ProductId))
                .Select(oi => new
                {
                    oi.OrderId,
                    oi.ProductId,
                    oi.Quantity,
                    oi.Price
                })
                .ToList(); 

            var orders = ordersList
                .ToList() 
                .GroupBy(o => new { o.Id, o.OrderDate, o.CustomerId })
                .Select(g => new
                {
                    OrderId = g.Key.Id,
                    OrderDate = g.Key.OrderDate,
                    UserName = _db.Customers
                        .Where(c => c.Id == g.Key.CustomerId)
                        .Select(c => c.Name)
                        .FirstOrDefault(),
                    Count = orderItems.Where(oi => oi.OrderId == g.Key.Id).Sum(oi => oi.Quantity),
                    TotalPrice = orderItems.Where(oi => oi.OrderId == g.Key.Id).Sum(oi => oi.Quantity * oi.Price),
                    Products = orderItems
                        .Where(oi => oi.OrderId == g.Key.Id)
                        .Select(oi => new
                        {
                            oi.ProductId,
                            oi.Quantity,
                            oi.Price
                        })
                        .ToList()
                })
                .ToList();

            return Ok(new { Orders = orders });
        }

    }
}
