using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface IItExecutionPlanShopRepository
    {
        public Task<List<ItExecutionPlanShop>> getSales();
    }
}
