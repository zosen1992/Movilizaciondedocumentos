namespace Movilizaciondedocumentos
{
    public partial class MainPage : ContentPage
    {
        private CancellationTokenSource _cts;
        private int intervalo = 60000; // 1 minuto

        public MainPage()
        {
            InitializeComponent();
        }

        private async void BtnIniciar_Clicked(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            await Task.Run(() => ProcesarArchivosPeriodicamente(_cts.Token));
        }

        private void BtnDetener_Clicked(object sender, EventArgs e)
        {
            _cts?.Cancel();
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
