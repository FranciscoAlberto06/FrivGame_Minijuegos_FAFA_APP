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
                cmd.Parameters.AddWithValue("@lvl", 1);
                cmd.Parameters.AddWithValue("@xp", 0);
                cmd.Parameters.AddWithValue("@ava", perfil.AvatarUrl ?? "");
                await cmd.ExecuteNonQueryAsync();
            }
            return Ok();
        }

        // PUT api/perfil/sincronizar-nombre
        [HttpPut("sincronizar-nombre")]
        public async Task<IActionResult> SincronizarNombre([FromBody] Perfil perfilACambiar)
        {
            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            // 1. Iniciamos una transacción para que se actualicen ambas tablas o ninguna
            using MySqlTransaction trans = await conn.BeginTransactionAsync();

            try
            {
                // 2. Validar que OTRA persona no tenga ya ese nombre de usuario en MySQL
                // Buscamos si existe el nombre, pero ignoramos el perfil del usuario actual (perfilUid)
                string sqlCheckUser = "SELECT COUNT(*) FROM PERFIL WHERE nombre_usuario = @nom AND perfil_uid != @uid";
                using (MySqlCommand cmdCheck = new MySqlCommand(sqlCheckUser, conn, trans))
                {
                    cmdCheck.Parameters.AddWithValue("@nom", perfilACambiar.NombreUsuario);
                    cmdCheck.Parameters.AddWithValue("@uid", perfilACambiar.PerfilUid);

                    int userExiste = Convert.ToInt32(await cmdCheck.ExecuteScalarAsync());
                    if (userExiste > 0)
                    {
                        // Si ya existe en otra cuenta, cancelamos la transacción y avisamos al móvil
                        return StatusCode(400, "ERROR: El nombre de usuario ya está registrado por otra persona.");
                    }
                }

                // 3. Si el nombre está libre, actualizamos la tabla USUARIO (Padre)
                // (Nota: Asegúrate de si tu columna en MySQL es 'username' o 'nombre_usuario')
                string sqlUsuario = "UPDATE USUARIO SET username = @nom WHERE id_usuario = @id";
                using (MySqlCommand cmdUsuario = new MySqlCommand(sqlUsuario, conn, trans))
                {
                    cmdUsuario.Parameters.AddWithValue("@nom", perfilACambiar.NombreUsuario);
                    cmdUsuario.Parameters.AddWithValue("@id", perfilACambiar.IdUsuario);
                    await cmdUsuario.ExecuteNonQueryAsync();
                }

                // 4. Actualizamos la tabla PERFIL (Hijo)
                string sqlPerfil = "UPDATE PERFIL SET nombre_usuario = @nom WHERE perfil_uid = @uid";
                using (MySqlCommand cmdPerfil = new MySqlCommand(sqlPerfil, conn, trans))
                {
                    cmdPerfil.Parameters.AddWithValue("@nom", perfilACambiar.NombreUsuario);
                    cmdPerfil.Parameters.AddWithValue("@uid", perfilACambiar.PerfilUid);
                    await cmdPerfil.ExecuteNonQueryAsync();
                }

                // 5. Si todo ha ido bien de manera atómica, confirmamos los cambios en MySQL
                await trans.CommitAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                // Si hay cualquier error de conexión o SQL, deshacemos todo para no dejar datos inconsistentes
                await trans.RollbackAsync();
                return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
            }
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
