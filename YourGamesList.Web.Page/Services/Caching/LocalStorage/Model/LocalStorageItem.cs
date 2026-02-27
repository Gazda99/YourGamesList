using System;

namespace YourGamesList.Web.Page.Services.Caching.LocalStorage.Model;

public class LocalStorageItem<T>
{
    public required DateTimeOffset LastModified { get; set; }
    public required TimeSpan? Ttl { get; set; }
    public required T Item { get; set; }
}