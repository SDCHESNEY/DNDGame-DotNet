using DNDGame.MauiApp.ViewModels;

namespace DNDGame.MauiApp.Pages;

public partial class SessionDetailPage : ContentPage
{
    public SessionDetailPage(SessionDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
