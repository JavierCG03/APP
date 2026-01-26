using CarslineApp.ViewModels;

namespace CarslineApp.Views;

public partial class RefaccionesTrabajo : ContentPage
{
    private readonly RefaccionesTrabajoViewModel _viewModel;

    public RefaccionesTrabajo(int trabajoId)
    {
        InitializeComponent();

        _viewModel = new RefaccionesTrabajoViewModel(trabajoId);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InicializarAsync();
    }
}