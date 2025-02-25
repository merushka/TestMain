using LinqToDB.Mapping;

namespace WebApplication.Models
{
    [Table(Name = "PRODUCTS")]
    public class Product
    {
        [PrimaryKey, Identity]
        [Column(Name = "ID")]  
        public int Id { get; set; }

        [Column(Name = "NAME"), NotNull]
        public string Name { get; set; } = string.Empty;

        [Column(Name = "PRICE"), NotNull]
        public decimal Price { get; set; }

        [Column(Name = "QUANTITY"), NotNull]
        public int Quantity { get; set; }
    }
}
