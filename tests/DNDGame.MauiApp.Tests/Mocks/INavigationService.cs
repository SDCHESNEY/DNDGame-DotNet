namespace DNDGame.MauiApp.Interfaces;

public interface INavigationService
{
    Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null);
    Task GoBackAsync();
    Task PopToRootAsync();
    string CurrentRoute { get; }
}
