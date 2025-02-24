using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.DTOs
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "CustomerId обязателен")]
        public int CustomerId { get; set; }

        private DateTime _date;

        [Required(ErrorMessage = "Дата заказа обязательна")]
        public DateTime Date
        {
            get => _date;
            set => _date = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, DateTimeKind.Utc);
        }

        [Required(ErrorMessage = "Список товаров не может быть пустым")]
        [MinLength(1, ErrorMessage = "В заказе должен быть хотя бы один товар")]
        public List<OrderItemDto> Items { get; set; } = new();
    }


    public class OrderItemDto
    {
        [Required(ErrorMessage = "ProductId обязателен")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Количество обязательно")]
        [Range(1, 1000, ErrorMessage = "Количество товара должно быть от 1 до 1000")]
        [DefaultValue(1)]
        public int Quantity { get; set; } = 1;
    }
}
