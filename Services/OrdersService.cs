using BlazingPizza.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazingPizza.Services;

public class OrdersService
{
    private readonly IDbContextFactory<PizzaStoreContext> _contextFactory;

    public OrdersService(IDbContextFactory<PizzaStoreContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<OrderWithStatus>> GetOrders()
    {
        using var db = _contextFactory.CreateDbContext();
        var orders = await db.Orders
         .Include(o => o.Pizzas).ThenInclude(p => p.Special)
         .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
         .OrderByDescending(o => o.CreatedTime)
         .ToListAsync();

        return orders.Select(o => OrderWithStatus.FromOrder(o)).ToList();
    }

    public async Task<int> PlaceOrder(Order order)
    {
        using var db = _contextFactory.CreateDbContext();

        order.CreatedTime = DateTime.Now;

        // Enforce existence of Pizza.SpecialId and Topping.ToppingId
        // in the database - prevent the submitter from making up
        // new specials and toppings
        foreach (var pizza in order.Pizzas)
        {
            pizza.SpecialId = pizza.Special.Id;
            pizza.Special = null;
        }

        db.Orders.Attach(order);
        await db.SaveChangesAsync();

        return order.OrderId;
    }

    public async Task<OrderWithStatus> GetOrderWithStatus(int orderId)
    {
        using var db = _contextFactory.CreateDbContext();

        var order = await db.Orders
            .Where(o => o.OrderId == orderId)
            .Include(o => o.Pizzas).ThenInclude(p => p.Special)
            .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
            .SingleOrDefaultAsync();

        if (order == null)
        {
            throw new Exception("NotFound");
        }

        return OrderWithStatus.FromOrder(order);
    }
}