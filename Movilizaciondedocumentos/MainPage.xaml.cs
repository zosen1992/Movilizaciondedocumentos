namespace Movilizaciondedocumentos
{
    public partial class MainPage : ContentPage
    {
        private CancellationTokenSource _cts;
        private int intervalo = 60000; // 1 minuto

        private System.Timers.Timer _visualTimer;
        private TimeSpan _tiempoTranscurrido;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void BtnIniciar_Clicked(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();

            _tiempoTranscurrido = TimeSpan.Zero;

            // Iniciar temporizador visual
            _visualTimer = new System.Timers.Timer(1000); // cada segundo
            _visualTimer.Elapsed += (s, args) =>
            {
                _tiempoTranscurrido = _tiempoTranscurrido.Add(TimeSpan.FromSeconds(1));

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    lblTimer.Text = $"Tiempo transcurrido: {_tiempoTranscurrido:hh\\:mm\\:ss}";
                });
            };
            _visualTimer.Start();

            await Task.Run(() => ProcesarArchivosPeriodicamente(_cts.Token));


        }

        private void BtnDetener_Clicked(object sender, EventArgs e)
        {
            _cts?.Cancel();
            _visualTimer?.Stop();
            _visualTimer?.Dispose();

            lblTimer.Text = "Tiempo transcurrido: 00:00:00";

        }

        private async Task ProcesarArchivosPeriodicamente(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        validarexistencia(txto.Text, txtd.Text);
                    });

                    await Task.Delay(intervalo, token);
                }
            }
            catch (TaskCanceledException)
            {
                // La tarea fue cancelada, no hacemos nada. Esto es esperado.
            }
            catch (Exception ex)
            {
                // Por si ocurre otra excepción inesperada
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Error", $"Excepción: {ex.Message}", "OK");
                });
            }
        }
        private void validarexistencia(string origen, string destino)
        {
            if (Directory.Exists(origen))
            {
                string[] archivos = Directory.GetFiles(origen, "*.pdf");

                if (archivos.Length > 0)
                {
                    foreach (string archivo in archivos)
                    {
                        try
                        {
                            string nombreArchivo = Path.GetFileName(archivo);
                            string rutaDestino = Path.Combine(destino, nombreArchivo);

                            // Verificar si ya existe y generar nombre alternativo
                            int contador = 2;
                            string nombreSinExtension = Path.GetFileNameWithoutExtension(nombreArchivo);
                            string extension = Path.GetExtension(nombreArchivo);

                            while (File.Exists(rutaDestino))
                            {
                                string nuevoNombre = $"{nombreSinExtension} ({contador}){extension}";
                                rutaDestino = Path.Combine(destino, nuevoNombre);
                                contador++;
                            }

                            // Mover archivo
                            File.Move(archivo, rutaDestino);

                            //DisplayAlert("Éxito", $"Archivo movido: {Path.GetFileName(rutaDestino)}", "OK");
                        }
                        catch (Exception ex)
                        {
                            DisplayAlert("Error", $"Error al mover archivo: {ex.Message}", "OK");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No hay archivos PDF para mover.");
                }
            }
            else
            {
                DisplayAlert("Error", "La carpeta de origen no existe", "OK");
            }
        }



    }
}
