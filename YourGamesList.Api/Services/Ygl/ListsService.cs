using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Database;

namespace YourGamesList.Api.Services.Ygl;

public interface IListsService
{
}

public class ListsService : IListsService
{
    private readonly ILogger<ListsService> _logger;

    private readonly YglDbContext _yglDbContext;
    
    public ListsService(ILogger<ListsService> logger, IDbContextFactory<YglDbContext> yglDbContext)
    {
        _logger = logger;
        _yglDbContext = yglDbContext.CreateDbContext();
    }
}