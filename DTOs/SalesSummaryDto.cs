namespace WebApplication.DTOs
{
    public class SalesSummaryRequestDto
    {
        public DateTime date_start { get; set; }
        public DateTime date_end { get; set; }
    }

    public class SalesSummaryResponseDto
    {
        public List<SalesSummaryUserDto> Users { get; set; } = new();
    }

    public class SalesSummaryUserDto
    {
        public required string Name { get; set; }
        public decimal Summ { get; set; }
        public int Count { get; set; }
        public List<SalesSummaryOrderDto> Orders { get; set; } = new();
    }

    public class SalesSummaryOrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Summ { get; set; }
        public List<SalesSummaryProductDto> Products { get; set; } = new();
    }

    public class SalesSummaryProductDto
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}