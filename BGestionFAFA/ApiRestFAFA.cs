using BModelosFAFA;
using BModelosSQLFAFA;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BGestionFAFA
{
    public class ApiRestFAFA
    {
        //private static string _urlBase = "https://localhost:7087";

        private static string _urlBase = "https://frivgameminijuegosfafaapp-production.up.railway.app/api";
        private static HttpClient _http = new HttpClient();

        #region USUARIO
        public static async Task<int> InsertarUsuarioEnNube(string email, string user, string pass)
        {
            int idGenerado;

            // 1.- Creamos el objeto Usuario
            Usuario usuario = new Usuario
            {
                Email = email,
                NombreUsuario = user,
                Password = pass
            };

            // 2.- Lo convertimos a JSON y lo mandamos a la API por POST
            HttpResponseMessage response = await _http.PostAsJsonAsync($"{_urlBase}/usuario/insertar", usuario);

            // 3.- Si la api devuelve un badrequest lanzamos una excepción con el mensaje de error que nos dio la API
            //string contenido = await response.Content.ReadAsStringAsync();

            //if (!response.IsSuccessStatusCode)
            //{
            //    try
            //    {
            //        var json = System.Text.Json.JsonDocument.Parse(contenido);
            //        if (json.RootElement.TryGetProperty("mensaje", out var msg))
            //            throw new Exception(msg.GetString());
            //    }
            //    catch { }

            //    throw new Exception(contenido.Trim('"'));
            //}

            // 4.- Leemos la respuesta de la API (el ID que genero la BD)
            string resultado = await response.Content.ReadAsStringAsync();

            // 5.- Convierto ese string a int y lo devolvemos
            idGenerado = Convert.ToInt32(resultado);

            return idGenerado;
        }

        public static async Task<Usuario> ExtraerDatosLoginPorEmail(string email)
        {
            Usuario usu;

            // 1. Hacemos una peticion GET a la API pasando el email por la URL
            HttpResponseMessage response = await _http.GetAsync($"{_urlBase}/usuario/login?email={email}");

            // 2. Si la respuesta fue exitosa continuamos
            if (response.IsSuccessStatusCode)
            {
                // 3. Convertimos ese JSON a un objeto Usuario y lo devolvemos
                usu = await response.Content.ReadFromJsonAsync<Usuario>();
            }
            else
            {
                // 4. Sino va bien lo dejamos vacio
                usu = new Usuario();
            }

            return usu;
        }
        #endregion

        #region PERFIL
        public static async Task InsertarPerfilDirectoEnNube(Perfil perfil)
        {
            HttpResponseMessage response = await _http.PostAsJsonAsync($"{_urlBase}/perfil/insertar", perfil);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al crear perfil: {error}");
            }
        }

        public static async Task SincronizarPerfiles(List<Perfil> perfiles)
        {
            // Mandamos la lista de perfiles pendientes a la API de golpe
            await _http.PutAsJsonAsync($"{_urlBase}/perfil/sincronizar", perfiles);
        }

        public static async Task<List<Perfil>> CargarPerfilesDesdeNube()
        {
            List<Perfil> perfiles;

            // 1. Pedimos todos los perfiles a la API
            HttpResponseMessage response = await _http.GetAsync($"{_urlBase}/perfil/todos");

            // 2. Convertimos el JSON a una lista de Perfil
            if (response.IsSuccessStatusCode)
            {
                // 3. Convertimos ese JSON a una lista de objeto Perfil y lo devolvemos
                perfiles = await response.Content.ReadFromJsonAsync<List<Perfil>>();
            }
            else
            {
                // 4. Sino va bien lo dejamos vacio
                perfiles = new List<Perfil>();
            }

            return perfiles;
        }
        #endregion

        #region PARTIDA
        public static async Task<int> InsertarPartida(Partida partida)
        {
            int idGenerado;

            // 1. Mandamos la partida a la API por POST
            HttpResponseMessage response = await _http.PostAsJsonAsync($"{_urlBase}/partida", partida);

            // 2. Leemos el ID real que genero la BD
            string resultado = await response.Content.ReadAsStringAsync();
            idGenerado = Convert.ToInt32(resultado);

            return idGenerado;
        }

        public static async Task<List<PartidaSQL>> CargarPartidasDesdeNube()
        {
            List<PartidaSQL> partidas;

            // 1. Pedimos todas las partidas a la API
            HttpResponseMessage response = await _http.GetAsync($"{_urlBase}/partida/todos");

            // 2. Convertimos el JSON a una lista de PartidaSQL
            if (response.IsSuccessStatusCode)
            {
                partidas = await response.Content.ReadFromJsonAsync<List<PartidaSQL>>();
            }
            else
            {
                partidas = new List<PartidaSQL>();
            }

            return partidas;
        }
        #endregion

        #region LOGRO
        public static async Task<List<Logro>> CargarLogrosDesdeNube()
        {
            List<Logro> logros;

            // 1. Pedimos todos los logros a la API
            HttpResponseMessage response = await _http.GetAsync($"{_urlBase}/logro/todos");

            // 2. Convertimos el JSON a una lista de Logro
            if (response.IsSuccessStatusCode)
            {
                logros = await response.Content.ReadFromJsonAsync<List<Logro>>();
            }
            else
            {
                logros = new List<Logro>();
            }

            return logros;
        }

        public static async Task<List<PerfilLogroSQL>> CargarUsuarioLogrosDesdeNube()
        {
            List<PerfilLogroSQL> logros;

            // 1. Pedimos todos los logros de usuario a la API
            HttpResponseMessage response = await _http.GetAsync($"{_urlBase}/logro/todos-usuario-logros");

            // 2. Convertimos el JSON a una lista de PerfilLogroSQL
            if (response.IsSuccessStatusCode)
            {
                logros = await response.Content.ReadFromJsonAsync<List<PerfilLogroSQL>>();
            }
            else
            {
                logros = new List<PerfilLogroSQL>();
            }

            return logros;
        }

        public static async Task SincronizarLogros(List<PerfilLogroSQL> logros)
        {
            // Mandamos la lista de logros pendientes a la API de golpe
            await _http.PostAsJsonAsync($"{_urlBase}/logro/sincronizar", logros);
        }
        #endregion

        #region JUEGO
        public static async Task<List<Juego>> CargarJuegosDesdeNube()
        {
            List<Juego> juegos;

            HttpResponseMessage response = await _http.GetAsync($"{_urlBase}/juego/todos");

            if (response.IsSuccessStatusCode)
            {
                juegos = await response.Content.ReadFromJsonAsync<List<Juego>>();
            }
            else
            {
                juegos = new List<Juego>();
            }

            return juegos;
        }
        #endregion

        #region SINCRONIZACION COMPLETA
        public static async Task SincronizarHaciaApi(string tipo)
        {
            try
            {
                // Solo sincronizamos si hay internet
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    switch (tipo)
                    {
                        case "Partida":
                            List<Partida> partidas = ApiSQLiteFAFA.ExtraerPartidasPendientes();
                            foreach (Partida p in partidas)
                            {
                                int idReal = await InsertarPartida(p);
                                ApiSQLiteFAFA.ActualizarIdPartidaSincronizada(p, idReal);
                            }
                            break;
                        case "Perfil":
                            List<Perfil> perfiles = ApiSQLiteFAFA.ExtraerPerfilesPendientes();
                            await SincronizarPerfiles(perfiles);
                            break;
                        case "Logro":
                            List<PerfilLogroSQL> logros = ApiSQLiteFAFA.ExtraerLogrosPendientes();
                            await SincronizarLogros(logros);
                            break;
                        case "Todo":
                            List<Partida> todasPartidas = ApiSQLiteFAFA.ExtraerPartidasPendientes();
                            foreach (Partida p in todasPartidas)
                            {
                                int idReal = await InsertarPartida(p);
                                ApiSQLiteFAFA.ActualizarIdPartidaSincronizada(p, idReal);
                            }
                            await SincronizarPerfiles(ApiSQLiteFAFA.ExtraerPerfilesPendientes());
                            await SincronizarLogros(ApiSQLiteFAFA.ExtraerLogrosPendientes());
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sincronizando: {ex.Message}");
            }
        }

        public static async Task CargarDatosDesdeApi(string directorioTrabajo)
        {
            try
            {
                // Solo cargamos si hay internet
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    List<Perfil> perfiles = await CargarPerfilesDesdeNube();
                    List<PartidaSQL> partidas = await CargarPartidasDesdeNube();
                    List<Logro> logros = await CargarLogrosDesdeNube();
                    List<PerfilLogroSQL> usuarioLogros = await CargarUsuarioLogrosDesdeNube();
                    List<Juego> juegos = await CargarJuegosDesdeNube();


                    // Guardamos en SQLite local
                    ApiSQLiteFAFA.GuardarPerfilesEnLocal(perfiles);
                    ApiSQLiteFAFA.GuardarPartidasEnLocal(partidas);
                    ApiSQLiteFAFA.GuardarLogrosEnLocal(logros);
                    ApiSQLiteFAFA.GuardarUsuarioLogrosEnLocal(usuarioLogros);
                    ApiSQLiteFAFA.GuardarJuegosEnLocal(juegos);

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando: {ex.Message}");
            }
        }
        #endregion

        #region GESTION DE PASSWORD HASH
        public static string GenerarHash(string passwordReal)
        {
            // 1. Creamos una instancia del algoritmo SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // 2. Convertimos la contraseña real en un array de bytes
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(passwordReal));

                // 3. Convertimos esos bytes en una cadena Hexadecimal (el Hash)
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    // "x2" hace que cada byte sea un par de letras/números (hex)
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString(); // Esto devuelve algo como "a665a4592042..."
            }
        }
        #endregion
    }
}
