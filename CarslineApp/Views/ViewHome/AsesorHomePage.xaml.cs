using CarslineApp.ViewModels.ViewModelsHome;

namespace CarslineApp.Views
{
    public partial class AsesorHomePage : FlyoutPage
    {
        private readonly AsesorMainViewModel _viewModel;

        public AsesorHomePage()
        {
            InitializeComponent();
            _viewModel = new AsesorMainViewModel();
            BindingContext = _viewModel;

            // Configurar el comportamiento del flyout seg?n la plataforma
            ConfigurarFlyout();
        }

        private void ConfigurarFlyout()
        {
#if WINDOWS
            // En Windows, el men? inicia cerrado
            IsPresented = false;
            
            // Permitir que el men? se pueda cerrar haciendo clic fuera de ?l
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
#elif ANDROID
            // En Android, el men? se puede deslizar
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
#else
            // Otros dispositivos
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
#endif
        }

        // Manejador del evento del bot?n hamburguesa
        private void OnMenuButtonClicked(object sender, EventArgs e)
        {
            // Alternar la visibilidad del men?
            IsPresented = !IsPresented;
        }

        // Inicializar datos y configurar cierre autom?tico del men?
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Cargar ?rdenes cuando aparece la p?gina
            await _viewModel.InicializarAsync();

            // Cerrar el men? cuando se ejecute alg?n comando de navegaci?n
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.TituloSeccion))
                {
                    IsPresented = false;
                }
            };
        }
    }
}