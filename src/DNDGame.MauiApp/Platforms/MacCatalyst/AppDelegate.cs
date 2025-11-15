using Foundation;

namespace DNDGame.MauiApp;

[Register("AppDelegate")]
public class AppDelegate : Microsoft.Maui.MauiUIApplicationDelegate
{
	protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
