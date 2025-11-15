using DNDGame.MauiApp.Interfaces;
using System.Net.NetworkInformation;
using MauiConnectivityEventArgs = Microsoft.Maui.Networking.ConnectivityChangedEventArgs;

namespace DNDGame.MauiApp.Services;

public class ConnectivityService : IConnectivityService
{
    public event EventHandler<CustomConnectivityChangedEventArgs>? ConnectivityChanged;

    public ConnectivityService()
    {
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public async Task<bool> IsHostReachableAsync(string host)
    {
        if (!IsConnected)
        {
            return false;
        }

        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, 5000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    private void OnConnectivityChanged(object? sender, MauiConnectivityEventArgs e)
    {
        var isConnected = e.NetworkAccess == NetworkAccess.Internet;
        ConnectivityChanged?.Invoke(this, new CustomConnectivityChangedEventArgs(isConnected));
    }
}
