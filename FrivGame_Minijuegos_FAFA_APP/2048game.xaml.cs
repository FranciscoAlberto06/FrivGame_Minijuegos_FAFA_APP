using BGestionFAFA;
using BModelosFAFA;
using Microsoft.Maui.Controls.Shapes;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class _2048game : ContentPage
{
    #region VARIABLES GLOBALES
    // Esta variable va a representar que zona del tablero esta ocupada por cada ficha y su valor 
    int[,] board = new int[4, 4];

    Border[,] listaBorders = new Border[4, 4]; 
    Label[,] listaLabels = new Label[4, 4];
    int puntuacion = 0;
    Random rnd = new Random();
    string PerfilUidActual;
    #endregion

    public _2048game(string uIdPerfil)
    {
        InitializeComponent();
        PerfilUidActual = uIdPerfil;

        CrearTablero();
        IniciarJuego();
    }

    #region CONFIGURACION TECLADO FISICO WINDOWS
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
#if WINDOWS
        ConfigurarTecladoWindows();
#endif
    }

#if WINDOWS
    private void ConfigurarTecladoWindows()
    {
        if (this.Window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
        {
            // Nos aseguramos de eliminar cualquier suscripción previa para evitar múltiples manejadores
            nativeWindow.Content.KeyDown -= AlPulsarTeclaFisica;
            nativeWindow.Content.KeyDown += AlPulsarTeclaFisica;
        }
    }


    private void AlPulsarTeclaFisica(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (tableroGrid == null || !tableroGrid.IsEnabled) return;

        SwipeDirection? direccion = null;

        switch (e.Key)
        {
            case Windows.System.VirtualKey.Up or Windows.System.VirtualKey.W:
   
                direccion = SwipeDirection.Up;
                break;
            case Windows.System.VirtualKey.Down or Windows.System.VirtualKey.S:
                direccion = SwipeDirection.Down;
                break;
            case Windows.System.VirtualKey.Left or Windows.System.VirtualKey.A:
                direccion = SwipeDirection.Left;
                break;
            case Windows.System.VirtualKey.Right or Windows.System.VirtualKey.D:
                direccion = SwipeDirection.Right;
                break;
        }

        // Si se ha pulsado una tecla de dirección, manejamos el evento de deslizamiento correspondiente
        if (direccion.HasValue)
        {
            e.Handled = true; // Evita que la ventana haga scroll al tocar las flechas
            onDeslizamiento(this, new SwipedEventArgs(null, direccion.Value));
        }
    }
#endif
    #endregion

    #region INICIALIZACION DEL JUEGO

    private void CrearTablero()
    {
        for (int f = 0; f < 4; f++)
        {
            for (int c = 0; c < 4; c++)
            {
                // Crear el fondo de la celda usando Border
                Border border = new Border
                {
                    BackgroundColor = Color.FromArgb("#23272A"),
                    Padding = 0,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(5) }
                };

                // Crear el texto de la celda
                Label label = new Label
                {
                    Text = "",
                    FontSize = 28,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                // Unir el label al border
                border.Content = label;

                // Añadimos cada uno a su posición en el grid
                listaBorders[f, c] = border;
                listaLabels[f, c] = label;

                // Metemos el border (con su label dentro) en el grid del tablero
                tableroGrid.Add(border, c, f);
            }
        }
    }
    private void IniciarJuego()
    {
        // Reseteamos variables
        Array.Clear(board, 0, board.Length);
        puntuacion = 0;

        if(tableroGrid.IsEnabled == false)
        {
            tableroGrid.IsEnabled = true; // Volvemos a habilitar el tablero por si se habia perdido y se habia deshabilitado
        }

        //Reseteamos tablero
        ResetearInterfazInicio();

        // Invocamos 2 ficha nuevas
        AddFichasRandom();
        AddFichasRandom();
    }

    private void ResetJuego(object sender, EventArgs e)
    {
        IniciarJuego();
    }

    // añadir una fiche aleatoria en una posición vacía del tablero, y ejecutar su animación de aparición
    private void AddFichasRandom()
    {
     
            // Lista con lo hueco libre que hay en el tablero 
            int[,] listaHuecos = new int[16, 2]; // 16 porque el tablero tiene 16 casillas, y 2 porque guardaremos la fila y la columna 

            // Contador para ir guardando los huecos encontrados en la lista
            int contadorHuecos = 0;

            // 2. Recorremos el tablero para encontrar los huecos libres
            for (int f = 0; f < 4; f++)
            {
                for (int c = 0; c < 4; c++)
                {
                    // Si encontramos un hueco libre
                    if (board[f, c] == 0)
                    {
                        // Lo guardamos en la lista de huecos libres la posicion
                        listaHuecos[contadorHuecos, 0] = f;
                        listaHuecos[contadorHuecos, 1] = c;
                        // Marcamos que hemos encontrado al menos un hueco libre
                        contadorHuecos++;
                    }
                }
            }

          


            // Elegimos del largo de la lista de huecos encontrados una posicion aleatoria de las que tenemos guardada
            int posicionHuecoRandom = rnd.Next(0, contadorHuecos);

            // Entraemos de la lista de hueco la posicion(fila y columna) aleatoria elegida para poner la ficha nueva
            int filaElegida = listaHuecos[posicionHuecoRandom, 0];
            int colElegida = listaHuecos[posicionHuecoRandom, 1];

            // Elegir tipo de ficha a poner 90% de que sea un 2 y 10% de que sea un 4
            if (rnd.Next(10) < 9)
            {
                // Dejamos marcado que este lugar esta ocupado con un 2
                board[filaElegida, colElegida] = 2;
            }
            else
            {
                // Dejamos marcado que este lugar esta ocupado con un 4
                board[filaElegida, colElegida] = 4;
            }

            // Actualizamos la interfaz para mostrar la ficha nueva
            ActualizarCeldaVisual(filaElegida, colElegida);


    
       
    }
    #endregion

    #region METODOS DE ACTUALIZACION VISUAL

    // Este metodo se encarga de actualizar toda la pantalla según el estado actual del tablero y la puntuación
    private void RefrescarPantalla()
    {
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                int val = board[r, c];

                // Actualizamos el texto: si es 0 ponemos vacío, si no el número
                listaLabels[r, c].Text = (val == 0) ? "" : val.ToString();

                // Actualizamos el color de fondo según el valor real
                listaBorders[r, c].BackgroundColor = GetColorCorrepondienteAValor(val);

                // Actualizamos el color del texto
                listaLabels[r, c].TextColor = val <= 4 ? Color.FromArgb("#776E65") : Colors.White;
            }
        }
        lPuntuacion.Text = $"Puntos: {puntuacion}";
    }

    // Este metodo se encarga actualizar solo una celda concreta del tablero(las que se añaden por cada movimiento)
    private void ActualizarCeldaVisual(int f, int c)
    {
        // 1. Sacamos el valor que hay en la lógica
        int valorFichaActual = board[f, c];

        // 2. Buscamos el Border y el Label que corresponden a esa posición en la interfaz
        Border borderActual = listaBorders[f, c];
        Label labelActual = listaLabels[f, c];

        // 3. Actualizamos el texto
        labelActual.Text = valorFichaActual.ToString();

        // 4. Actualizamos colores segundo el numero del cuadrado
        borderActual.BackgroundColor = GetColorCorrepondienteAValor(valorFichaActual);
        labelActual.TextColor = valorFichaActual <= 4 ? Color.FromArgb("#776E65") : Colors.White;

        // 5. Si acabamos de poner una ficha (valor > 0), animamos
        AnimacionAparecerFicha(f, c);
        

    }

    private async void AnimacionAparecerFicha(int fila, int col)
    {
        Border border = listaBorders[fila, col];
        // Animación sencilla: Escalar arriba y luego volver a la normalidad
        await border.ScaleTo(1.2, 100, Easing.CubicOut);
        await border.ScaleTo(1.0, 100, Easing.CubicIn);


    }

    // Este metodo se encarga de resetear la interfaz al estado inicial, dejando el tablero vacío y la puntuación a 0
    private void ResetearInterfazInicio()
    {
        // Recorremos las 16 celdas del tablero
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                // 1. Modificamos al estado inical lo que no esten modificados
                if (listaLabels[r, c].Text != "")
                {
                    // 2. Vaciamos el texto si el valor es 0
                    listaLabels[r, c].Text = "";

                    // 3. El color de fondo para el 0 sera el gris que definimos en el switch 
                    listaBorders[r, c].BackgroundColor = GetColorCorrepondienteAValor(0);

                    // 4. Color de texto por defecto para fichas bajas
                    listaLabels[r, c].TextColor = Color.FromArgb("#776E65");
                }
    
            }
        }

        // Reset visual de los puntos
        lPuntuacion.Text = "Puntos: 0";
    }

    // Este metodo se encarga de asignar un color a la ficha segun el valor que tenga
    private Color GetColorCorrepondienteAValor(int valor)
    {
        Color color = null;

        switch (valor)
        {
            case 0:
                color = Color.FromArgb("#23272A"); // Fondo vacío (Gris oscuro)
                break;
            case 2:
                color = Color.FromArgb("#4E5D94"); // Azul sutil
                break;
            case 4:
                color = Color.FromArgb("#7289DA"); // Azul Discord
                break;
            case 8:
                color = Color.FromArgb("#3CA45C"); // Verde esmeralda
                break;
            case 16:
                color = Color.FromArgb("#43B581"); // Verde brillante
                break;
            case 32:
                color = Color.FromArgb("#F9A71A"); // Naranja vibrante
                break;
            case 64:
                color = Color.FromArgb("#F57731"); // Naranja oscuro
                break;
            case 128:
                color = Color.FromArgb("#FF5555"); // Rojo suave
                break;
            case 256:
                color = Color.FromArgb("#FF0000"); // Rojo fuerte
                break;
            case 512:
                color = Color.FromArgb("#FF007F"); // Magenta
                break;
            case 1024:
                color = Color.FromArgb("#EB459E"); // Rosa/Fucsia
                break;
            case 2048:
                color = Color.FromArgb("#FFD700"); // Dorado 
                break;
            default:
                color = Color.FromArgb("#9B59B6"); // Púurpura (Si son mayores a 2048)
                break;
        }

        return color;
    }


    #endregion

    #region MÉTODOS DE DESLIZAMIENTO
    // Este método se ejecuta cada vez que el usuario hace un gesto de deslizamiento en la pantalla
    private async void onDeslizamiento(object sender, SwipedEventArgs e)
    {
        bool moved = false;

        // Segun el tipo de direccions que hagamos hacemos una cosa u otra
        switch (e.Direction)
        {
            case SwipeDirection.Left: 
                moved = MoverHorizontal(true); 
                break;
            case SwipeDirection.Right: 
                moved = MoverHorizontal(false); 
                break;
            case SwipeDirection.Up: 
                moved = MoverVertical(true); 
                break;
            case SwipeDirection.Down: 
                moved = MoverVertical(false); 
                break;
        }

        // Si se ha producido el movimiento añadimos una ficha y ademas actualizamos la pantalla para mostrar los cambios
        if (moved)
        {
            AddFichasRandom();
            RefrescarPantalla();
            if (ComprobarSiHaPerdido())
            {
                // 3. Guardamos partida
                // 3.1. Preparamos los datos de la partida en nuestro modelo
                Partida partidaFinal = new Partida
                {
                    IdPerfil = PerfilUidActual,
                    IdJuego = 4, // ID del juego de topos en la base de datos
                    Puntuacion = puntuacion,
                };

                #region GUARDADO DE PARTIDA EN BASE DE DATOS LOCAL Y NUBE
                // Insertamos la partida en la base de datos local
                ApiSQLiteFAFA.InsertarPartida(partidaFinal);

                // Subimos a la nube lo que tengamos en sqlite como no sincronizado, incluyendo esta partida que acabamos de guardar
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    await ApiRestFAFA.SincronizarHaciaApi("Partida");
                }
                #endregion
                await DisplayAlert("¡Has perdido!", "No hay mas movimientos posibles", "Cerrar");
                tableroGrid.IsEnabled = false; // Deshabilitamos el tablero para que no se puedan hacer más movimientos
            }
        }
    }
    private bool ComprobarSiHaPerdido()
    {
        bool perdido = true;

        // 1. Comprobar si hay huecos vacíos
        for (int f = 0; f < 4; f++)
        {
            for (int c = 0; c < 4; c++)
            {
                if (board[f, c] == 0)
                {
                    perdido = false;
                }
            }
        }

        // 2. Solo si seguimos pensando que ha perdido, buscamos fusiones horizontales
        if (perdido)
        {
            for (int f = 0; f < 4; f++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (board[f, c] == board[f, c + 1])
                    {
                        perdido = false;
                    }
                }
            }
        }

        // 3. Solo si seguimos pensando que ha perdido, buscamos fusiones verticales
        if (perdido)
        {
            for (int c = 0; c < 4; c++)
            {
                for (int f = 0; f < 3; f++)
                {
                    if (board[f, c] == board[f + 1, c])
                    {
                        perdido = false;
                    }
                }
            }
        }

        return perdido;
    }

    private bool MoverHorizontal(bool esIzquierda)
    {
        bool movido = false;

        for (int f = 0; f < 4; f++)
        {
            // 1. Extraemos la fila
            int[] filaTemporal = { board[f, 0], board[f, 1], board[f, 2], board[f, 3] };

            // 2. Si es derecha, giramos la fila antes de procesar
            if (!esIzquierda) Array.Reverse(filaTemporal);

            // 3. Procesamos (M
            if (MoverYFusionar(filaTemporal))
            {
                movido = true;
            }

            // 4. Si era derecha, giramos otra vez para devolverla a su sitio
            if (!esIzquierda) Array.Reverse(filaTemporal);

            // 5. Guardamos los cambios de vuelta en el board
            for (int c = 0; c < 4; c++)
            {
                board[f, c] = filaTemporal[c];
            }
        }

        return movido;
    }

    private bool MoverVertical(bool esArriba)
    {
        bool movido = false;

        for (int c = 0; c < 4; c++)
        {
            // 1. Extraemos la fila
            int[] filaTemporal = { board[0, c], board[1, c], board[2, c], board[3, c] };

            // 2. Si es derecha, giramos la fila antes de procesar
            if (!esArriba) Array.Reverse(filaTemporal);

            // 3. Procesamos (M
            if (MoverYFusionar(filaTemporal))
            {
                movido = true;
            }

            // 4. Si era derecha, giramos otra vez para devolverla a su sitio
            if (!esArriba) Array.Reverse(filaTemporal);

            // 5. Guardamos los cambios de vuelta en el board
            for (int f = 0; f < 4; f++)
            {
                board[f, c] = filaTemporal[f];
            }
        }

        return movido;
    }

    // Este metodo se encarga de deslizar y fusionar una linea (fila o columna) del tablero según las reglas del juego
    private bool MoverYFusionar(int[] lineas)
    {
        bool cambioRealizado = false;

        #region MOVER LAS FICHAS HACIA LOS HUECO VACIOS
        cambioRealizado = QuitarEspaciosAlMover(lineas, cambioRealizado);
        #endregion

        #region UNA VEZ MOVIDO, FUSIONAS LAS FICHAS IGUALES QUE ESTEN JUNTAS

        for (int i = 0; i < 3; i++)
        {
            if (lineas[i] == lineas[i+1] && lineas[i] !=0)
            {
                lineas[i] *= 2; // Duplicamos el valor de la ficha al fusionar
                lineas[i+1] = 0; // Vaciamos la ficha que se ha fusionado
                puntuacion += lineas[i]; // Sumamos a la puntuación el valor de la ficha resultante de la fusión
                cambioRealizado = true; // Marcamos que se ha realizado un cambio

            }
        }

        #endregion

        #region UNA VEZ FUSIONADOS LAS FICHAS MOVER LOS ESPACIOS QUE HAY ENTRE LAS FICHAS PARA DEJAR EL TABLERO ORDENADO

        cambioRealizado = QuitarEspaciosAlMover(lineas, cambioRealizado);


        #endregion

        

        return cambioRealizado;

    }

    private static bool QuitarEspaciosAlMover(int[] lineas, bool cambioRealizado)
    {
        // Desplazar a la izquierda
        for (int i = 0; i < 3; i++)
        {
            // Comprobamos si la ficha actual es 0, si es asi, buscamos la siguiente ficha no vacía para desplazarla a esta posicion
            if (lineas[i] == 0)
            {

                bool fichaEncontrada = false;
                // Buscamos a partir de donde hemos encontrado un ficha vacia, para mandarla a la ficha vacia la que encontremos llenas
                for (int j = i + 1; j < 4; j++)
                {
                    // Si encontramos la primer ficha que no este vacia
                    if (lineas[j] != 0 && !fichaEncontrada)
                    {
                        // Desplazamos esa ficha a la posicion vacia que habiamos encontrado
                        lineas[i] = lineas[j];
                        // Marcamos como vacia el lugar donde estaba la ficha que hemos desplazado
                        lineas[j] = 0;
                        // Hemos hecho un cambia asi que marcamos que se ha realizado un cambio
                        cambioRealizado = true;
                        // Marcamos que ya hemos movido y pues el hueco ya esta ocupado por una ficha 
                        fichaEncontrada = true; // Esto hace que ignore el resto del bucle j
                    }
                }
            }
        }

        return cambioRealizado;
    }
    #endregion

}