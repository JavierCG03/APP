using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class AgregarRefaccionPage : ContentPage
    {
        public AgregarRefaccionPage()
        {
            InitializeComponent();
            BindingContext = new AgregarRefaccionViewModel();
        }
    }
}