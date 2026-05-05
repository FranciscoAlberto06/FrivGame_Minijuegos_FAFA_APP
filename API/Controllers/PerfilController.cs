using BModelosFAFA;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerfilController : ControllerBase
    {
        private readonly string _connString;

        public PerfilController(IConfiguration config)
        {
            _connString = config.GetConnectionString("MySQL");
        }

        // POST api/perfil/insertar
        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarPerfil([FromBody] Perfil perfil)
        {
            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            string query = @"INSERT INTO PERFIL (perfil_uid, id_usuario, nombre_usuario, nivel, xp_total, avatar_url) 
                             VALUES (@uid, @idUsu, @nom, @niv, @xp, @ava)";

            using MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", perfil.PerfilUid);
            cmd.Parameters.AddWithValue("@idUsu", perfil.IdUsuario);
            cmd.Parameters.AddWithValue("@nom", perfil.NombreUsuario);
            cmd.Parameters.AddWithValue("@niv", 1);
            cmd.Parameters.AddWithValue("@xp", 0);
            cmd.Parameters.AddWithValue("@ava", string.IsNullOrEmpty(perfil.AvatarUrl) ? (object)DBNull.Value : perfil.AvatarUrl);

            await cmd.ExecuteNonQueryAsync();
            return Ok();
        }

        // PUT api/perfil/sincronizar
        [HttpPut("sincronizar")]
        public async Task<IActionResult> SincronizarPerfiles([FromBody] List<Perfil> perfiles)
        {
            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            foreach (Perfil perfil in perfiles)
            {
                string query = @"INSERT INTO PERFIL (perfil_uid, id_usuario, nombre_usuario, nivel, xp_total, avatar_url) 
                                 VALUES (@uid, @idU, @nom, @lvl, @xp, @ava) 
                                 ON DUPLICATE KEY UPDATE nivel = @lvl, xp_total = @xp, avatar_url = @ava, nombre_usuario = @nom";

                using MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@uid", perfil.PerfilUid);
                cmd.Parameters.AddWithValue("@idU", perfil.IdUsuario);
                cmd.Parameters.AddWithValue("@nom", perfil.NombreUsuario);
                cmd.Parameters.AddWithValue("@lvl", perfil.Nivel);
                cmd.Parameters.AddWithValue("@xp", perfil.XpTotal);
                cmd.Parameters.AddWithValue("@ava", perfil.AvatarUrl ?? "");
                await cmd.ExecuteNonQueryAsync();
            }
            return Ok();
        }

        // GET api/perfil/todos
        [HttpGet("todos")]
        public async Task<IActionResult> GetTodos()
        {
            List<Perfil> perfiles = new List<Perfil>();
            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            string sql = "SELECT * FROM PERFIL";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            using MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                perfiles.Add(new Perfil
                {
                    PerfilUid = reader.GetString("perfil_uid"),
                    IdUsuario = reader.GetInt32("id_usuario"),
                    NombreUsuario = reader.GetString("nombre_usuario"),
                    Nivel = reader.GetInt32("nivel"),
                    XpTotal = reader.GetInt32("xp_total"),
                    AvatarUrl = reader.IsDBNull(reader.GetOrdinal("avatar_url")) ? "" : reader.GetString("avatar_url")
                });
            }
            return Ok(perfiles);
        }
    }
}
