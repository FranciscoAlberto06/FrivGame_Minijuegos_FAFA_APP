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
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await ApiRestFAFA.CargarPartidasPorJuegoDesdeNube(idJuego);
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

        CargarSelectorJuegos();
        Task.Run(async () =>
        {
            await CargarPartidasNuevas();
        });
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        if (_conexionHub != null)
            await _conexionHub.StopAsync();
    }
    #endregion

    private async void CargarSelectorJuegos()
    {
        // 1. Sacamos de la bd los juegos disponibles 
        listaJuegos = ApiSQLiteFAFA.ExtraerTodosLosJuegos();

        // 2. Asignamos los nombres al Picker
        pickerJuegos.ItemsSource = listaJuegos.Select(j => j.Nombre).ToList();

        // 3. Seleccionamos el primero por defecto, esto activara el evento de cambio y cargara el ranking del primer juego
        pickerJuegos.SelectedIndex = 0;


    }

    private async Task CargarPartidasNuevas()
    {
        // 2. Si tenemos internet, cargamos de la nube y refrescamos, esto nos asegura que el ranking este actualizado con los datos de otros usuarios
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            // 2.2 Cargamos de la nube las partidas del juego seleccionados
            List<PartidaSQL> partidas = await ApiRestFAFA.CargarPartidasDesdeNube();
            ApiSQLiteFAFA.GuardarPartidasEnLocal(partidas);

            List<Perfil> perfiles = await ApiRestFAFA.CargarPerfilesDesdeNube();
            ApiSQLiteFAFA.GuardarPerfilesEnLocal(perfiles);

        }
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

        // 1. Cargamos lo que tengamos en local para ese juego
        List<Partida> listaRankings = ApiSQLiteFAFA.ExtraerrMejoresMarcasPorJuego(idJuego);

        // 2. Cargamos las puntuacion de cada usuario
        for (int i = 0; i < listaRankings.Count();i++)
        {
            listaRankings[i].NombreUsuario = ApiSQLiteFAFA.ExtraerNombrePerfilPorIdPerfil(listaRankings[i].IdPerfil);
            listaRankings[i].ColorFondoRanking = listaRankings[i].IdPerfil == uIdPerfilActual ? "#3A0000" : "#252525";
            if (i == 0)
                listaRankings[i].PuestoRanking = "🥇";
            else if (i == 1)
                listaRankings[i].PuestoRanking = "🥈";
            else if (i == 2)
                listaRankings[i].PuestoRanking = "🥉";
            else
                listaRankings[i].PuestoRanking = "🏆";
        }

        // 3. Asignamos a la vista
        miCollectionView.ItemsSource = listaRankings;

    
    }

   

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

}