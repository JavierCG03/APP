using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.ViewModels
{
    public class ListaUsuariosViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private ObservableCollection<UsuarioDto> _usuarios = new();

        public ListaUsuariosViewModel()
        {
            _apiService = new ApiService();
            RefreshCommand = new Command(async () => await CargarUsuarios());
            VolverCommand = new Command(async () => await OnVolver());
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<UsuarioDto> Usuarios
        {
            get => _usuarios;
            set
            {
                _usuarios = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand VolverCommand { get; }

        public async Task CargarUsuarios()
        {
            IsLoading = true;

            try
            {
                int adminId = Preferences.Get("user_id", 0);
                var usuarios = await _apiService.ObtenerUsuariosAsync(adminId);

                Usuarios.Clear();
                foreach (var usuario in usuarios)
                {
                    Usuarios.Add(usuario);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al cargar usuarios: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnVolver()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}