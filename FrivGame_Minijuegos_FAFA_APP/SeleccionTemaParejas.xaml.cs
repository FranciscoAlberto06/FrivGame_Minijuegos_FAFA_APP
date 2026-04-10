namespace FrivGame_Minijuegos_FAFA_APP;

public partial class SeleccionTemaParejas : ContentPage
{
	public SeleccionTemaParejas()
	{
		InitializeComponent();
	}

    private async void OnTemaClicked(object sender, EventArgs e)
    {

		Button botonPulsado = (Button)sender;

        // Lo mandamos a la pagina de busqueda de parejas, pasando el texto del botón como parámetro para conocer el tema seleccionado en la otra pagina
        await Navigation.PushAsync(new BuscarParejas(botonPulsado.Text));



    }
}