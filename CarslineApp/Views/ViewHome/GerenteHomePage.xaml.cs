namespace CarslineApp.Views
{
    public partial class GerenteHomePage : ContentPage
    {
        public GerenteHomePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var userName = Preferences.Get("user_name", "Usuario");
            NombreLabel.Text = $"Bienvenido, {userName}";
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Cerrar Sesion", "Estas seguro que deseas cerrar sesion?", "Si", "No");
            if (confirm)
            {
                Preferences.Clear();
                Application.Current.MainPage = new NavigationPage(new LoginPage())
                {
                    BarBackgroundColor = Color.FromArgb("#512BD4"),
                    BarTextColor = Colors.White
                };
            }
        }
    }
}