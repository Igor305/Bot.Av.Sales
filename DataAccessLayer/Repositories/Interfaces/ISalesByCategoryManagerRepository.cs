using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface ISalesByCategoryManagerRepository
    {
        public Task<List<SalesByCategoryManager>> getCategoryManagers();
        public Task<SalesByCategoryManager> getCategoryManager(int id);
    }
}
