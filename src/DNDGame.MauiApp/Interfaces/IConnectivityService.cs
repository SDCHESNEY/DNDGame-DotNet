namespace DNDGame.MauiApp.Interfaces;

public interface IConnectivityService
{
    bool IsConnected { get; }
    event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;
    Task<bool> IsHostReachableAsync(string host);
}

public class ConnectivityChangedEventArgs : EventArgs
{
    public bool IsConnected { get; set; }
    public NetworkAccess NetworkAccess { get; set; }
}