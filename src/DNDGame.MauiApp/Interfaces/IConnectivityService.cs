namespace DNDGame.MauiApp.Interfaces;

public interface IConnectivityService
{
    bool IsConnected { get; }
    event EventHandler<CustomConnectivityChangedEventArgs>? ConnectivityChanged;
    Task<bool> IsHostReachableAsync(string host);
}

public class CustomConnectivityChangedEventArgs : EventArgs
{
    public bool IsConnected { get; }
    
    public CustomConnectivityChangedEventArgs(bool isConnected)
    {
        IsConnected = isConnected;
    }
}