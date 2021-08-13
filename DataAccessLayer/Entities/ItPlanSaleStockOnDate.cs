using System;
using System.Collections.Generic;

#nullable disable

namespace DataAccessLayer
{
    public partial class ItPlanSaleStockOnDate
    {
        public ItPlanSaleStockOnDate()
        {
            ItPlanSaleStockOnDateDs = new HashSet<ItPlanSaleStockOnDateD>();
        }

        public long ChId { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual ICollection<ItPlanSaleStockOnDateD> ItPlanSaleStockOnDateDs { get; set; }
    }
}
