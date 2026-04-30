
using BGestionFAFA;
using BModelosFAFA;
using System.Diagnostics;

namespace FrivGame_Minijuegos_FAFA_APP;

//Definimos los estados posibles
public enum EstadoTopo { Abajo, Arriba, Golpeado, Dorado, GolpeadoDorado }
public partial class JuegoTopos : ContentPage
{
    #region VARIABLES GLOBALES
    // El Stopwatch va a generar un cronometro para medir el registro
    Stopwatch cronometro = new Stopwatch();
    // Creamos el temporizador que va a hacer que aparezcan los topos
    IDispatcherTimer timer;
    // Creamos el temporizador que va a contar los segundo que hay en el cronometro para mostrarlo en la interfaz
    IDispatcherTimer timerCro;

    // Variable global para almacenar el estado de los topos, si estan arriba, abajo o golpeados
    Image[,] ArrayTopos = new Image[3, 3];

    string PerfilUidActual;

    private bool mensajeLogroActivo = false;




    #endregion


    public JuegoTopos(string uIdPerfil)
    {
        InitializeComponent();

        // Hacemos global el uid del perfil acutal
        PerfilUidActual = uIdPerfil;
        // Metodo que realiza las creacion del tablero de juegoas
        CrearTablero();
        // Metodo que realiza la suma de tiempo al cronometro
        ConfigurarCronometro();
    }

    #region CREACION DEL TABLERO DE JUEGO
    private void CrearTablero()
    {

        // No dejamos visible el tablero hasta que no le demos a empezar, asi no puede darle a los topos antes de tiempo
        TableroJuego.IsVisible = false;

        // 1- Generamos random para dar quien es el topo
        Random random = new Random();



        // 2.- Definimos el Grid con 3 filas y 3 columnas
        for (int i = 0; i < 3; i++)
        {
            TableroJuego.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            TableroJuego.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        }

        // 3.- Creación de los elementos del tablero
        for (int fila = 0; fila < 3; fila++)
        {
            for (int columna = 0; columna < 3; columna++)
            {
                // 3.2.- Creamos el boton de imagen
                Image imagenBoton = new Image
                {
                    Source = "hueco_vacio.png",
                    Aspect = Aspect.AspectFit,
                    StyleId = EstadoTopo.Abajo.ToString(), // ESTADO INICIAL
                    //AutomationId = "vacio",
                    BackgroundColor = Colors.Transparent,
                    HeightRequest = 120,
                    WidthRequest = 120,

                };

                // 2. Creamos la función de tocar y de clickar
                // 3. Definimos que hace la funcion al tocar y clickar
                if (DeviceInfo.Platform.ToString() == "Android")
                {
                    TapGestureRecognizer tapEvento = new TapGestureRecognizer();

                    tapEvento.Tapped += (s, e) =>
                    {
                        // Esta es la función de tocar
                        AlGolpearTopo(s, e);
                    };

                    // 4. Añadimos la función a la imagen
                    imagenBoton.GestureRecognizers.Add(tapEvento);

                }
                else if(DeviceInfo.Platform.ToString() == "WinUI")
                {
                    PointerGestureRecognizer ratonEvento = new PointerGestureRecognizer();

                    ratonEvento.PointerPressed += (s, e) =>
                    {
                        AlGolpearTopo(s, e);
                    };

                    // 4. Añadimos la función a la imagen
                    imagenBoton.GestureRecognizers.Add(ratonEvento);

                }




                // 5. Lo añadimos al Grid 
                ArrayTopos[fila, columna] = imagenBoton;
                TableroJuego.Add(imagenBoton, columna, fila);
            }
        }


        // 4.- Configuración del temporizador con tipo IDispatcherTimer
        timer = Dispatcher.CreateTimer();

        // 4.2.- Le asignamos un tiempo de 1 segundo
        timer.Interval = TimeSpan.FromMilliseconds(1500);


        // Se podria añadir aqui el evento pero no podrias pasarme el arrayTopos que necesito
        //timer.Tick += new EventHandler(dispatcherTimer_Tick);
        //Se puede hacer asin tambien
        //timer.Tick += metodo;



        // 5.- Creacion de evento cuando acabe el temporizador

        // Con el .tick asignamos un evento cuando cumpla su tiempo estableciodo el temporizador

        timer.Tick += (object sender, EventArgs e) =>

        {
            // Primero limpiamos el tablero (bajamos a todos los que subieron antes)
            foreach (Image topo in ArrayTopos)
            {
                // Solo entramos si el topo esta arriba o ha sido clickado/golpeado, si esta abajo no hacemos nada
                if (topo.StyleId != EstadoTopo.Abajo.ToString())
                {
                    topo.Source = "hueco_vacio.png";
                    topo.StyleId = EstadoTopo.Abajo.ToString();
                }
            }

            // TODO: Ajustar segun sea la dificultad poniendo mas topos
            // Decidimos cuántos topos van a salir (por ejemplo entre 1 y 3)
            int cantidadTopos = random.Next(1, 4);

            // Ejecutamos la acción tantas veces como diga nuestra cantidadTopos aleatoria
            for (int i = 0; i < cantidadTopos; i++)
            {
                // Generamos una fila y una columna aleatoria
                int filaAzar = random.Next(0, 3);
                int colAzar = random.Next(0, 3);
                Image topoActual = ArrayTopos[filaAzar, colAzar];

                // Probabilidad del 15% para topo dorado
                if (random.Next(1, 101) >= 15)
                {
                    topoActual.Source = "hueco_contopo.png";
                    topoActual.StyleId = EstadoTopo.Arriba.ToString();

                }
                else // Si esa probabilidad no se cumple, sale un topo dorado
                {
                    topoActual.Source = "hueco_contopo_dorado.png";
                    topoActual.StyleId = EstadoTopo.Dorado.ToString();
                }
            }


        };
        // Inicamos el temporizador
        //timer.Start();


    }
    #endregion

    #region CONFIGURACION CRONOMETRO
    private void ConfigurarCronometro()
    {
        timerCro = Dispatcher.CreateTimer();
        timerCro.Interval = TimeSpan.FromMilliseconds(100); // Actualiza 10 veces por segundo
        timerCro.Tick += (s, e) =>
        {
            if (cronometro.Elapsed.TotalSeconds >= 30)
            {
                FinalizarJuego();
            }
            // Esta línea hace que el tiempo "suba" visualmente
            // Formato: mm (minutos) : ss (segundos) . fff (milesimas)
            lbCronometro.Text = cronometro.Elapsed.ToString(@"mm\:ss\.fff");
            // Cuando llegemos a 30 s finalizamos
         

        };
    }

    


    private async void FinalizarJuego()
    {
        // 1. Detenemos todos los timers y el cronómetro
        cronometro.Stop();
        timer.Stop();
        timerCro.Stop();

        // 2. Ocultamos el tablero para que no sigan clickeando
        TableroJuego.IsVisible = false;
        bStartStop.IsVisible = false; // Deshabilitamos Iniciar/Pausar hasta que reinicie

        #region LOGRO PUNTUACION MAYOR A 450
        // Si la puntuacion a sido mayor a 450 debloqueamos el logro correspondiente a dicho usuario
        if (int.Parse(lbPuntaje.Text.Replace("Puntos: ", "")) >= 450)
        {
            await InsertarLogroUsuario(8);
        }
        #endregion


        // 3. Guardamos partida
        // 3.1. Preparamos los datos de la partida en nuestro modelo
        Partida partidaFinal = new Partida
        {
            IdPerfil = PerfilUidActual,
            IdJuego = 1, // ID del juego de topos en la base de datos
            Puntuacion = int.Parse(lbPuntaje.Text.Replace("Puntos: ", "")),
            TiempoSegundos = (int)cronometro.Elapsed.TotalSeconds,
        };
        #region REGISTRO DE PARTIDA
        try
        {
            // Insertamos la partida en la base de datos sqlite
            ApiSQLiteFAFA.InsertarPartida(partidaFinal);

            // Sincronizamos la partida con la nube si tenemos internet, si no se sincronizara cuando tenga conexion
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                await ApiAivenFAFA.SincronizarHaciaAiven("Partida");
            }

        }catch (Exception ex)
        {
            // Manejo de errores
            await DisplayAlert("Error", "No se pudo guardar la partida: " + ex.Message, "Aceptar");
        }

        #endregion

        // 4. Mostramos una alerta con el puntaje final
        await DisplayAlert("¡Tiempo agotado!", $"Partida finalizada. {lbPuntaje.Text}", "Aceptar");


    }
    #endregion

    #region FUNCIONES DE LOS TOPOS
    private void Timer_Tick(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    // Metodo al que podemos meterle dentro del temporizador
    //private void dispatcherTimer_Tick(object sender, EventArgs e)
    //{
    //    // Updating the Label which displays the current second
    //    lblSeconds.Content = DateTime.Now.Second;

    //    // Forcing the CommandManager to raise the RequerySuggested event
    //    CommandManager.InvalidateRequerySuggested();
    //}

    private async void AlGolpearTopo(object? sender, EventArgs e)
    {
        // Sacamos la imagen pulsada mediant el sender
        Image boton = (Image)sender;


        // Obtención del puntaje actual directamente desde el Label de la interfaz
        int puntosActuales = int.Parse(lbPuntaje.Text.Replace("Puntos: ", ""));
        int nuevoPuntaje = 0;

        // Verificacion del estado de la imagen para sumar puntos
        if (boton.StyleId == EstadoTopo.Arriba.ToString())
        {
            boton.Source = "topo_golpeado.png";
            boton.StyleId = EstadoTopo.Golpeado.ToString();
            nuevoPuntaje = puntosActuales + 10;
        }
        else if (boton.StyleId == EstadoTopo.Dorado.ToString())
        {
            boton.Source = "hueco_topodorado_golpeado.png";
            boton.StyleId = EstadoTopo.GolpeadoDorado.ToString();
            nuevoPuntaje = puntosActuales + 20;
            #region LOGRO TOPO DORADOR
            // Si ha tocado el logro del topo dorado le insertamos pasandole el id del logro en especifico de este caso
            await InsertarLogroUsuario(1);
            #endregion
        }
        else if (puntosActuales != 0) // Para que no sea negativo no se pierde punto estando en 0 
        {
            // Si fallamos nos resta puntos
            nuevoPuntaje = puntosActuales - 5;
            
        }

        // Actualización de la puntuacion inutil
        lbPuntaje.Text = "Puntos: " + nuevoPuntaje.ToString();

    }


    #endregion

    #region CONTROLADOR DEL CURSO DEL JUEGO  (INICIAR, PAUSAR, REINICIAR)
    private void StartStopClick(object sender, EventArgs e)
    {
        // Si el Cronometro no esta activo se activa el cronometro y el timer que genera lo topos
        if (!cronometro.IsRunning)
        {
            cronometro.Start();
            timer.Start();
            timerCro.Start();
            bStartStop.Text = "Pausar";
            TableroJuego.IsVisible = true;
        }
        else // en caso contrario dentrendriamos el cronometro y le dariamos a para de sacar topos, quitamos el tablero de los topos para que no pueda darle
        {
            cronometro.Stop();
            timer.Stop();
            timerCro.Stop();
            bStartStop.Text = "Continuar";
            TableroJuego.IsVisible= false;
        }
    }

    private void ReiniciarClick(object sender, EventArgs e)
    {
        // 1. Detenemos todo por seguridad
        cronometro.Stop();
        cronometro.Reset();
        timer.Stop();
        timerCro.Stop();

        // 2. Reseteamos la interfaz
        lbPuntaje.Text = "Puntos: 0";
        lbCronometro.Text = "00:00.000";
        bStartStop.Text = "Iniciar";
        bStartStop.IsVisible = true;
        TableroJuego.IsVisible = false;

        // 3. Limpiamos visualmente los topos (volver a huecos vacíos)
        foreach (Image topo in ArrayTopos)
        {
            topo.Source = "hueco_vacio.png";
            topo.StyleId = EstadoTopo.Abajo.ToString();
        }
    }
    #endregion

    #region METODOS LOGROS

    private async Task InsertarLogroUsuario(int idLogro)
    {
        // Comprobamos antes de nada si el usuario ya tiene el logro
        if (!ApiSQLiteFAFA.ComprobarSiTieneElLogro(PerfilUidActual, idLogro))
        {
            // Sino lo tiene lo insertamos
            ApiSQLiteFAFA.InsertarPerfilLogro(PerfilUidActual, idLogro);

            // Y si tiiene internet lo subimos a la nube el logro que ha debloqueado
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                await ApiAivenFAFA.SincronizarHaciaAiven("Logro");
            }

            // Mostramos el cartel del logro desbloqueado
            Logro logro = ApiSQLiteFAFA.ExtraerLogroPorId(idLogro);
            _ = cartelLogro.MostrarLogro(logro.Nombre, logro.XpPremio);

        }

    }

    #endregion



}