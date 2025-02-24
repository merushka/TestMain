using Microsoft.AspNetCore.Mvc;
using System.Linq;
using LinqToDB;
using WebApplication.Data;
using WebApplication.Models;
using WebApplication.DTOs;
using System;
using System.Collections.Generic;
using LinqToDB.Data;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly DatabaseContext _db;

        public OrderController(DatabaseContext db)
        {
            _db = db;
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] CreateOrderDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = _db.Customers.FirstOrDefault(c => c.Id == request.CustomerId);

            if (customer == null)
                return BadRequest($"❌ Покупатель с ID={request.CustomerId} не найден.");

            using var tran = _db.BeginTransaction();
            try
            {
                Console.WriteLine("🔄 Создание заказа...");

                // ✅ Обрезаем миллисекунды из OrderDate
                var cleanOrderDate = request.Date.AddMilliseconds(-request.Date.Millisecond);

                var order = new Order
                {
                    CustomerId = request.CustomerId,
                    OrderDate = new DateTime(
                        request.Date.Year,
                        request.Date.Month,
                        request.Date.Day,
                        request.Date.Hour,
                        request.Date.Minute,
                        0, // Секунды обрезаем
                        DateTimeKind.Utc)
                };
                var orderIdObj = _db.InsertWithIdentity(order);
                order.Id = Convert.ToInt32(orderIdObj);

                var orderItems = new List<OrderItem>();
                decimal totalOrderPrice = 0;

                foreach (var itemReq in request.Items)
                {
                    var product = _db.Products.FirstOrDefault(p => p.Id == itemReq.ProductId);
                    if (product == null)
                    {
                        tran.Rollback();
                        return BadRequest($"❌ Ошибка: Товар с ID={itemReq.ProductId} не найден.");
                    }

                    if (product.Quantity < itemReq.Quantity)
                    {
                        tran.Rollback();
                        return BadRequest($"❌ Ошибка: Недостаточно товара ID={itemReq.ProductId}. Остаток: {product.Quantity}");
                    }

                    var price = product.Price;
                    var totalPrice = price * itemReq.Quantity;
                    totalOrderPrice += totalPrice;

                    orderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = itemReq.ProductId,
                        Quantity = itemReq.Quantity,
                        Price = price
                    });

                    product.Quantity -= itemReq.Quantity;
                    _db.Update(product);

                    Console.WriteLine($"✅ Товар {itemReq.ProductId}: {itemReq.Quantity} шт. × {price} = {totalPrice}");
                }

                _db.BulkCopy(orderItems);
                tran.Commit();
                Console.WriteLine($"✅ Заказ {order.Id} успешно создан! Итоговая сумма: {totalOrderPrice}");

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new
                {
                    order.Id,
                    order.CustomerId,
                    OrderDate = cleanOrderDate,  // ✅ Отправляем дату без миллисекунд
                    TotalOrderPrice = totalOrderPrice,
                    Items = orderItems.Select(i => new
                    {
                        i.ProductId,
                        i.Quantity,
                        i.Price,
                        TotalPrice = i.Price * i.Quantity
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                tran.Rollback();
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
                return StatusCode(500, "Ошибка при создании заказа");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            var order = _db.Orders.FirstOrDefault(o => o.Id == id);
            
            return order == null ? NotFound($"Заказ {id} не найден") : Ok(order);
        }
    }
}
