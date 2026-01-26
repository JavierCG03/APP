using CarslineApp.ViewModels.ViewModelsHome;

namespace CarslineApp.Views
{
    public partial class AdminHomePage : FlyoutPage
    {
        private readonly AdminHomeViewModel _viewModel;

        public AdminHomePage()
        {
            InitializeComponent();
            _viewModel = new AdminHomeViewModel();
            BindingContext = _viewModel;

            // Configurar el comportamiento del flyout según la plataforma
            ConfigurarFlyout();
        }

        private void ConfigurarFlyout()
        {
#if WINDOWS
            // En Windows, el menú inicia cerrado
            IsPresented = false;
            
            // Permitir que el menú se pueda cerrar haciendo clic fuera de él
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
#elif ANDROID
            // En Android, el menú se puede deslizar
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
#else
            // Otros dispositivos
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
#endif
        }

        // Manejador del evento del botón hamburguesa
        private void OnMenuButtonClicked(object sender, EventArgs e)
        {
            // Alternar la visibilidad del menú
            IsPresented = !IsPresented;
        }

        // Inicializar datos y configurar cierre automático del menú
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Cerrar el menú cuando se ejecute algún comando de navegación
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.NombreUsuarioActual) ||
                    e.PropertyName == nameof(_viewModel.SuccessMessage))
                {
                    IsPresented = false;
                }
            };
        }
    }
}