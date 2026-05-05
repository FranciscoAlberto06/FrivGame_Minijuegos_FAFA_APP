using BModelosFAFA;
using BModelosSQLFAFA;

using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogroController : ControllerBase
    {
        private readonly string _connString;

        public LogroController(IConfiguration config)
        {
            _connString = config.GetConnectionString("MySQL");
        }

        // GET api/logro/todos
        [HttpGet("todos")]
        public async Task<IActionResult> GetTodos()
        {
            List<Logro> logros = new List<Logro>();
            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            string sql = "SELECT * FROM LOGRO";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            using MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                logros.Add(new Logro
                {
                    IdLogro = reader.GetInt32("id_logro"),
                    IdJuego = reader.GetInt32("id_juego"),
                    Nombre = reader.GetString("nombre"),
                    CondicionDesbloqueo = reader.GetString("condicion_desbloqueo"),
                    XpPremio = reader.GetInt32("xp_premio")
                });
            }
            return Ok(logros);
        }

        // GET api/logro/todos-usuario-logros
        [HttpGet("todos-usuario-logros")]
        public async Task<IActionResult> GetTodosUsuarioLogros()
        {
            List<PerfilLogroSQL> logros = new List<PerfilLogroSQL>();
            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            string sql = "SELECT * FROM USUARIO_LOGRO";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            using MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                logros.Add(new PerfilLogroSQL
                {
                    Id = reader.GetInt32("id"),
                    IdPerfil = reader.GetString("perfil_uid"),
                    IdLogro = reader.GetInt32("id_logro"),
                    FechaObtencion = reader.GetDateTime("fecha_obtencion")
                });
            }
            return Ok(logros);
        }

        // POST api/logro/sincronizar
        [HttpPost("sincronizar")]
        public async Task<IActionResult> SincronizarLogros([FromBody] List<PerfilLogroSQL> logros)
        {
            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            foreach (PerfilLogroSQL logro in logros)
            {
                string query = @"INSERT INTO USUARIO_LOGRO (perfil_uid, id_logro, fecha_obtencion) 
                                 VALUES (@uid, @idL, @fecha)
                                 ON DUPLICATE KEY UPDATE fecha_obtencion = @fecha";

                using MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@uid", logro.IdPerfil);
                cmd.Parameters.AddWithValue("@idL", logro.IdLogro);
                cmd.Parameters.AddWithValue("@fecha", logro.FechaObtencion);
                await cmd.ExecuteNonQueryAsync();
            }
            return Ok();
        }
    }
}
