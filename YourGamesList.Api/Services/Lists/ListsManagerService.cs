using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Common;

namespace YourGamesList.Api.Services.Lists;

public interface IListsManagerService
{
    Task<CombinedResult<Guid, ListsError>> Create(CreateListDto createListDto);
    Task<ErrorResult<ListsError>> Delete(DeleteListDto deleteListDto);
}

public class ListsManagerService : IListsManagerService
{
    private readonly ILogger<ListsManagerService> _logger;

    public ListsManagerService(ILogger<ListsManagerService> logger)
    {
        _logger = logger;
    }

    public Task<CombinedResult<Guid, ListsError>> Create(CreateListDto createListDto)
    {
        throw new NotImplementedException();
    }

    public Task<ErrorResult<ListsError>> Delete(DeleteListDto deleteListDto)
    {
        throw new NotImplementedException();
    }
}