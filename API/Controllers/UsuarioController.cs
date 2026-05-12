using API.Hubs;
using BModelosFAFA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MySqlConnector;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly string _connString;
        private readonly IHubContext<RankingHub> _hubContext;
        private readonly IConfiguration _config;

        // (codigo correcto, fecha expiracion, usuario completo)
        private static List<(string email, string codigo, DateTime expiracion, Usuario usuario)> _codigosPendientes = new();


        public UsuarioController(IConfiguration config, IHubContext<RankingHub> hubContext)
        {
            _connString = config.GetConnectionString("MySQL");
            _hubContext = hubContext;
            _config = config;
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

                #region GENERAR CODIGOS DE VERIFICACION Y ENVIAR EMAIL
                // Generamos 3 codigos aleatorios de 6 digitos
                Random rnd = new Random();
                string codigoCorrecto = rnd.Next(100000, 999999).ToString();
                string codigo2 = rnd.Next(100000, 999999).ToString();
                string codigo3 = rnd.Next(100000, 999999).ToString();

                // Los mezclamos para que el correcto no siempre esté en la misma posición
                List<string> opciones = new List<string> { codigoCorrecto, codigo2, codigo3 }
                                        .ToList();

                // Eliminamos cualquier codigo pendiente anterior para ese email (si el usuario ya había intentado registrarse antes)
                _codigosPendientes.RemoveAll(x => x.email == usuario.Email);

                // Añadimos la opcion correcta a la lista de pendientes con su fecha de expiracion (15 minutos) y el usuario completo
                _codigosPendientes.Add((usuario.Email, codigoCorrecto, DateTime.Now.AddMinutes(15), usuario));

                // Enviamos el email con el codigo correcto
                await EnviarEmailVerificacion(usuario.Email, codigoCorrecto);
                #endregion

                // Delvomemos al proyecto las opciones
                return Ok(opciones);



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


        // POST api/usuario/verificar
        [HttpPost("verificar")]
        public async Task<IActionResult> Verificar([FromBody] Verificacion infoSelec)
        {
            try
            {
                (string email, string codigo, DateTime expiracion, Usuario usuario) datos = _codigosPendientes.FirstOrDefault(x => x.email == infoSelec.Email);

                if (datos == default)
                    return StatusCode(400, "No hay ningún código pendiente para este email.");

                if (datos.expiracion < DateTime.Now)
                {
                    _codigosPendientes.RemoveAll(x => x.email == infoSelec.Email);
                    return StatusCode(400, "El código ha expirado. Vuelve a registrarte.");
                }

                if (datos.codigo != infoSelec.CodigoSelec)
                    return StatusCode(400, "Código incorrecto.");

                _codigosPendientes.RemoveAll(x => x.email == infoSelec.Email);

                using MySqlConnection conn = new MySqlConnection(_connString);
                await conn.OpenAsync();

                string sql = "INSERT INTO USUARIO (email, username, password_hash) VALUES (@e, @u, @p)";
                using MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@e", datos.usuario.Email);
                cmd.Parameters.AddWithValue("@u", datos.usuario.NombreUsuario);
                cmd.Parameters.AddWithValue("@p", GenerarHash(datos.usuario.Password));
                await cmd.ExecuteNonQueryAsync();

                return Ok((int)cmd.LastInsertedId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }


        private string GenerarHash(string password)
        {

            using SHA256 sha256 = SHA256.Create();

            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            StringBuilder builder = new StringBuilder();

            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        private async Task EnviarEmailVerificacion(string email, string codigo)
        {
            string apiKey = _config["SendGrid:ApiKey"];
            SendGridClient client = new SendGridClient(apiKey);

            SendGridMessage msg = new SendGridMessage
            {
                From = new EmailAddress("tu@email.com", "FrivGame"),
                Subject = "Verifica tu cuenta en FrivGame",
                HtmlContent = $@"
            <h2>¡Bienvenido a FrivGame!</h2>
            <p>Tu código de verificación es:</p>
            <h1 style='color: #10B981; font-size: 48px;'>{codigo}</h1>
            <p>Caduca en 15 minutos.</p>
        "
            };
            msg.AddTo(new EmailAddress(email));
            await client.SendEmailAsync(msg);
        }


    }
}
