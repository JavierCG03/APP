
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.Views.ChecksList;

public partial class CheckListReparacion : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    private readonly int _trabajoId;
    private readonly int _ordenId;
    private readonly string _trabajo;
    private bool _trabajoFinalizado = false;



    public CheckListReparacion(int trabajoId, int ordenId, string orden, string trabajo, string vehiculo, string indicacionestrabajo, string VIN)
    {
        InitializeComponent();

        _trabajoId = trabajoId;
        _ordenId = ordenId;
        _trabajo = trabajo;

        // Datos para mostrar en la vista
        lblVIN.Text = VIN;
        lblOrden.Text = orden;
        lblTrabajo.Text = trabajo;
        lblVehiculo.Text = vehiculo;
        lblindicacionesTrabajo.Text = indicacionestrabajo;
    }
    private async void PausarTrabajo_Clicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert(
            "Confirmar",
            "¿Deseas pausar la reparacion?",
            "Sí",
            "Cancelar");

        if (!confirmar) return;

        // ?? Pedir motivo
        string motivo = await DisplayPromptAsync(
            "Motivo de la pausa",
            "Describe el motivo de la pausa",
            "Aceptar",
            "Cancelar",
            placeholder: "Ej. Esperando refacciones");

        if (string.IsNullOrWhiteSpace(motivo))
        {
            await DisplayAlert(
                "Atención",
                "El motivo de la pausa es obligatorio",
                "OK");
            return;
        }

        int tecnicoId = Preferences.Get("user_id", 0);

        var response = await _apiService.PausarTrabajoAsync(
            _trabajoId,
            tecnicoId,
            motivo.Trim()
        );

        if (response.Success)
        {
            _trabajoFinalizado = true;
            await DisplayAlert("Éxito", response.Message, "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", response.Message, "OK");
        }
    }

    private async void FinalizarTrabajo_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtComentarioReparacion.Text) &&
            string.IsNullOrWhiteSpace(txtComentarioGeneral.Text))
        {
            await DisplayAlert(
                "Atención",
                "Debes agregar al menos un comentario",
                "OK");
            return;
        }

        bool confirmar = await DisplayAlert(
            "Confirmar",
            "¿Deseas concluir la reparacion?",
            "Sí",
            "Cancelar");

        if (!confirmar) return;

        // ?? Unir comentarios
        string comentariosFinales = ConstruirComentarios();

        var checklist = new CheckListServicioModel
        {
            TrabajoId = _trabajoId,
            OrdenId = _ordenId,
            Trabajo = _trabajo,
            ComentariosTecnico = comentariosFinales
        };

        try
        {
            int TecnicoId = Preferences.Get("user_id", 0);
            var response = await _apiService.CompletarTrabajoAsync(
                _trabajoId,
                TecnicoId,
                comentariosFinales
            );

            if (response.Success)
            {
                _trabajoFinalizado = true; // Marcar como finalizado
                await DisplayAlert("Éxito", response.Message, "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", response.Message, "OK");
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private string ConstruirComentarios()
    {
        var sb = new System.Text.StringBuilder();

        if (!string.IsNullOrWhiteSpace(txtComentarioReparacion.Text))
        {
            sb.AppendLine("Reparación:");
            sb.AppendLine(txtComentarioReparacion.Text.Trim());
        }

        if (!string.IsNullOrWhiteSpace(txtComentarioGeneral.Text))
        {
            sb.AppendLine("Comentarios generales:");
            sb.AppendLine(txtComentarioGeneral.Text.Trim());
        }

        return sb.ToString().Trim();
    }
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        if (_trabajoFinalizado) return; // Si ya finalizó, no hacer nada

        try
        {
            System.Diagnostics.Debug.WriteLine($"?? Restableciendo trabajo {_trabajoId} a PENDIENTE");
            int tecnicoId = Preferences.Get("user_id", 0);
            var response = await _apiService.RestablecerTrabajoAsync(_trabajoId, tecnicoId);

            if (!response.Success)
            {
                await Application.Current.MainPage.DisplayAlert("Error", response.Message, "OK");
                return;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error al restablecer estado: {ex.Message}");
        }
    }
}