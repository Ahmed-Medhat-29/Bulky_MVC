using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Interfaces;
using Bulky.Models;

namespace Bulky.DataAccess.Repositories;

public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
{
    private readonly ApplicationDbContext _context;

    public OrderDetailRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public void Update(OrderDetail OrderDetail)
    {
        _context.OrderDetails.Update(OrderDetail);
    }
}

