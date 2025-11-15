using DNDGame.MauiApp.Pages;

namespace DNDGame.MauiApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute("character-detail", typeof(CharacterDetailPage));
        Routing.RegisterRoute("character-create", typeof(CharacterCreatePage));
        Routing.RegisterRoute("session-list", typeof(SessionListPage));
        Routing.RegisterRoute("session-detail", typeof(SessionDetailPage));
        Routing.RegisterRoute("session-create", typeof(SessionCreatePage));
    }
}
