using DNDGame.MauiApp.Interfaces;

namespace DNDGame.MauiApp.Services;

public class NavigationService : INavigationService
{
    public Task NavigateToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }

    public Task NavigateToAsync(string route, IDictionary<string, object> parameters)
    {
        return Shell.Current.GoToAsync(route, parameters);
    }

    public Task GoBackAsync()
    {
        return Shell.Current.GoToAsync("..");
    }

    public Task PopToRootAsync()
    {
        return Shell.Current.GoToAsync("//");
    }

    public string CurrentRoute => Shell.Current?.CurrentState?.Location?.ToString() ?? string.Empty;
}
