using Microsoft.Maui.Controls;

namespace CarslineApp.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            bool isPasswordVisible = false;

            TogglePasswordButton.Clicked += async (s, e) =>
            {
                isPasswordVisible = !isPasswordVisible;

                PasswordEntry.IsPassword = !isPasswordVisible;
                TogglePasswordButton.Source = isPasswordVisible ? "eye_off.png" : "eye.png";

                // Animación moderna
                await TogglePasswordButton.ScaleTo(0.85, 60);
                await TogglePasswordButton.ScaleTo(1, 60);
            };

            LoginButton.Pressed += async (_, _) =>
            {
                await LoginButton.ScaleTo(0.97, 80);
            };

            LoginButton.Released += async (_, _) =>
            {
                await LoginButton.ScaleTo(1, 80);
            };


        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            LogoImage.TranslationY = -30;
            LoginCard.TranslationY = 60;

            await Task.WhenAll(
                LogoImage.FadeTo(1, 600, Easing.CubicOut),
                LogoImage.TranslateTo(0, 0, 600, Easing.CubicOut)
            );

            await Task.WhenAll(
                LoginCard.FadeTo(1, 600, Easing.CubicOut),
                LoginCard.TranslateTo(0, 0, 600, Easing.CubicOut)
            );

            await FooterText.FadeTo(1, 600);
        }


        private void NavigateByRole(string rolNombre)
        {
            if (string.IsNullOrEmpty(rolNombre)) return;

            Page targetPage = rolNombre switch
            {
                "Administrador" => new AdminHomePage(),
                "Asesor de servicio" => new AsesorHomePage(),
                "Jefe de Taller" => new JefeHomePage(),
                "Gerente" => new GerenteHomePage(),
                "Tecnico de mantenimiento" => new TecnicoHomePage(),
                _ => null
            };

            if (targetPage != null)
            {
                Application.Current.MainPage = new NavigationPage(targetPage)
                {
                    BarBackgroundColor = Color.FromArgb("#D60000"),
                    BarTextColor = Colors.White
                };
            }
        }
    }
}