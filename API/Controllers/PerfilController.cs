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

        // PUT api/perfil/modificar-nombre
        [HttpPut("modificar-nombre")]
        public async Task<IActionResult> ModificarNombre([FromBody] Perfil perfil)
        {
            try
            {
                using MySqlConnection conn = new MySqlConnection(_connString);
                await conn.OpenAsync();

                // 1. Validar que el nuevo nombre no esté siendo usado por OTRO usuario
                // Buscamos en la tabla USUARIO excluyendo al usuario actual
                string checkQuery = "SELECT COUNT(*) FROM USUARIO WHERE nombre_usuario = @nom AND id_usuario != @idUsu";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@nom", perfil.NombreUsuario);
                    checkCmd.Parameters.AddWithValue("@idUsu", perfil.IdUsuario);
                    long existe = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());

                    if (existe > 0)
                    {
                        return BadRequest("El nombre de usuario ya está en uso por otro jugador.");
                    }
                }

                // 2. Iniciamos una Transacción para asegurar que se cambie en ambos sitios o en ninguno
                using MySqlTransaction transaccion = await conn.BeginTransactionAsync();

                try
                {
                    // A. Actualizar la tabla PERFIL
                    string queryPerfil = @"UPDATE PERFIL 
                                   SET nombre_usuario = @nom 
                                   WHERE perfil_uid = @uid";

                    using MySqlCommand cmdPerfil = new MySqlCommand(queryPerfil, conn, transaccion);
                    cmdPerfil.Parameters.AddWithValue("@nom", perfil.NombreUsuario);
                    cmdPerfil.Parameters.AddWithValue("@uid", perfil.PerfilUid);
                    await cmdPerfil.ExecuteNonQueryAsync();

                    // B. Actualizar la tabla USUARIO
                    // Asumo que tu tabla se llama USUARIO, y los campos son id_usuario y nombre_usuario (ajústalos si varían)
                    string queryUsuario = @"UPDATE USUARIO 
                                    SET username = @nom 
                                    WHERE id_usuario = @idUsu";

                    using MySqlCommand cmdUsuario = new MySqlCommand(queryUsuario, conn, transaccion);
                    cmdUsuario.Parameters.AddWithValue("@nom", perfil.NombreUsuario);
                    cmdUsuario.Parameters.AddWithValue("@idUsu", perfil.IdUsuario);
                    await cmdUsuario.ExecuteNonQueryAsync();

                    // 3. Si ambos comandos se ejecutaron sin problemas, confirmamos los cambios en la BD
                    await transaccion.CommitAsync();

                    return Ok();
                }
                catch (Exception ex)
                {
                    // Si algo falla a mitad de camino, deshacemos todo para evitar datos corruptos
                    await transaccion.RollbackAsync();
                    return StatusCode(500, $"Error al ejecutar la transacción en la base de datos: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

    }
}
