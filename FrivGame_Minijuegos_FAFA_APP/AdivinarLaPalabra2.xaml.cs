
using BGestionFAFA;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using System.Linq.Expressions;
using System.Text.Json;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class AdivinarLaPalabra2 : ContentPage
{
    //Ponemos static para que se pueda acceder a esta variable desde cualquier parte de la clase sin necesidad de crear una instancia de la clase
    private string _palabraSecreta = ""; // La palabra secreta modificada (sin acentos y en mayusculas)
    private int _filaActual = 0;
    private int _colActual = 0;
    private string _intentoActual = "";
    private Border[,] _celdas; // un array bidimensional para almacenar las celdas del tablero, cada celda es un Border que contiene un Label con la letra

    #region PROPIEDADES
    public string PalabraSecreta
    {
        get { return _palabraSecreta; }
        set { _palabraSecreta = value; }
    }



    public int FilaActual
    {
        get { return _filaActual; }
        set { _filaActual = value; }
    }

    public int ColActual
    {
        get { return _colActual; }
        set { _colActual = value; }
    }

    public string IntentoPalabraActual
    {
        get { return _intentoActual; }
        set { _intentoActual = value; }
    }

    public Border[,] Celdas
    {
        get { return _celdas; }
        set { _celdas = value; }
    }
    #endregion

    public AdivinarLaPalabra2()
    {
        InitializeComponent();


        ComprobarInternet(); // Comprobamos si hay conexion a internet para cargar la palabra de forma online o offline
        CrearTablero(); // Creamos el tablero adaptandose a la palabra objetivo
        CrearTeclado(); // Creamos el teclado con sus botones y su funcionalidad

    }


    #region CONFIGURACION TECLADO FISICO WINDOWS


    // Hay que usar el evento Handler  y no el OnAppearing porque el OnAppearing se ejecuta cada vez que se vuelve a mostrar la pantalla, y el Handler
    // solo se ejecuta la primera vez que se crea la pantalla, por lo que es más adecuado para configurar el teclado físico de Windows, ya que
    // solo queremos configurarlo una vez al crear la pantalla y no cada vez que se vuelva a mostrar
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
#if WINDOWS
            // Solo ejecutamos esto si estamos físicamente en Windows
            ConfigurarTecladoWindows();
#endif
    }



#if WINDOWS
        private void ConfigurarTecladoWindows()
    {

        // Accedemos a la ventana de Windows de forma segura
        // El nativeWindow es el objeto que representa la ventana nativa de Windows, y el Handler.PlatformView es la forma de acceder a esa ventana desde MAUI+
        // Este if hace que solo se ejecute el código de configuración del teclado físico si estamos en Windows, evitando errores en otras plataformas que no tienen esa ventana nativa
        if (this.Window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
        {
            // Quitamos el evento antes de ponerlo para no escribir 2 veces por pulsación
            nativeWindow.Content.KeyDown -= AlPulsarTeclaFisica;
            // El keydown es el evento que se ejecutar al pulsar una tecla fisica
            nativeWindow.Content.KeyDown += AlPulsarTeclaFisica;
        }
    }





    private void AlPulsarTeclaFisica(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        // Si el teclado visual está apagado (fin de juego), no hacemos nada
        if (KeyboardLayout == null || !KeyboardLayout.IsEnabled) return;

        // Convertimos la tecla a texto (ej: "A", "Enter", "Back")
        string key = e.Key.ToString().ToUpper();

        if (key == "ENTER")
        {
            if (_intentoActual.Length == _palabraSecreta.Length)
            {
                ValidarPalabra();
            }
        }
        else if (key == "BACK" || e.Key == Windows.System.VirtualKey.Back)
        {
            BorrarUltimaLetra();
        }
        else if (key.Length == 1 && char.IsLetter(key[0]))
        {
            // Solo enviamos letras de la A a la Z
            EscribirLetra(key);
        }

    }
#endif
    #endregion
    private void RecargarJuego(object sender, EventArgs e)
    {
        ComprobarInternet(); // Comprobamos si hay conexion a internet para cargar la palabra de forma online o offline
        CrearTablero(); // Creamos el tablero adaptandose a la palabra objetivo
        // El teclado no hace falta volver a crearlo porque no cambia
        bSignificado.IsVisible = false; // Volvemos a ocultar el boton de significado para que no se vea en medio del juego
        ButtonReiniciar.IsVisible = false; // Volvemos a ocultar el boton de reiniciar para que no se vea en medio del juego
        LabelMensaje.Text = "";
        FilaActual = 0; // Reseteamos la fila actual para volver a empezar desde la primera fila
        ColActual = 0; // Reseteamos la columna actual para volver a empezar desde la primera columna
        IntentoPalabraActual = ""; // Reseteamos el intento actual para que no se quede guardada la palabra que se estaba escribiendo
        KeyboardLayout.IsEnabled = true; // Volvemos a habilitar el teclado para que se pueda escribir la nueva palabra al haber reiniciado el juego
    }

    private void ComprobarInternet()
    {
        // 1. Comprobamos el estado de la red
        NetworkAccess accesoRed = Connectivity.Current.NetworkAccess;

        //CargarPalabraOffline();

        if (accesoRed == NetworkAccess.Internet)
        {
            // Si hay internet, ejecutamos el método online
            //CargarPalabraOnline();
            PalabraSecreta = ApiWordle.ObtenerPalabraAleatoria(); // Quitamos los acentos de la palabra secreta para que no haya problemas al comparar con el intento del usuario
        }
        else
        {
            // Si no hay internet, ejecutamos el método offline
            //CargarPalabraOffline();
            PalabraSecreta = ApiWordle.CargarPalabraOffline();
        }
    }







    public void CrearTablero()
    {
        // 1. Primero comprobamos el largo de la palabra a adivinar, para ajustar la cantidad de las columnas que necesita la palabra en el tablero 
        int columnas = PalabraSecreta.Length;
        Celdas = new Border[6, columnas]; // el numero de fila no lo tocamos porque siempre va a ver ese numero de intentos de escribir la palabra

        // 2. Limpiamos el Grid por si da problemas al volver a crear el tablero
        GridTablero.Children.Clear();
        GridTablero.ColumnDefinitions.Clear();
        GridTablero.RowDefinitions.Clear();

        for (int i = 0; i < columnas; i++)
        {
            // Esto hace que cada columna ocupe el mismo espacio proporcional
            GridTablero.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        }

        // 3. Crear las celdas
        for (int fila = 0; fila < 6; fila++)
        {
            for (int c = 0; c < columnas; c++)
            {
                Label etiqueta = new Label
                {
                    Text = "",
                    TextColor = Colors.Black,
                    FontSize = 25,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                Border cuadro = new Border
                {
                    Stroke = Colors.Gray,
                    StrokeThickness = 2,
                    // CAMBIO CLAVE: No ponemos WidthRequest ni HeightRequest fijos.
                    // Ponemos un tamaño mínimo para que mantenga la forma en móviles.
                    MinimumHeightRequest = 55,
                    MinimumWidthRequest = 55,
                    Padding = 5,
                    Content = etiqueta
                };

                Celdas[fila, c] = cuadro;
                GridTablero.Add(cuadro, c, fila);
            }
        }
    }

    private void CrearTeclado()
    {

     

        // 1. Definimos las filas con las teclas a usar
        string[][] filasTeclado = new string[][]
        {
            new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
            new string[] { "A", "S", "D", "F", "G", "H", "J", "K", "L", "Ñ" },
            new string[] { "ENTER", "Z", "X", "C", "V", "B", "N", "M", "<--" }
        };


        // Limpiamos el contenedor antes de generar
        KeyboardLayout.Children.Clear();


        // Vamos a recorrer cada fila del teclado
        foreach (string[] teclasSeparadas in filasTeclado)
        {
            // 2. Creamos un Grid para la fila 
            Grid fila = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                ColumnSpacing = 2,
                Margin = new Thickness(0, 2)
            };

            // 3. Recorremos todas las teclas de dicha fila de array 
            for (int i = 0; i < teclasSeparadas.Length; i++)
            {
             
                // Alargamos tanta veces columnas como letras haya en esa fila
                fila.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            }

            // 4. Creamos un botón para cada tecla y lo agregamos a la fila
            for (int i = 0; i < teclasSeparadas.Length; i++)
            {
                string texto = teclasSeparadas[i];

                // Creamos el botón con tus propiedades originales
                Button botonNuevo = new Button
                {
                    Text = texto,
                    BackgroundColor = Colors.LightGray,
                    TextColor = Colors.Black,
                    CornerRadius = 5,
                    Padding = 0, // Añadido para que el texto largo no se corte
                    FontSize = 14,

        
                };


                botonNuevo.Clicked += (s, e) => PresionarTecla(botonNuevo);

                // Añadimos el boton al Grid en la columna correspondiente
                fila.Add(botonNuevo, i, 0);
            }

            // Añadimos la fila completa al diseño del teclado (KeyboardLayout)
            KeyboardLayout.Add(fila);
        }

    }

    public void PresionarTecla(Button boton)
    {
        string teclaPulsada = boton.Text;

        // Usamos switch para que sea más visible y fácil de organizar
        switch (teclaPulsada)
        {
            case "ENTER":
                // Comprobamos que haya puesto una palabra 
                if (IntentoPalabraActual.Length == PalabraSecreta.Length )
                {
                    ValidarPalabra();
                }
                break;

            case "<--":
                // Borramos la ultima letra escrita
                BorrarUltimaLetra();
                break;

            default:
                // Si no es Enter ni Borrar, asumimos que es una letra
                EscribirLetra(teclaPulsada);
                break;
        }

    }

    private async void EscribirLetra(string teclaPulsada)
    {
        // Comprobamos si no hemos superado el largo de la palabra secreta
        if (IntentoPalabraActual.Length < PalabraSecreta.Length)
        {
            // 1. Localizamos la celda visual usando nuestros índices
            Border borderActual = Celdas[FilaActual, ColActual];  // La primera vez seria la 0, 0

            // 2. Sacamo el Label que está dentro del Border para poder cambiar su texto
            Label labelActual = (Label)borderActual.Content;

            // 3. Editamo el contenido del Label para mostrar la letra que se ha pulsado
            labelActual.Text = teclaPulsada;

            // Cambiamos el color del borde para indicar que la celda ya tiene una letra
            borderActual.Stroke = Colors.Black;

            // 4. A la palabra actual le añadimos la letra que se ha pulsado para ir formando la palabra que se va a ir escribiendo
            IntentoPalabraActual = IntentoPalabraActual + teclaPulsada;
            ColActual = ColActual + 1; // Cada vez que escribimos una leta le sumamo uno a las propieda de columna actual para la proxima letra que se escriba

            // 5. Animación simple simulando un rebote al escribir la leta
            // Lo ponemos await para que no se ejecute la 2 animacions a la vez y se ejecute en orden para que no se solapen
            await borderActual.ScaleTo(1.1, 50); // Crece un 10%
            await borderActual.ScaleTo(1.0, 50); // Vuelve a su tamaño

       
        }
    }

    private void BorrarUltimaLetra()
    {
        // Si no hay niguna letra escrita que no borremos nada
        if (IntentoPalabraActual.Length > 0)
        {
            // Retrocedemos una columna para posicionarno en la celda ultima escrita
            ColActual -= 1;

            Celdas[FilaActual, ColActual].Stroke = Colors.Gray; // Le devolvemos el color que tenia que tener el borde del Border 

            // Sacamos el label de la celda para cambiar su texto a vacio
            Label labelActual = (Label)Celdas[FilaActual, ColActual].Content; // Sacamos el label de la celda para cambiar su texto

            labelActual.Text = ""; // Borramos el contenido

            // Actualizar la IntentoPalabraActual quitando su ultima letra puesta
            IntentoPalabraActual = IntentoPalabraActual.Remove(IntentoPalabraActual.Length - 1);
            // El Remove quita una parte del string, le pasamos la posición desde donde queremos quitar, en este caso quitamos 1 caracter desde la ultima posición

        }

    }

    private async void ValidarPalabra()
    {
        try
        {
            // Bloqueamos el teclado para que no se pueda seguir escribiendo mientras se valida la palabra
            KeyboardLayout.IsEnabled = false;

            // Comprobamos si la palabra escrita existe en el diccionario, si no existe se lanzará una excepción que se capturará en el catch para mostrar un mensaje de error al usuario
            if (!ApiWordle.ComprobarSiExiste(IntentoPalabraActual))
            {
                LabelMensaje.TextColor = Colors.Green;
                throw new Exception("La palabra no existe en el diccionario, prueba con otra palabra");
            }

            // 1. Sacamos la palabra secreta en una lista de caracteres para ir tachando
            List<char> letrasRestantes = PalabraSecreta.ToList();

            // 2. Buscamos los verdes primero para quitar esas letras de la lista de letras restantes y que no se validen como amarillas después 
            for (int i = 0; i < PalabraSecreta.Length; i++)
            {
                // Si encontralos una letra en la posición correcta, la quitamos de la lista de letras restantes
                if (IntentoPalabraActual[i] == PalabraSecreta[i])
                {
                    // La quitamos de la lista para que no se valide como amarilla en la siguiente pasada
                    letrasRestantes.Remove(IntentoPalabraActual[i]);
                }
            }

            // Pintamos y animamos cada letra de la palabra
            for (int i = 0; i < PalabraSecreta.Length; i++)
            {
                // Sacar el border desde donde partirmo para leer
                Border border = Celdas[FilaActual, i];
                // Label de dicho border para cambiar su color de texto al validar
                Label label = (Label)border.Content;
                // Letras actual que se están validando 
                char letraIntento = IntentoPalabraActual[i];

                await border.RotateYTo(90, 150);

                // Si la letra esta en dicha posicion, la ponemos ne verde
                if (letraIntento == PalabraSecreta[i])
                {
                    // Cambiamos a VERDE
                    border.BackgroundColor = Colors.Green;
                }
                // Si la letra existe en la palabra pero no en esa posicion, la ponemos amarilla. En caso de que la letra exista pero ya se
                // haya encontrado en su posicion, y en otro lugar mas, la pondremos en gris la que no este en su lugar
                else if (letrasRestantes.Contains(letraIntento))
                {
                    // Es AMARILLO porque existe en la lista de "no usadas"
                    border.BackgroundColor = Colors.Gold;
                    letrasRestantes.Remove(letraIntento); // La quitamos para que la próxima repetida sea gris
                }
                else
                {
                    // Es GRIS
                    border.BackgroundColor = Colors.Gray;
                }

                border.Stroke = border.BackgroundColor;
                label.TextColor = Colors.White;
                await border.RotateYTo(0, 150);
            }
            // 2. Comprobar resultado final usando las propiedades y actualizando el Label de la interfaz
            if (IntentoPalabraActual == PalabraSecreta)
            {
                LabelMensaje.TextColor = Colors.Green;
                KeyboardLayout.IsEnabled = false; // Deshabilitamos el teclado para que no pueda seguir escribiendo al haber ganado
                ButtonReiniciar.IsVisible = true; // Hacemos visible el boton de reiniciar para que pueda volver a jugar
                bSignificado.IsVisible = true; // Hacemos visible el boton de significado para que pueda consultar el significado de la palabra al haber ganado
                throw new Exception("¡ENHORABUENA! HAS ACERTADO!");
            }
            else
            {
                // Avanzamos fila y reseteamos columnas/intento
                FilaActual = FilaActual + 1;
                ColActual = 0;
                IntentoPalabraActual = "";

                if (FilaActual == 6)
                {
                    LabelMensaje.TextColor = Colors.Red;
                    KeyboardLayout.IsEnabled = false; // Deshabilitamos el teclado para que no pueda seguir escribiendo al haber ganado
                    ButtonReiniciar.IsVisible = true; // Hacemos visible el boton de reiniciar para que pueda volver a jugar
                    bSignificado.IsVisible = true;
                    throw new Exception("FIN DEL JUEGO. LA PALABRA ERA: " + PalabraSecreta);


                }
                else
                {
                    LabelMensaje.TextColor = Colors.Gray;
                    LabelMensaje.Text="¡INTENTA DE NUEVO! TE QUEDAN " + (6 - FilaActual) + " INTENTOS";
                }
           
            }
        }
        catch (Exception error) { 
        
            LabelMensaje.Text = error.Message;


        }
        finally {             
            KeyboardLayout.IsEnabled = true;
        }
    }

    private async void ConsultarPalabra(object sender, EventArgs e)
    {
        
        string signficado = await ApiWordle.ObtenerSignificado(PalabraSecreta);

        DisplayAlert("Wikipedia", signficado, "Cerrar");

    }
}