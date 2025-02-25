using System;
using System.Collections.Generic;

namespace WebApplication.DTOs
{
    public class SalesSummaryRequest
    {
        public List<int> ProductIds { get; set; } = new();
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }       
    }
}
