using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface IItGetCashByStockRepository
    {
        public Task<List<ItGetCashByStock>> getAll();

        public Task<decimal?> getSumm();
    }
}
