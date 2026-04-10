
using System.Diagnostics;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class JuegoTopos : ContentPage
{

    // El Stopwatch va a generar un cronometro para medir el registro
    Stopwatch cronometro = new Stopwatch();
    // Creamos el temporizador que va a hacer que aparezcan los topos
    IDispatcherTimer timer;
    // Creamos el temporizador que va a hacer que aparezcan los topos
    IDispatcherTimer timerCro;



    public JuegoTopos()
    {
        InitializeComponent();

        // Metodo que realiza las creacion del tablero de juego
        CrearTablero();
        // Metodo que realiza la suma de tiempo al cronometro
        ConfigurarCronometro();
    }

    private void ConfigurarCronometro()
    {
        timerCro = Dispatcher.CreateTimer();
        timerCro.Interval = TimeSpan.FromMilliseconds(100); // Actualiza 10 veces por segundo
        timerCro.Tick += (s, e) =>
        {
            // Esta línea hace que el tiempo "suba" visualmente
            // Formato: mm (minutos) : ss (segundos) . fff (milesimas)
            lbCronometro.Text = cronometro.Elapsed.ToString(@"mm\:ss\.fff");

   
        };
    }

    private void CrearTablero()
    {

        // No dejamos visible el tablero hasta que no le demos a empezar, asi no puede darle a los topos antes de tiempo
        TableroJuego.IsVisible = false;

        // 1.- Preapramos nuestras imagenes en un plano de 3x3
        ImageButton[,] ArrayTopos = new ImageButton[3, 3];
        // 1.1- Generamos random para dar quien es el topo
        Random random = new Random();

        

        // 2.- Definimos el Grid con 3 filas y 3 columnas
        for (int i = 0; i < 3; i++)
        {
            TableroJuego.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            TableroJuego.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }

        // 3.- Creación de los elementos del tablero
        for (int fila = 0; fila < 3; fila++)
        {
            for (int columna = 0; columna < 3; columna++)
            {
                // 3.2.- Creamos el boton de imagen
                ImageButton boton = new ImageButton
                {
                    Source = "hueco_vacio.png",
                    HeightRequest = 150,
                    WidthRequest = 150,
                    //AutomationId = "vacio",
                    BindingContext = "abajo",
                    BackgroundColor = Colors.Transparent
                };

                // 3.3.- Asignación del evento para detectar el golpe
                boton.Clicked += AlGolpearTopo;

                // Almacenamiento en el arreglo y adición al Grid
                ArrayTopos[fila, columna] = boton;
                TableroJuego.Add(boton, columna, fila);
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
            foreach (ImageButton topo in ArrayTopos)
            {
                // Solo entramos si el topo esta arriba o ha sido clickado/golpeado, si esta abajo no hacemos nada
                if (topo.BindingContext.ToString() == "arriba" || topo.BindingContext.ToString() == "golpeado")
                {
                    topo.Source = "hueco_vacio.png";
                    topo.BindingContext = "abajo";
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

                // Subimos el topo en esa posición aleatoria
                ArrayTopos[filaAzar, colAzar].Source = "hueco_contopo.png";
                ArrayTopos[filaAzar, colAzar].BindingContext = "arriba";
            }


        };
        // Inicamos el temporizador
        //timer.Start();


    }

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

    private void AlGolpearTopo(object? sender, EventArgs e)
    {
        // Sacamos la imagen pulsada mediant el sender
        ImageButton boton = (ImageButton)sender;


        // Obtención del puntaje actual directamente desde el Label de la interfaz
        int puntosActuales = int.Parse(lbPuntaje.Text.Replace("Puntos: ", ""));
        int nuevoPuntaje;

        // Verificación del estado de la imagen para sumar puntos
        if (boton.BindingContext.ToString() == "arriba")
        {
            boton.Source = "topo_golpeado.png";

            // Le sumamos numeros a las puntuación
            nuevoPuntaje = puntosActuales + 10;

            boton.BindingContext = "golpeado";


        }
        else
        {
            // Si fallamos nos resta puntos
            nuevoPuntaje = puntosActuales - 10;
        }

        // Actualización de la puntuacion inutil
        lbPuntaje.Text = "Puntos: " + nuevoPuntaje.ToString();

    }

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
}