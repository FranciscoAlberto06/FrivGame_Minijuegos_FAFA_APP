using BModelosFAFA;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class MenuInicioJuegos : ContentPage
{


  

    public MenuInicioJuegos()
	{
        InitializeComponent();

        List<Juego> listaJuegos = new List<Juego>

        {

        new Juego("TOPOS", "portada_topos.png"),

        new Juego("WORDLE", "portada_adivinalapalabra.png"),

        new Juego("PAREJAS", "portada_buscarparejas.png"),

        new Juego("2048", "portada_buscarparejas.png")

        };



        ColeccionJuegos.ItemsSource = listaJuegos;

    }


    // Metodo que se ejecutar al entrar con el mouse 
    private void MouseEntra(object sender, PointerEventArgs e)
    {
        // Buscamos los elementos dentro del Border que se ha activado el evento
        Border border = (Border)sender;
        Image foto = border.FindByName<Image>("foto");
        Label label = border.FindByName<Label>("nombreLabel");


        border.ScaleTo(1.2, 100);
        foto?.FadeTo(0.2, 100);
        label?.FadeTo(1, 100);
    }

    // Metodo que se ejecuta al salir con el mouse
    private void MouseSale(object sender, PointerEventArgs e)
    {
        // Buscamos los elementos dentro del Border que se ha activado el evento
        Border border = (Border)sender;
        Image foto = border.FindByName<Image>("foto");
        Label label = border.FindByName<Label>("nombreLabel");

        border.ScaleTo(1.0, 100);
        foto.FadeTo(0.9, 100);
        label.FadeTo(0, 100);
    }

    // Metodo que se ejecuta al hacer click con el mouse
    private void JuegoPresionado(object sender, PointerEventArgs e)
    {
        Border border = (Border)sender;
        Juego datos = (Juego)border.BindingContext;
        MoverAlJuego(datos.Titulo);
    }




    private async void MoverAlJuego(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return;

        switch (nombre)
        {
            case "TOPOS":
                await Navigation.PushAsync(new JuegoTopos());
                break;
            case "WORDLE":
                await Navigation.PushAsync(new AdivinarLaPalabra2());
                break;
            case "PAREJAS":
                await Navigation.PushAsync(new SeleccionTemaParejas());
                break;
                // Añade aquí el resto de casos como el de "2048"
        }
    }
}