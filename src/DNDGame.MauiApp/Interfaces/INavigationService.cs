namespace DNDGame.MauiApp.Interfaces;

public interface INavigationService
{
    Task NavigateToAsync(string route);
    Task NavigateToAsync(string route, IDictionary<string, object> parameters);
    Task GoBackAsync();
    Task PopToRootAsync();
    string CurrentRoute { get; }
}