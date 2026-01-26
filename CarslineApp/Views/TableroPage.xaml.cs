using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.Views;

public partial class TableroPage : ContentPage, INotifyPropertyChanged
{
    private readonly ApiService _apiService;
    private bool _isLoading;

    public ObservableCollection<TecnicoTableroDetalle> Tecnicos { get; set; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }
    }

    public bool IsNotLoading => !IsLoading;

    public Command<TecnicoTableroDetalle> ToggleExpandCommand { get; }

    public TableroPage()
    {
        InitializeComponent();
        _apiService = new ApiService();

        // Inicializar comando de expansión/contracción
        ToggleExpandCommand = new Command<TecnicoTableroDetalle>(tecnico =>
        {
            if (tecnico != null)
            {
                tecnico.IsExpanded = !tecnico.IsExpanded;
            }
        });

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarTableroAsync();
    }

    private async Task CargarTableroAsync()
    {
        if (IsLoading) return; // Evitar llamadas múltiples simultáneas

        IsLoading = true;

        try
        {
            Tecnicos.Clear();

            var trabajos = await _apiService.ObtenerTrabajosTecnicosAsync();

            if (trabajos == null || !trabajos.Any())
            {
                await DisplayAlert(
                    "Sin trabajos",
                    "No hay trabajos activos en este momento",
                    "OK");
                return;
            }

            // Agrupar trabajos por técnico y ordenar en una sola operación LINQ
            var gruposTecnicos = trabajos
                .Where(t => !string.IsNullOrWhiteSpace(t.TecnicoNombre))
                .GroupBy(t => t.TecnicoNombre)
                .Select(grupo => new
                {
                    Nombre = grupo.Key,
                    TrabajosOrdenados = grupo
                        .OrderByDescending(t => ObtenerPrioridadEstado(t.EstadoTrabajoNombre))
                        .ThenBy(t => t.FechaHoraPromesaEntrega)
                        .ToList()
                })
                .OrderBy(g => g.TrabajosOrdenados.Count) // Menos ocupados primero
                .ToList();

            // Crear objetos de técnicos
            foreach (var grupo in gruposTecnicos)
            {
                var tecnico = new TecnicoTableroDetalle
                {
                    TecnicoNombre = grupo.Nombre,
                    IsExpanded = false
                };

                foreach (var trabajo in grupo.TrabajosOrdenados)
                {
                    tecnico.Trabajos.Add(new TrabajoTableroItem
                    {
                        Trabajo = trabajo.Trabajo,
                        Estado = trabajo.EstadoTrabajoNombre ?? "Desconocido",
                        FechaEntrega = trabajo.FechaHoraPromesaEntrega
                    });
                }

                Tecnicos.Add(tecnico);
            }
        }
        catch (HttpRequestException)
        {
            await DisplayAlert(
                "Error de Conexión",
                "No se pudo conectar al servidor. Verifica tu conexión a internet.",
                "OK");
        }
        catch (TaskCanceledException)
        {
            await DisplayAlert(
                "Tiempo Agotado",
                "La solicitud tardó demasiado tiempo. Intenta nuevamente.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                $"Error al cargar tablero: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Determina la prioridad de un estado para ordenamiento
    /// </summary>
    private int ObtenerPrioridadEstado(string estado)
    {
        return estado switch
        {
            "Pendiente" => 1,
            "En Proceso" => 2,
            "Pausado" => 3,
            "Completado" => 4,
            _ => 5
        };
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await CargarTableroAsync();
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // Implementación de INotifyPropertyChanged
    public new event PropertyChangedEventHandler PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}