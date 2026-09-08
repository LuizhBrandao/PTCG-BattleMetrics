using System.Text.Json;
using Microsoft.JSInterop;

namespace PTCGBattleMetrics.Client.Services;

public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
}

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrWhiteSpace(json)) return default;
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch
        {
            // LocalStorage might be full or restricted
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch
        {
            // Ignore
        }
    }
}
