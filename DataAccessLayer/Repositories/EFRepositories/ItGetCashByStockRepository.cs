using DataAccessLayer.AppContext;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories.EFRepositories
{
    public class ItGetCashByStockRepository : IItGetCashByStockRepository
    {
        private readonly AvroraContext _avroraContext;

        public ItGetCashByStockRepository(AvroraContext avroraContext)
        {
            _avroraContext = avroraContext;
        }

        public async Task<List<ItGetCashByStock>> getAll()
        {
            List<ItGetCashByStock> itGetCashByStocks = await _avroraContext.ItGetCashByStocks.OrderBy(x=>x.StockId).ToListAsync();

            return itGetCashByStocks;
        }

        public async Task<decimal?> getSumm()
        {
            decimal? summ = await _avroraContext.ItGetCashByStocks.SumAsync(x => x.SumGetCash);

            return summ;
        }
    }
}
