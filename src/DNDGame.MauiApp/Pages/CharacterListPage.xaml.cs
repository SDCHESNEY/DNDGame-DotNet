using DNDGame.MauiApp.ViewModels;

namespace DNDGame.MauiApp.Pages;

public partial class CharacterListPage : ContentPage
{
    public CharacterListPage(CharacterListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is CharacterListViewModel viewModel)
        {
            await viewModel.LoadCharactersCommand.ExecuteAsync(null);
        }
    }
}
