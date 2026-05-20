using BGestionFAFA;
using BModelosFAFA;
using System.Diagnostics;
using System.Security.Cryptography;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class BuscarParejas : ContentPage    
{
    #region MIEMBROS PRIVADOS
    // Datos del juego
    ImageButton primeraCarta = null;
    int parejasEncontradas = 0;
    string temaActual = "";
    private string PerfilUidActual; // Guarda el id del usuario que esta jugando ahora mismo
    private Stopwatch _cronometroPartida = new Stopwatch(); // Un cronometro para medir el tiempo que tarda el usuario en resolver la partida
    int segundosTardados; // segundo que acaba tardando
    #endregion

    public BuscarParejas(string temaElegido, string uidPerfil)
    {
        InitializeComponent();
        temaActual = temaElegido;
        PerfilUidActual = uidPerfil;
        CrearTablero();
    }

    private List<string> BuscarImagenesTema()
    {
        List<string> listaImagenes = new List<string>();

        switch (temaActual)
        {
            case "Animales":
                listaImagenes = new List<string> { "gato.png", "perro.png", "conejo.png", "pollito.png", "cerdo.png", "capibara.png" };
                break;
            case "Frutas":
                listaImagenes = new List<string> { "fruta1.png", "fruta2.png", "fruta3.png", "fruta4.png", "fruta5.png", "fruta6.png" };
                break;
            case "Banderas":
                listaImagenes = new List<string> { "alemania.png", "argentina.jpg", "espana.png", "brasil.png", "francia.png", "moriles.jpg" };
                break;
            case "SuperHeroes":
                listaImagenes = new List<string> { "superheroe1.png", "superheroe2.png", "superheroe3.png", "superheroe4.png", "superheroe5.png", "superheroe6.png" };
                break;
            case "Version Especial Abelin":
                listaImagenes = new List<string> { "gato1.png", "gato2.jpg", "gato3.jpg", "gato4.png", "gato5.jpg", "gato6.png"};
                break;
        }

        listaImagenes.AddRange(listaImagenes); // Duplicamos la lista para tener las parejas

        return listaImagenes;

    }

    private void CrearTablero()
    {
        LblEstado.Text = "¡Encuentra las parejas!";

        // Buscamos las imagenes del tema elegido y las cargamos en la lista
        List<string> listaImagenesObtenidas = BuscarImagenesTema();


        // Usamos Random para elegir el índice al azar
        Random rnd = new Random();

        // Recorremos la lista de imagenes

        for (int f = 0; f < 4; f++)
        {
            for (int c = 0; c < 3; c++)
            {
                // 1. Elegimos un numero al azar entre 0 y el total de cartas que quedan
                int indiceAzar = rnd.Next(listaImagenesObtenidas.Count);

                // 2. Sacamos el nombre de la imagen de la lista principal
                string imagenElegida = listaImagenesObtenidas[indiceAzar];

                // 3. Borramos esa imagen de la lista principal para que no se repita
                listaImagenesObtenidas.RemoveAt(indiceAzar);

                // 4. Creamos el botón de imagen
                ImageButton boton = new ImageButton
                {
                    Source = "carta.png",        // Lo que se ve al principio
                    HeightRequest = 110,
                    WidthRequest = 110,
                    Padding = 5,
                    Margin = 10,
                    AutomationId = imagenElegida, // Para identificarlo luego
                    BackgroundColor = Colors.Transparent,

                };

                // 5. Le asignamos el evento del Click
                boton.Clicked += OnCartaClicked;


                // 6. Lo añadimos al Grid
                GridTablero.Add(boton, c, f);
            }

        }

        _cronometroPartida.Start(); // Empezamos a contar el tiempo desde que se crea el tablero, es decir, desde que el usuario puede empezar a jugar

    }

    private async void OnCartaClicked(object sender, EventArgs e)
    {
        // Bloqueamos TODO el tablero para que no entre ningún clic más
        GridTablero.IsEnabled = false;

        try
        {

            ImageButton cartaPulsada = (ImageButton)sender;


            // Comprobamos si hemos pulsado la misma carta que ya estaba dada la vuelta, en ese caso, no hacemos nada y desbloqueamos el tablero
            if (primeraCarta != cartaPulsada)
            {


                // 1. Siempre que clickemos una carta, la giramos para mostrar su imagen, usando su AutomationId para saber que imagen es
                // Giramos hasta el lateral donde no se ve la carta
                await cartaPulsada.RotateYTo(90, 250);

                // Cambiamos la imagen por la que tiene asignada en su AutomationId
                cartaPulsada.Source = cartaPulsada.AutomationId; // Le damos la vuelta mostrando su imagen

                // Terminamos de girar del todo para mostrar la carta
                await cartaPulsada.RotateYTo(0, 250);


                // 2. Comprobamos si ya hay alguna carta dada la vuelta, o si hemos clickado la misma carta (en ese caso, no hacemos nada)

                if (primeraCarta == null)
                {
                    // Sino la hay, esta es la primera carta, le damos la vuelta y la guardamos como primeraCarta
                    primeraCarta = cartaPulsada;

                }
                else
                {


                    // Comprobamos si las cartas coinciden comparando su AutomationId
                    if (primeraCarta.AutomationId == cartaPulsada.AutomationId)
                    {
                        // Si coinciden, las dejamos dadas la vuelta, aumentamos el contador de parejas encontradas y la dejamos para que no se puden clickar
                        primeraCarta.IsEnabled = false;
                        cartaPulsada.IsEnabled = false;
                        primeraCarta = null; // Reseteamos la primera carta para que pueda volver a ser la primera en el siguiente intento
                        parejasEncontradas++;

                        // Cuando se encentre las 6 parejas, mostramos mensaje de victoria y el botón de reiniciar
                        if (parejasEncontradas == 6)
                        {
                            _cronometroPartida.Stop(); // Paramos el cronometro al encontrar la ultima pareja
                            segundosTardados = _cronometroPartida.Elapsed.Seconds; // Guardamos los segundos que ha tardado en resolver la partida
                            LblEstado.Text = "¡HAS GANADO!";
                            LblEstado.TextColor = Colors.Green;

                            await ProbarLogro(4);
                            #region GUARDADO EN BD

                            // Preparamos partida
                            Partida partidaNueva = new Partida
                            {
                                IdPerfil = PerfilUidActual,
                                IdJuego = 3, // Id del juego de buscar parejas
                                Puntuacion = CalcularPuntuacion(segundosTardados),
                                Victoria = true,
                                TiempoSegundos = segundosTardados,
                            };

                            // Mandamos al sqlite local
                            ApiSQLiteFAFA.InsertarPartida(partidaNueva);

                            // Si tenemos conexión a internet, mandamos a la nube la partida
                            if(Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                            {
                                await ApiRestFAFA.SincronizarHaciaApi("Partida");
                            }

                            #endregion

                            // Mostramos el botón de reiniciar
                            bReiniciar.IsVisible = true;
                        }
                    }
                    else
                    {
                        // Si no coinciden, esperamos un momento para que el usuario lo vea, y las volvemos a girar boca abajo
                        await Task.Delay(1000);
                        // Giramos la carta , tanto la primera con la segunda
                        AnimacionVolverCarta(cartaPulsada);
                        AnimacionVolverCarta(primeraCarta);
                        primeraCarta = null; // Reseteamos la primera carta para que pueda volver a ser la primera en el siguiente intento


                    }
                }


            }
        }
        finally
        {
            // Pase lo que pase, al terminar, desbloqueamos el tablero
            GridTablero.IsEnabled = true;
        }
    

    }

    private static async Task AnimacionVolverCarta(ImageButton cartaPulsada)
    {
        await cartaPulsada.RotateYTo(90, 250);

        // Cambiamos la imagen por la que tiene asignada en su AutomationId
        cartaPulsada.Source = "carta.png"; // Le damos la vuelta mostrando su imagen

        cartaPulsada.RotationY = 90;

        // Terminamos de girar del todo para mostrar la carta
        await cartaPulsada.RotateYTo(0, 250);
    }

    private void ReiniciarJuego(object sender, EventArgs e)
    {
        // Limpiamos tablero si hay partida previa
        GridTablero.Children.Clear(); 

        // Reseteamos el cronometro para la nueva partida
        _cronometroPartida.Reset(); 

        // Recargamos el tablero con el mismo tema
        CrearTablero();

        // Reseteamos el contador de parejas encontradas
        parejasEncontradas = 0;

        // Ocultamos el botón de reiniciar
        bReiniciar.IsVisible = false;


    }

    #region GESTION DE PUNTUACION
    private int CalcularPuntuacion(int tiempoSegundos)
    {

        int puntuacionBase = 1; // 1 punto asegurado por ganar
        int puntosExtrasPorTiempo = 0;

        // 25 segundos
        // 1 minutos = 60 segundos
        // 1,5 minutos = 90 segundos
        if (tiempoSegundos < 25)
        {
            puntosExtrasPorTiempo = 3;
        }
        else if (tiempoSegundos < 60)
        {
            puntosExtrasPorTiempo = 2;
        }
        else if (tiempoSegundos < 90)
        {
            puntosExtrasPorTiempo = 1;
        }
        else if (tiempoSegundos >= 120) // Si tarda mas de 2 minutos no se le da ningun punto 
        {
            puntuacionBase = 0;
        }

        // Devolvemos la puntuación total sumando la puntuación base y los puntos extras por tiempo
        return puntuacionBase + puntosExtrasPorTiempo;

    }
    #endregion

    #region GESTIONAR LOGRO
    private async Task ProbarLogro(int idlogro)
    {
        bool mostrarLogro = await ApiSQLiteFAFA.InsertarLogroUsuario(idlogro, PerfilUidActual);

        if (mostrarLogro)
        {
            // Mostramos el cartel del logro desbloqueadoº
            Logro logro = ApiSQLiteFAFA.ExtraerLogroPorId(idlogro);
            _ = cartelLogro.MostrarLogro(logro.Nombre, logro.XpPremio);
        }
    }
    #endregion
}