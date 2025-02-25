using LinqToDB.Mapping;

namespace WebApplication.Models
{
    [Table(Name = "CUSTOMERS")]
    public class Customer
    {
        [PrimaryKey, Identity]
        [Column(Name = "ID")]  
        public int Id { get; set; }

        [Column(Name = "NAME"), NotNull]
        public string Name { get; set; } = string.Empty;

        [Column(Name = "EMAIL"), NotNull]
        public string Email { get; set; } = string.Empty;
    }
}
