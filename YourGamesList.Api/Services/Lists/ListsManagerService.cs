using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Api.Services.Lists;

public interface IListsManagerService
{
    Task<CombinedResult<List<GamesListDto>, ListsError>> GetSelf(GetSelfListParams getSelfListParams);
    Task<CombinedResult<Guid, ListsError>> Create(CreateListParams createListParams);
    Task<ErrorResult<ListsError>> Delete(DeleteListParams deleteListParams);
}

public class ListsManagerService : IListsManagerService
{
    private readonly ILogger<ListsManagerService> _logger;

    public ListsManagerService(ILogger<ListsManagerService> logger)
    {
        _logger = logger;
    }

    public Task<CombinedResult<List<GamesListDto>, ListsError>> GetSelf(GetSelfListParams getSelfListParams)
    {
        throw new NotImplementedException();
    }

    public Task<CombinedResult<Guid, ListsError>> Create(CreateListParams createListParams)
    {
        throw new NotImplementedException();
    }

    public Task<ErrorResult<ListsError>> Delete(DeleteListParams deleteListParams)
    {
        throw new NotImplementedException();
    }
}