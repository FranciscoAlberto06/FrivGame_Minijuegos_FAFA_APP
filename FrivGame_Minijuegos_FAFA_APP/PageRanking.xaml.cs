using BGestionFAFA;
using BModelosFAFA;
using BModelosSQLFAFA;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.SignalR.Client;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class PageRanking : ContentPage
{
	string uIdPerfilActual;
    List<Juego> listaJuegos; // para manejar el ID y Nombre del juego
    HubConnection _conexionHub;


    public PageRanking(string uIdPerfil)
	{
		InitializeComponent();
        uIdPerfilActual = uIdPerfil;
        CargarSelectorJuegos();
    }


    #region CONFIGURACION DE HUB PARA SINCRONIZACION EN TIEMPO REAL
    protected override async void OnAppearing()
    {
        base.OnAppearing();

      
        string urlHub = "https://frivgameminijuegosfafaapp-production.up.railway.app/rankingHub";
       

        // Conectamos al hub de SignalR
        _conexionHub = new HubConnectionBuilder()
            .WithUrl(urlHub)
            .WithAutomaticReconnect()
            .Build();

        // Cuando la API avise de ranking actualizado
        _conexionHub.On<int>("RankingActualizado", (idJuego) =>
        {
            // Revisamos que el juego seleccionado sea el que hay que actualizar
            int indice = pickerJuegos.SelectedIndex;
            if (indice != -1 && listaJuegos[indice].IdJuego == idJuego)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ActualizarRanking(idJuego);
                });
            }

        });


        try
        {
            
            await _conexionHub.StartAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error SignalR: {ex.Message}");
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        if (_conexionHub != null)
            await _conexionHub.StopAsync();
    }
    #endregion

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

    private async void ActualizarRanking(int idJuego)
    {
        // 1. Recargarmo el sqlite primero para asegurarnos de tener los datos mas recientes
        if(Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            List<PartidaSQL> partidas = await ApiRestFAFA.CargarPartidasDesdeNube();
            ApiSQLiteFAFA.GuardarPartidasEnLocal(partidas);

            // tambien los perfiles por si es un usu nuevo que ha entrado en el ranking
            List<Perfil> perfiles = await ApiRestFAFA.CargarPerfilesDesdeNube();
            ApiSQLiteFAFA.GuardarPerfilesEnLocal(perfiles);

        }
        // 1. Extraemos las mejores marcas usando el método de la API
        List<Partida> listaRankings = ApiSQLiteFAFA.ExtraerrMejoresMarcasPorJuego(idJuego);

        // 2. Procesamos la lista para aplicar colores de resaltado
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