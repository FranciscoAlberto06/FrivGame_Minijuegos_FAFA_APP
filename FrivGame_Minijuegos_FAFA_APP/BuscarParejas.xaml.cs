namespace FrivGame_Minijuegos_FAFA_APP;

public partial class BuscarParejas : ContentPage    
{
    // Datos del juego
    ImageButton primeraCarta = null;
    int parejasEncontradas = 0;
    string temaActual = "";


    public BuscarParejas(string temaElegido)
    {
        InitializeComponent();
        temaActual = temaElegido;
        CrearTablero();
    }

    private List<string> BuscarImagenesTema()
    {
        List<string> listaImagenes = new List<string>();

        switch (temaActual)
        {
            case "Animales":
                listaImagenes = new List<string> { "gato.png", "perro.png", "conejo.png", "pollito.png", "cerdo.png", "capibara.png", "gato.png", "perro.png", "conejo.png", "pollito.png", "cerdo.png", "capibara.png" };
                break;
            case "Frutas":
                break;
            case "Banderas":
                listaImagenes = new List<string> { "alemania.png", "argentina.jpg", "espana.png", "brasil.png", "francia.png", "moriles.jpg", "alemania.png", "argentina.jpg", "espana.png", "brasil.png", "francia.png", "moriles.jpg" };
                break;
            case "SuperHeroes":
                break;
            case "Version Especial Abelin":
                listaImagenes = new List<string> { "gato1.png", "gato2.jpg", "gato3.jpg", "gato4.png", "gato5.jpg", "gato6.png", "gato1.png", "gato2.jpg", "gato3.jpg", "gato4.png", "gato5.jpg", "gato6.png" };
                break;
        }

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
                            LblEstado.Text = "¡HAS GANADO!";
                            LblEstado.TextColor = Colors.Green;

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
        GridTablero.Children.Clear(); // Limpiamos tablero si hay partida previa

        // Recargamos el tablero con el mismo tema
        CrearTablero();

        // Reseteamos el contador de parejas encontradas
        parejasEncontradas = 0;

        // Ocultamos el botón de reiniciar
        bReiniciar.IsVisible = false;

    }
}