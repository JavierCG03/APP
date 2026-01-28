using Microsoft.Maui.Controls;

namespace CarslineApp.Controls
{
    /// <summary>
    /// ContentView que detecta gestos de swipe horizontal
    /// </summary>
    public class SwipableContentView : ContentView
    {
        private const double SWIPE_THRESHOLD = 100; // Distancia mínima para considerar un swipe
        private const double SWIPE_VELOCITY_THRESHOLD = 0.3; // Velocidad mínima

        private double _startX;
        private double _startY;
        private DateTime _startTime;

        public event EventHandler? SwipedLeft;
        public event EventHandler? SwipedRight;

        public SwipableContentView()
        {
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGesture);
        }

        private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _startX = e.TotalX;
                    _startY = e.TotalY;
                    _startTime = DateTime.Now;
                    break;

                case GestureStatus.Running:
                    // Opcional: aquí podrías agregar feedback visual durante el swipe
                    break;

                case GestureStatus.Completed:
                    HandleSwipe(e.TotalX, e.TotalY);
                    break;

                case GestureStatus.Canceled:
                    // Resetear si se cancela
                    break;
            }
        }

        private void HandleSwipe(double totalX, double totalY)
        {
            var deltaX = totalX - _startX;
            var deltaY = totalY - _startY;
            var timeElapsed = (DateTime.Now - _startTime).TotalSeconds;

            // Verificar que el movimiento sea principalmente horizontal
            if (Math.Abs(deltaX) > Math.Abs(deltaY))
            {
                // Verificar que se cumplan los requisitos de distancia y velocidad
                if (Math.Abs(deltaX) > SWIPE_THRESHOLD)
                {
                    var velocity = Math.Abs(deltaX) / timeElapsed;

                    if (velocity > SWIPE_VELOCITY_THRESHOLD)
                    {
                        if (deltaX > 0)
                        {
                            // Swipe hacia la derecha
                            SwipedRight?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            // Swipe hacia la izquierda
                            SwipedLeft?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }
    }
}