using Bulky.Models;

namespace Bulky.DataAccess.Repositories.Interfaces;

public interface IOrderDetailRepository : IRepository<OrderDetail>
{
    void Update(OrderDetail orderDetail);
}
