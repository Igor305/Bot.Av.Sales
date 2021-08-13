using System;
using System.Collections.Generic;

#nullable disable

namespace DataAccessLayer
{
    public partial class ItPlanSaleStockOnDateD
    {
        public long ChId { get; set; }
        public int StockId { get; set; }
        public decimal PlanSum { get; set; }

        public virtual ItPlanSaleStockOnDate Ch { get; set; }
    }
}
