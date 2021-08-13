using DataAccessLayer.AppContext;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories.EFRepositories
{
    public class SalesByCategoryManagerRepository : ISalesByCategoryManagerRepository
    {
        public async Task<List<SalesByCategoryManager>> getCategoryManagers()
        {
            AvroraContext avroraContext = new AvroraContext();

            List<SalesByCategoryManager> salesByCategoryManagers = await avroraContext.SalesByCategoryManagers.OrderByDescending(x => x.SalesByCategoryManager1).ToListAsync();

            return salesByCategoryManagers;
        }
        public async Task<SalesByCategoryManager> getCategoryManager(int id)
        {
            AvroraContext avroraContext = new AvroraContext();

            SalesByCategoryManager salesByCategoryManager = await avroraContext.SalesByCategoryManagers.Where(x => x.CategoryManagerId == id).FirstAsync();

            return salesByCategoryManager;
        }
    }
}
