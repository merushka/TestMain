namespace WebApplication.DTOs
{
    public class ProductSalesDto
    {
        public int LeftCount { get; set; }
        public List<ProductOrderInfoDto> Orders { get; set; } = new();
    }

    public class ProductOrderInfoDto
    {
        public int OrderId { get; set; }
        public int Count { get; set; }
        public decimal Summ { get; set; }
        public required string UserName { get; set; }
    }
}