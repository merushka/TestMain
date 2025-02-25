namespace WebApplication.DTOs
{
    public class ProductsSummaryRequestDto
    {
        public List<int> ProductIds { get; set; } = new();
        public DateTime date_start { get; set; }
        public DateTime date_end { get; set; }
    }

    public class ProductsSummaryResponseDto
    {
        public List<ProductsSummaryOrderDto> Orders { get; set; } = new();
    }

    public class ProductsSummaryOrderDto
    {
        public int OrderId { get; set; }
        public int Count { get; set; }
        public decimal Summ { get; set; }
        public DateTime OrderDate { get; set; }
        public string UserName { get; set; } = "";
        public List<ProductsSummaryItemDto> Products { get; set; } = new();
    }

    public class ProductsSummaryItemDto
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}