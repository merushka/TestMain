using LinqToDB.Mapping;

namespace WebApplication.Models
{
    [Table(Name = "ORDERITEMS")] 
    public class OrderItem
    {
        [PrimaryKey, Identity]
        [Column(Name = "ID")]
        public int Id { get; set; }

        [Column(Name = "ORDERID"), NotNull]
        public int OrderId { get; set; }

        [Column(Name = "PRODUCTID"), NotNull]
        public int ProductId { get; set; }

        [Column(Name = "QUANTITY"), NotNull]
        public int Quantity { get; set; }

        [Column(Name = "PRICE"), NotNull]
        public decimal Price { get; set; }
    }
}
