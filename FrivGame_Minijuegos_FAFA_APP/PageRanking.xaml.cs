using BGestionFAFA;
using BModelosFAFA;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class PageRanking : ContentPage
{
	string uIdPerfilActual;
    List<Juego> listaJuegos; // Clase sencilla para manejar el ID y Nombre del juego

    public PageRanking(string uIdPerfil)
	{
		InitializeComponent();
        uIdPerfilActual = uIdPerfil;
        CargarSelectorJuegos();
    }

    private void CargarSelectorJuegos()
    {
        // 1. Sacamos de la bd los juegos disponibles 
        listaJuegos = ApiSQLiteFAFA.ExtraerTodosLosJuegos();

        // 2. Asignamos los nombres al Picker
        pickerJuegos.ItemsSource = listaJuegos.Select(j => j.Nombre).ToList();

        // 3. Seleccionamos el primero por defecto, esto activara el evento de cambio y cargara el ranking del primer juego
        pickerJuegos.SelectedIndex = 0;
    }

    private void OnJuegoCambiado(object sender, EventArgs e)
    {
        int indice = pickerJuegos.SelectedIndex;
        if (indice == -1) return;

        // Obtenemos el ID real del juego basado en la selección
        int idJuegoSeleccionado = listaJuegos[indice].IdJuego;

        ActualizarRanking(idJuegoSeleccionado);
    }

    private void ActualizarRanking(int idJuego)
    {
        // 1. Extraemos las mejores marcas usando tu método de la API
        List<Partida> listaRankings = ApiSQLiteFAFA.ExtraerrMejoresMarcasPorJuego(idJuego);

        // 2. Procesamos la lista para aplicar colores de "resaltado"
        foreach (Partida partida in listaRankings)
        {
            partida.NombreUsuario = ApiSQLiteFAFA.ExtraerNombrePerfilPorIdPerfil(partida.IdPerfil);

            if (partida.IdPerfil == uIdPerfilActual)
            {
                // Si soy yo: Fondo rojo oscuro y borde carmesí
                partida.ColorFondoRanking = "#3A0000";
            }
            else
            {
                // Si es otro: Fondo gris oscuro y sin borde
                partida.ColorFondoRanking = "#252525";
            }
        }

        // 3. Mandamos la lista al CollectionView
        miCollectionView.ItemsSource = listaRankings;
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

}