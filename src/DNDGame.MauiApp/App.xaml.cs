namespace DNDGame.MauiApp;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();
        
        MainPage = new AppShell();
    }

    protected override Microsoft.Maui.Controls.Window CreateWindow(Microsoft.Maui.IActivationState? activationState)
    {
        return new Microsoft.Maui.Controls.Window(MainPage) { Title = "DND Game" };
    }
}
