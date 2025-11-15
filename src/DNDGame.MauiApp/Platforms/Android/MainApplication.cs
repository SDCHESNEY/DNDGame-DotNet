using Android.App;
using Android.Runtime;

namespace DNDGame.MauiApp;

[Application]
public class MainApplication : Microsoft.Maui.MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
