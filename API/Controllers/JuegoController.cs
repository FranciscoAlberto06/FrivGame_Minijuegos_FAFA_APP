using BModelosFAFA;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JuegoController : ControllerBase
    {
        private readonly string _connString;

        public JuegoController(IConfiguration config)
        {
            _connString = config.GetConnectionString("MySQL");
        }

        // GET api/juego/todos
        [HttpGet("todos")]
        public async Task<IActionResult> GetTodos()
        {
            List<Juego> juegos = new List<Juego>();

            using MySqlConnection conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            string sql = "SELECT * FROM JUEGO";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            using MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                juegos.Add(new Juego
                {
                    IdJuego = reader.GetInt32("id_juego"),
                    Nombre = reader.GetString("nombre"),
                    ImagenURL = reader.GetString("imagen_url"),
                    ColorHex = reader.GetString("color_hex")
                });
            }

            return Ok(juegos);
        }
    }
}
