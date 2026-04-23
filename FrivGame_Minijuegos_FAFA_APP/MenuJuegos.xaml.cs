using BGestionFAFA;
using BModelosFAFA;
using BModelosSQLFAFA;
using Microsoft.Maui.Controls.Shapes;
using System.Security.Cryptography;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class MenuJuegos : ContentPage
{
    Perfil PerfilActual = new Perfil();

    // Variable que local que no va a decir en que plataforma nos encontramos
    bool esWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;


    public MenuJuegos(int usuarioIniciado)
	{
		InitializeComponent();
        // Hacemos accesible id del usuario inicado desde cualquier luegar del codigo
        PerfilActual = new Perfil();
        PerfilActual.IdUsuario = usuarioIniciado;
        CrearMenuJuegos();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // TODO: Implementar ajuste de perfil de usuario
        // Lo ponemos aqui para que siempre se actualice el estaod del perfil
        CargarPerfil(PerfilActual.IdUsuario);
    }

    private void CargarPerfil(int idUsuIniciado)
    {
        // Sacamos el perfil 
        PerfilActual = ApiSQLiteFAFA.ExtraerPerfilPorId(idUsuIniciado);

        // metemos experiencia para probar nuestro progressbar
        PerfilActual.XpTotal = 3127;

        // Mandamos el unico perfil que va a haber siempre,a nuestro biding context
        this.BindingContext = PerfilActual;

    }

    private void CrearMenuJuegos()
    {

        // 1.- Obtenemos lista de juegos existente en la base de datos
        List<Juego> listaJuegos = ApiSQLiteFAFA.ExtraerTodosLosJuegos();

        // 2.- Recorremos la lista de objetos Juego
        foreach (Juego juego in listaJuegos)
        {
            // 2.1- Sacamos los datos directamente del modelo Juego
            string nombreDelJuego = juego.Nombre;
            string rutaImagen = juego.ImagenURL; // Usamos el nombre que tenemos nuestra clase Juego

            // Sacamos el color de borde que va a tener
            Color colorDelBorde = Color.FromArgb(juego.ColorHex);

            // 2.2- Creación del borde
            Border contenedorBorder = new Border
            {
                Stroke = colorDelBorde,
                StrokeThickness = 3,
                StrokeShape = new RoundRectangle { CornerRadius = 15 },
                HeightRequest = 250,
                WidthRequest = 250,
                BackgroundColor = Colors.Black,
                Padding = 0,
                Margin = 10
            };

            Grid contenidoInternoGrid = new Grid();

            Image foto = new Image
            {
                Source = rutaImagen,
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
                Opacity = 0
            };

            // 3.- Los unimos en la interfaz, metiendo el label y la imagen en el grid, y el grid en el borde

            // Metemos el label y la imagen en el grid
            contenidoInternoGrid.Children.Add(foto);
            contenidoInternoGrid.Children.Add(nombreLabel);

            // Y lo metemos el grid  en su border
            contenedorBorder.Content = contenidoInternoGrid;

            #region CONFIGURACION DE EFECTO SEGUN LA PLATAFORMA
            //4.- Configuracion de efectos que queremos que tenga el border segun la plataforma
            if (esWindows)
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
            else 
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
            #endregion

            // 5.- Y por ultimo, añadimos el border a la interfaz, colocandolo en la columna correspondiente segun el indice del juego
            ContenedorJuegos.Children.Add(contenedorBorder);
        }
    }

    private async void MoverAlJuego(string nombreDelJuego)
    {
        // Creamos un switch que nos dirija a la pagina del juego correspondiente segun el nombre del juego que se haya pulsado
        // Pasandole el id del perfil actual, y id del juego
        switch (nombreDelJuego)
        {
            case "TOPOS":
                await Navigation.PushAsync(new JuegoTopos(PerfilActual.PerfilUid));
                break;
            case "WORDLE":
                await Navigation.PushAsync(new AdivinarLaPalabra2(PerfilActual.PerfilUid));
                break;
            case "PAREJAS":
                await Navigation.PushAsync(new SeleccionTemaParejas(PerfilActual.PerfilUid));
                break;
            case "2048":
                await Navigation.PushAsync(new _2048game(PerfilActual.PerfilUid));
                break;
        }

    }


    private async void OnRankingGlobalClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PageRanking(PerfilActual.PerfilUid));

    }
}