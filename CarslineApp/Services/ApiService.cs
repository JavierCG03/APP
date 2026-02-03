
namespace CarslineApp.Services
{
    public partial class ApiService
    {
        private readonly HttpClient _httpClient;

        //private const string BaseUrl = "http://10.34.96.32:5293/api"; // Url_ Xiaomi
        private const string BaseUrl = "http://192.168.43.29:5293/api";//UrlHuawei
        //private const string BaseUrl = "http://192.168.3.95:5293/api"; // Url_ Oficina CARSLINE
        //private const string BaseUrl = "https://unmultipliable-advancingly-penni.ngrok-free.dev/api"; // Url_ Ngrok
        public ApiService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }


    }
}
        