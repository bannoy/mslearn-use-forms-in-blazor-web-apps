using Microsoft.EntityFrameworkCore;
using BlazingPizza.Data;

namespace BlazingPizza.Services;

public class SpecialsService
{
    private readonly IDbContextFactory<PizzaStoreContext> _contextFactory;

    public SpecialsService(IDbContextFactory<PizzaStoreContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<PizzaSpecial>> GetSpecials()
    {
        using var db = _contextFactory.CreateDbContext();
        return (await db.Specials.ToListAsync()).OrderByDescending(s => s.BasePrice).ToList();
    }
}
