using System;

namespace YourGamesList.WebPage.Services.LocalStorage.Model;

public class LocalStorageItem<T>
{
    public required DateTimeOffset LastModified { get; set; }
    public required TimeSpan? Ttl { get; set; }
    public required T Item { get; set; }
}