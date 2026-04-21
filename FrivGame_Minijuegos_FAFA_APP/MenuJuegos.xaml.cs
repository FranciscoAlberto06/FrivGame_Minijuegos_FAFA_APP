using Microsoft.Maui.Controls.Shapes;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class MenuJuegos : ContentPage
{
	public MenuJuegos(int usuarioIniciado)
	{
		InitializeComponent();
        CrearMenuJuegos();
    }

    private void CrearMenuJuegos()
    {
        // 1.- Creamos arrays con los datos de cada juego para poder recorrerlos y crear los contenedores de cada juego
        string[] titulos = { "TOPOS", "WORDLE", "PAREJAS","2048" };
        string[] imagenes = { "portada_topos.png", "portada_adivinalapalabra.png", "portada_buscarparejas.png", "portada_2048.png" };
        Color[] colores = { Colors.Crimson, Colors.MediumSpringGreen, Colors.DeepSkyBlue, Colors.Yellow };


       
        for (int i = 0; i < titulos.Length; i++)
        {
            // 2.- Recorremos el array y por cada juego creamos un contenedor con su imagen y su nombre, y le aplicamos los efectos al entrar el puntero

            // 2.1- Sacamos el nombre del juego
            string nombreDelJuego = titulos[i];

            // 2.2- Creaccion del borde, el grid, el label, la imagen y su configuración de estilos y efectos
            Border contenedorBorder = new Border
            {
                Stroke = colores[i], // Color del borde
                StrokeThickness = 3, // Grosor del borde
                StrokeShape = new RoundRectangle { CornerRadius = 15 }, // Forma del borde 
                HeightRequest = 250,
                WidthRequest = 250,
                BackgroundColor = Colors.Black,
                Padding = 0,
                Margin = 10
            };

            // Tambien un grid para colocar la imagen y el nombre del juego uno encima de otro dentro del borde
            Grid contenidoInternoGrid = new Grid();

            Image foto = new Image
            {
                Source = imagenes[i],
                Aspect = Aspect.AspectFill,
                Opacity = 0.9
            };

            Label nombreLabel = new Label
            {
                Text = nombreDelJuego,
                TextColor = Colors.White,
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Opacity = 0 // lo ponemos a 0 para que no se vea al principio
            };


            // 3.- Los unimos en la interfaz, metiendo el label y la imagen en el grid, y el grid en el borde

            // Metemos el label y la imagen en el grid
            contenidoInternoGrid.Children.Add(foto);
            contenidoInternoGrid.Children.Add(nombreLabel);

            // Y lo metemos el grid  en su border
            contenedorBorder.Content = contenidoInternoGrid;


            //4.- Configuracion de efectos que queremos que tenga el border segun la plataforma
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI || DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst)
            {
                //Esta variable sirve para crear los ajustes segun la necesidades que tengamos del mouse
                PointerGestureRecognizer mouseGesture = new PointerGestureRecognizer();

                //Aqui configuramos el efecto que queremos que se aplique al entrar con el puntero sobre el contenedor del juego
                mouseGesture.PointerEntered += (object sender, PointerEventArgs e) =>
                {
                    // Creamos un efecto de que escale el border y degrade la foto y muestre el nombre del juego
                    contenedorBorder.ScaleTo(1.2, 100);
                    foto.FadeTo(0.2, 100);
                    nombreLabel.FadeTo(1, 100);
                };

                //Aqui configuramos especificamente el efecto al salir el puntero del contenedor, para que vuelva a su estado original
                mouseGesture.PointerExited += (object sender, PointerEventArgs e) =>
                {
                    // Volvemos a la escala original, quitamos el degradado de la foto y ocultamos el nombre del juego
                    contenedorBorder.ScaleTo(1.0, 100);
                    foto.FadeTo(0.9, 100);
                    nombreLabel.FadeTo(0, 100);
                };

                //Ahora lo configuramos para que se pueda hacer click  sobre el contenedor
                mouseGesture.PointerPressed += async (object sender, PointerEventArgs e) =>
                {
                    // Le pasamos el evento que querramos ejecutar
                    MoverAlJuego(nombreDelJuego);
                };


                //Una vez configurado los efectos que queremos, añadimos la configuracion a cada border del juego que queramos para que se apliquen
                contenedorBorder.GestureRecognizers.Add(mouseGesture);
            }
            // CONFIGURACION PARA MOVIL 
            else if (DeviceInfo.Current.Platform == DevicePlatform.Android || DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                // Esta varibale sirve para crear los ajustes segun la necesidades que tengamos del touch
                TapGestureRecognizer touchGesture = new TapGestureRecognizer();

                // Aqui configuramos el efecto que queremos que se aplique al tocar el contenedor del juego
                touchGesture.Tapped += async (s, e) => {
                    // En móvil, como no hay "hover", podemos hacer una pequeña 
                    // animación rápida de pulsación antes de navegar
                    await contenedorBorder.ScaleTo(0.95, 50);
                    await contenedorBorder.ScaleTo(1.0, 50);

                    MoverAlJuego(nombreDelJuego);
                };

                contenedorBorder.GestureRecognizers.Add(touchGesture);
            }

            // 5.- Y por ultimo, añadimos el border a la interfaz, colocandolo en la columna correspondiente segun el indice del juego
            ContenedorJuegos.Children.Add(contenedorBorder);
        }
    }

    private async void MoverAlJuego(string nombreDelJuego)
    {
        // Creamos un switch que nos dirija a la pagina del juego correspondiente segun el nombre del juego que se haya pulsado
        switch (nombreDelJuego)
        {
            case "TOPOS":
                Navigation.PushAsync(new JuegoTopos());
                break;
            case "WORDLE":
                Navigation.PushAsync(new AdivinarLaPalabra2());
                break;
            case "PAREJAS":
                Navigation.PushAsync(new SeleccionTemaParejas());
                break;
            case "2048":
                Navigation.PushAsync(new _2048game());
                break;
        }

    }
}