using API.Hubs;
using BModelosFAFA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MySqlConnector;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartidaController : ControllerBase
    {
        private readonly string _connString;
        private readonly IHubContext<RankingHub> _hubContext;

        public PartidaController(IConfiguration config, IHubContext<RankingHub> hubContext)
        {
            _connString = config.GetConnectionString("MySQL");
            _hubContext = hubContext;
        }

        // GET api/partida/ranking?idJuego=1
        [HttpGet("ranking")]
        public async Task<IActionResult> GetRanking([FromQuery] int idJuego)
        {
            List<Partida> partidas = new List<Partida>();

            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            string sql = "SELECT * FROM PARTIDA WHERE id_juego = @idJ ORDER BY puntuacion DESC";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idJ", idJuego);

            using MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                partidas.Add(new Partida
                {
                    IdJuego = reader.GetInt32("id_juego"),
                    IdPerfil = reader.GetString("id_perfil"),
                    Puntuacion = reader.GetInt32("puntuacion"),
                    TiempoSegundos = reader.GetInt32("tiempo_segundos"),
                    Victoria = reader.GetBoolean("victoria"),
                });
            }

            return Ok(partidas);
        }

        // POST api/partida
        [HttpPost]
        public async Task<IActionResult> InsertarPartida([FromBody] Partida partida)
        {
            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            string sql = @"INSERT INTO PARTIDA (id_juego, id_perfil, puntuacion, tiempo_segundos, victoria, fecha_hora)
                           VALUES (@idJ, @idP, @pts, @time, @vic, @fecha)";

            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idJ", partida.IdJuego);
            cmd.Parameters.AddWithValue("@idP", partida.IdPerfil);
            cmd.Parameters.AddWithValue("@pts", partida.Puntuacion);
            cmd.Parameters.AddWithValue("@time", partida.TiempoSegundos);
            cmd.Parameters.AddWithValue("@vic", partida.Victoria);

            await cmd.ExecuteNonQueryAsync();
            int idNuevo = (int)cmd.LastInsertedId;

            // ✅ Notificamos a todos los dispositivos conectados
            await _hubContext.Clients.All.SendAsync("RankingActualizado", partida.IdJuego);

            return Ok(idNuevo);
        }
    }

}
