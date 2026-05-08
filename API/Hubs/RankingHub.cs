using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class RankingHub : Hub
    {

        public async Task NotificarRankingActualizado(int idJuego)
        {
            // Notifica a todos los que haya conectados en ese juego
            await Clients.All.SendAsync("RankingActualizado", idJuego);
        }

    }
}
