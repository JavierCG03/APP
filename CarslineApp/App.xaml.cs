using CarslineApp.Views;

namespace CarslineApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            
            MainPage = new NavigationPage(new LoginPage())
            {
                BarBackgroundColor = Color.FromArgb("#D60000"),
                BarTextColor = Colors.White
            };

        }

    }
}