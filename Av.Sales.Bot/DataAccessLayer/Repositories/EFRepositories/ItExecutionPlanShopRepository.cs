using DataAccessLayer.AppContext;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories.EFRepositories
{
    public class ItExecutionPlanShopRepository : IItExecutionPlanShopRepository
    {
        public async Task<List<ItExecutionPlanShop>> getSales()
        {
            AvroraContext avroraContext = new AvroraContext();

            List<ItExecutionPlanShop> itExecutionPlanShops = await avroraContext.ItExecutionPlanShops.OrderBy(x=>x.StockId).ToListAsync();

            return itExecutionPlanShops;
        }
    }
}
