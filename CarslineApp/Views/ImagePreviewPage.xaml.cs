
namespace CarslineApp.Views
{
    public partial class ImagePreviewPage : ContentPage
    {
        // Estados de zoom y pan
        private double currentZoom = 1.0;
        private double startZoom = 1.0;
        private double xOffset = 0;
        private double yOffset = 0;

        // Límites
        private const double MIN_ZOOM = 1.0;
        private const double MAX_ZOOM = 4.0;
        private const double ZOOM_STEP = 0.50;
        private const double HEADER_HEIGHT = 56;
        private const double FOOTER_HEIGHT = 50;



        private bool isAnimating = false;


        public ImagePreviewPage(ImageSource imagen)
        {
            InitializeComponent();
            PreviewImage.Source = imagen;
        }
        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (isAnimating) return;

            switch (e.Status)
            {
                case GestureStatus.Started:
                    startZoom = currentZoom;

                    // Zoom centrado en los dedos
                    PreviewImage.AnchorX = e.ScaleOrigin.X;
                    PreviewImage.AnchorY = e.ScaleOrigin.Y;
                    break;

                case GestureStatus.Running:
                    double delta = Math.Abs(e.Scale - 1.0);

                    // Factores según intensidad del gesto
                    double zoomFactor =
                        delta < 0.25 ? 1.25 : 1.5;

                    double targetZoom;

                    if (e.Scale > 1.0)
                    {
                        // Zoom IN
                        targetZoom = startZoom * (1 + (e.Scale - 1) * zoomFactor);
                    }
                    else
                    {
                        // Zoom OUT
                        targetZoom = startZoom * (1 - (1 - e.Scale) * zoomFactor);
                    }

                    targetZoom = Math.Clamp(targetZoom, MIN_ZOOM, MAX_ZOOM);

                    if (Math.Abs(targetZoom - currentZoom) < 0.01)
                        return;

                    currentZoom = targetZoom;
                    PreviewImage.Scale = currentZoom;

                    UpdateZoomLabel();
                    break;

                case GestureStatus.Completed:
                    xOffset = PreviewImage.TranslationX;
                    yOffset = PreviewImage.TranslationY;
                    break;
            }
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (currentZoom <= 1.0 || isAnimating) return;


            switch (e.StatusType)
            {
                case GestureStatus.Started:

                    break;

                case GestureStatus.Running:


                    double newX = xOffset + e.TotalX;
                    double newY = yOffset + e.TotalY;

                    ClampPanBounds(ref newX, ref newY);

                    PreviewImage.TranslationX = newX;
                    PreviewImage.TranslationY = newY;
                    break;

                case GestureStatus.Completed:
                    xOffset = PreviewImage.TranslationX;
                    yOffset = PreviewImage.TranslationY;

                    // Snap a bordes
                    _ = SnapToEdgesAsync();
                    break;
            }
        }

        private async void OnDoubleTap(object sender, TappedEventArgs e)
        {
            if (isAnimating) return;

            // Alternar entre zoom base y 2x
            double targetZoom = currentZoom > 1.5 ? 1.0 : 2.0;
            await AnimateZoomAsync(targetZoom);
        }

        // ═══ BOTONES DEL HEADER ═══

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void OnZoomInClicked(object sender, EventArgs e)
        {
            if (isAnimating || currentZoom >= MAX_ZOOM) return;

            double targetZoom = Math.Min(currentZoom + ZOOM_STEP, MAX_ZOOM);
            await AnimateZoomAsync(targetZoom);
        }

        private async void OnZoomOutClicked(object sender, EventArgs e)
        {
            if (isAnimating || currentZoom <= MIN_ZOOM) return;

            double targetZoom = Math.Max(currentZoom - ZOOM_STEP, MIN_ZOOM);
            await AnimateZoomAsync(targetZoom);
        }

        // ═══ BOTONES DEL FOOTER ═══

        private async void OnResetClicked(object sender, EventArgs e)
        {
            await ResetZoomAsync();
        }

        private async void OnFitClicked(object sender, EventArgs e)
        {
            await ResetZoomAsync();
        }

        private async void OnInfoClicked(object sender, EventArgs e)
        {
            await DisplayAlert(
                "Información",
                $"Zoom: {(int)(currentZoom * 100)}%\n" +
                $"Posición X: {(int)xOffset}px\n" +
                $"Posición Y: {(int)yOffset}px",
                "OK"
            );
        }



        // ═══ ANIMACIONES ═══

        private async Task AnimateZoomAsync(double targetZoom)
        {
            if (isAnimating) return;
            isAnimating = true;

            try
            {
                // Asegurar que la imagen esté centrada durante zoom
                PreviewImage.AnchorX = 0.5;
                PreviewImage.AnchorY = 0.5;

                await PreviewImage.ScaleTo(targetZoom, 200, Easing.CubicInOut);
                currentZoom = targetZoom;
                UpdateZoomLabel();

                // Si reseteamos, también limpiar traducción
                if (Math.Abs(targetZoom - 1.0) < 0.01)
                {
                    await PreviewImage.TranslateTo(0, 0, 200, Easing.CubicOut);
                    xOffset = 0;
                    yOffset = 0;
                }
            }
            finally
            {
                isAnimating = false;
            }
        }

        private async Task ResetZoomAsync()
        {
            if (isAnimating) return;
            isAnimating = true;

            try
            {
                currentZoom = 1.0;
                xOffset = 0;
                yOffset = 0;

                await Task.WhenAll(
                    PreviewImage.ScaleTo(1.0, 250, Easing.CubicOut),
                    PreviewImage.TranslateTo(0, 0, 250, Easing.CubicOut)
                );

                UpdateZoomLabel();
            }
            finally
            {
                isAnimating = false;
            }
        }

        private async Task SnapToEdgesAsync()
        {
            const double snapDistance = 20;

            double targetX = Math.Abs(xOffset) < snapDistance ? 0 : xOffset;
            double targetY = Math.Abs(yOffset) < snapDistance ? 0 : yOffset;

            if (Math.Abs(targetX - xOffset) > 0.1 || Math.Abs(targetY - yOffset) > 0.1)
            {
                await PreviewImage.TranslateTo(targetX, targetY, 150, Easing.CubicOut);
                xOffset = targetX;
                yOffset = targetY;
            }
        }
        private void ClampPanBounds(ref double x, ref double y)
        {
            if (PreviewImage.Width <= 0 || PreviewImage.Height <= 0)
                return;

            double scaledWidth = PreviewImage.Width * currentZoom;
            double scaledHeight = PreviewImage.Height * currentZoom;

            double containerWidth = this.Width;
            double containerHeight = this.Height - HEADER_HEIGHT - FOOTER_HEIGHT;

            double maxX = Math.Max(0, (scaledWidth - containerWidth) / 2);
            double maxY = Math.Max(0, (scaledHeight - containerHeight) / 2);

            x = Math.Clamp(x, -maxX, maxX);
            y = Math.Clamp(y, -maxY, maxY);
        }


        // ═══ UTILIDADES ═══

        private void UpdateZoomLabel()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                int percentage = (int)(currentZoom * 100);
                ZoomLabel.Text = $"{percentage}%";

                // Cambiar color según nivel de zoom
                ZoomLabel.TextColor = currentZoom > 1.0
                    ? Color.FromArgb("#00FF00")
                    : Color.FromArgb("#00D4FF");
            });
        }
    }
}