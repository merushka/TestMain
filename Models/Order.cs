using LinqToDB.Mapping;
using System;

namespace WebApplication.Models
{
    [Table(Name = "ORDERS")]
    public class Order
    {
        [PrimaryKey, Identity]
        [Column("ID")]
        public int Id { get; set; }

        [Column("CUSTOMERID"), NotNull]
        public int CustomerId { get; set; }

        [Column("ORDERDATE"), NotNull]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow.AddMilliseconds(-DateTime.UtcNow.Millisecond); 

        [Association(ThisKey = "CustomerId", OtherKey = "Id", CanBeNull = false)]
        public Customer Customer { get; set; }
    }
}
