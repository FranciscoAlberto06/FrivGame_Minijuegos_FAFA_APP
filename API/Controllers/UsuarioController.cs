using BModelosFAFA;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly string _connString;

        public UsuarioController(IConfiguration config)
        {
            _connString = config.GetConnectionString("MySQL");
        }

        // POST api/usuario/insertar
        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarUsuario([FromBody] Usuario usuario)
        {
            try
            {
                using MySqlConnection conn = new MySqlConnection(_connString);
                await conn.OpenAsync();

                // 2. Comprobamos si ya existe ese email
                //string sqlCheckEmail = "SELECT COUNT(*) FROM USUARIO WHERE email = @e";
                //using MySqlCommand cmdEmail = new MySqlCommand(sqlCheckEmail, conn);
                //cmdEmail.Parameters.AddWithValue("@e", usuario.Email);
                //int emailExiste = Convert.ToInt32(await cmdEmail.ExecuteScalarAsync());
                //if (emailExiste > 0)
                //    return BadRequest("ERROR: El correo electronico ya esta registrado.");

                //// 3. Comprobamos si ya existe ese nombre de usuario
                //string sqlCheckUser = "SELECT COUNT(*) FROM USUARIO WHERE username = @u";
                //using MySqlCommand cmdUser = new MySqlCommand(sqlCheckUser, conn);
                //cmdUser.Parameters.AddWithValue("@u", usuario.NombreUsuario);
                //int userExiste = Convert.ToInt32(await cmdUser.ExecuteScalarAsync());
                //if (userExiste > 0)
                //    return BadRequest("ERROR: El nombre de usuario ya esta registrado.");

                string sql = "INSERT INTO USUARIO (email, username, password_hash) VALUES (@e, @u, @p)";
                using MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@e", usuario.Email);
                cmd.Parameters.AddWithValue("@u", usuario.NombreUsuario);
                cmd.Parameters.AddWithValue("@p", GenerarHash(usuario.Password));
                await cmd.ExecuteNonQueryAsync();

                return Ok((int)cmd.LastInsertedId);



            }
            catch(Exception error)
            {
                return StatusCode(500, new { mensaje = error.Message });

            }

        }

        // GET api/usuario/login?email=x
        [HttpGet("login")]
        public async Task<IActionResult> Login([FromQuery] string email)
        {
            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            string sql = "SELECT id_usuario, username, email, password_hash FROM USUARIO WHERE email = @email LIMIT 1";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);

            using MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Ok(new Usuario
                {
                    IdUsuario = reader.GetInt32("id_usuario"),
                    NombreUsuario = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Password = reader.GetString("password_hash")
                });
            }
            return NotFound();
        }

        // PUT api/usuario/sincronizar
        [HttpPut("sincronizar")]
        public async Task<IActionResult> SincronizarUsuarios([FromBody] List<Usuario> usuarios)
        {
            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            foreach (Usuario user in usuarios)
            {
                string passHash = GenerarHash(user.Password);
                string query = @"INSERT INTO USUARIO (username, email, password_hash) 
                                 VALUES (@user, @email, @pass) 
                                 ON DUPLICATE KEY UPDATE password_hash = @pass";

                using MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@user", user.NombreUsuario ?? "Usuario");
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@pass", passHash);
                await cmd.ExecuteNonQueryAsync();
            }
            return Ok();
        }

        // GET api/usuario/todos
        [HttpGet("todos")]
        public async Task<IActionResult> GetTodos()
        {
            List<Usuario> usuarios = new List<Usuario>();
            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            string sql = "SELECT * FROM USUARIO";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            using MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                usuarios.Add(new Usuario
                {
                    IdUsuario = reader.GetInt32("id_usuario"),
                    NombreUsuario = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Password = reader.GetString("password_hash")
                });
            }
            return Ok(usuarios);
        }

        private string GenerarHash(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}
