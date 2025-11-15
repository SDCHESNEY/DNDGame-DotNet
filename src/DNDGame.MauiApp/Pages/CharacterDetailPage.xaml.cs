using DNDGame.MauiApp.ViewModels;

namespace DNDGame.MauiApp.Pages;

public partial class CharacterDetailPage : ContentPage
{
    public CharacterDetailPage(CharacterDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
