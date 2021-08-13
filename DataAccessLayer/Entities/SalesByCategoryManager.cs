using System;

#nullable disable

namespace DataAccessLayer
{
    public partial class SalesByCategoryManager
    {
        public DateTime Date { get; set; }
        public int? CategoryManagerId { get; set; }
        public string CategoryManagerName { get; set; }
        public decimal? SalesByCategoryManager1 { get; set; }
    }
}
