using BModelosFAFA;
using BModelosSQLFAFA;
using MySqlConnector;
using SQLite;
using System.Security.Cryptography;
using System.Text;

namespace BGestionFAFA
{
    public class ApiAivenFAFA
    {
        // Link de conexion a la base de datos en Aiven 
        static string conexionString = "Server=mysql-14c4eb7-finalproyect-dam.i.aivencloud.com;Port=15348;Database=GMiniJuegos;Uid=avnadmin;Pwd=AVNS_lnSHKPORCdt_mt007wR;SslMode=Required";

        // Ruta local del archivo SQLite
        static string rutaCompletaPersonal;
        // Nombre del archivo db
        static string nombreArchivo = "myDB.db3";

        #region METODOS DE AIVEN A SQLITE

        // Metodo que va cargar todo los datos nuevo que no tenga sqlite 
        public async static Task CargarDatosNuevosDesdeAiven(string directorioTrabajo)
        {
            try
            {
                // 1. Comprobamos internet antes de mover nada
                NetworkAccess accesoRed = Connectivity.Current.NetworkAccess;
                if (accesoRed == NetworkAccess.Internet)
                {

                    // 2. Si hay internet, construimos la ruta completa al archivo SQLite
                    rutaCompletaPersonal = Path.Combine(directorioTrabajo, nombreArchivo);


                    // 3. Llamamos a cada método individual de carga
                    // Usamos await para asegurar que una tabla termine antes de empezar la otra 
                    await CargarUsuariosDesdeNube();
                    await CargarPerfilesDesdeNube();
                    await CargarJuegosDesdeNube();
                    await CargarPartidasDesdeNube();
                    await CargarLogrosDesdeNube();
                    await CargarUsuarioLogrosDesdeNube();
                    //await CargarAmistadesDesdeNube();
                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
            }



        }

        private static async Task CargarUsuariosDesdeNube()
        {
            using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
            {
                await conexionNube.OpenAsync();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM USUARIO", conexionNube);
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    using (SQLiteConnection conexionLocal = new SQLiteConnection(rutaCompletaPersonal))
                    {
                        while (await reader.ReadAsync())
                        {
                            int idNube = reader.GetInt32("id_usuario");
                      
                                conexionLocal.InsertOrReplace(new UsuarioSQL
                                {
                                    IdUsuario = idNube,
                                    NombreUsuario = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    Password = reader.GetString("password_hash"),
                                    Sincronizada = true
                                });
                            
                           
                        }
                    }
                }
            }
        }

        private static async Task CargarPerfilesDesdeNube()
        {
            using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
            {
                await conexionNube.OpenAsync();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM PERFIL", conexionNube);
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    using (SQLiteConnection local = new SQLiteConnection(rutaCompletaPersonal))
                    {
                        while (await reader.ReadAsync())
                        {
                            string uidNube = reader.GetString("perfil_uid");
                    
                                local.InsertOrReplace(new PerfilSQL
                                {
                                    PerfilUid = uidNube,
                                    IdUsuario = reader.GetInt32("id_usuario"),
                                    Nivel = reader.GetInt32("nivel"),
                                    XpTotal = reader.GetInt32("xp_total"),
                                    AvatarUrl = reader.IsDBNull(reader.GetOrdinal("avatar_url")) ? "" : reader.GetString("avatar_url"),
                                    NombreUsuario = reader.GetString("nombre_usuario"),
                                    Sincronizada = true
                                });
                            
                        }
                    }
                }
            }
        }

        private static async Task CargarJuegosDesdeNube()
        {
            using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
            {
                await conexionNube.OpenAsync();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM JUEGO", conexionNube);
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    using (SQLiteConnection local = new SQLiteConnection(rutaCompletaPersonal))
                    {
                        while (await reader.ReadAsync())
                        {
                            int idNube = reader.GetInt32("id_juego");
                         
                                local.InsertOrReplace(new JuegoSQL
                                {
                                    IdJuego = idNube,
                                    Nombre = reader.GetString("nombre"),
                                    ImagenURL = reader.GetString("imagen_url"),
                                    ColorHex = reader.GetString("color_hex")
                                });
                            
                        }
                    }
                }
            }
        }

        private static async Task CargarLogrosDesdeNube()
        {
            using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
            {
                await conexionNube.OpenAsync();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM LOGRO", conexionNube);
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    using (SQLiteConnection local = new SQLiteConnection(rutaCompletaPersonal))
                    {
                        while (await reader.ReadAsync())
                        {
                            int idNube = reader.GetInt32("id_logro");
                            
                                local.InsertOrReplace(new LogroSQL
                                {
                                    IdLogro = idNube,
                                    IdJuego = reader.GetInt32("id_juego"),
                                    Nombre = reader.GetString("nombre"),
                                    CondicionDesbloqueo = reader.GetString("condicion_desbloqueo"),
                                    XpPremio = reader.GetInt32("xp_premio"),
                                    Sincronizada = true
                                });
                            
                        }
                    }
                }
            }
        }

        public static async Task CargarPartidasDesdeNube()
        {
            using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
            {
                await conexionNube.OpenAsync();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM PARTIDA", conexionNube);
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    using (SQLiteConnection local = new SQLiteConnection(rutaCompletaPersonal))
                    {
                        while (await reader.ReadAsync())
                        {
                            int idNube = reader.GetInt32("id_partida");
                            
                                local.InsertOrReplace(new PartidaSQL
                                {
                                    IdPartida = idNube,
                                    IdJuego = reader.GetInt32("id_juego"),
                                    IdPerfil = reader.GetString("id_perfil"),
                                    Puntuacion = reader.GetInt32("puntuacion"),
                                    TiempoSegundos = reader.GetInt32("tiempo_segundos"),
                                    Victoria = reader.GetBoolean("victoria"),
                                    FechaHora = reader.GetDateTime("fecha_hora"),
                                    Sincronizada = true
                                });
                            
                        }
                    }
                }
            }
        }

        private static async Task CargarUsuarioLogrosDesdeNube()
        {
            using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
            {
                await conexionNube.OpenAsync();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM USUARIO_LOGRO", conexionNube);
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    using (SQLiteConnection local = new SQLiteConnection(rutaCompletaPersonal))
                    {
                        while (await reader.ReadAsync())
                        {
                            int idNube = reader.GetInt32("id");
                            if (local.Table<PerfilLogroSQL>().FirstOrDefault(x => x.Id == idNube) == null)
                            {
                                local.Insert(new PerfilLogroSQL
                                {
                                    Id = idNube,
                                    IdPerfil = reader.GetString("perfil_uid"), // Cambiado para coincidir con SQL
                                    IdLogro = reader.GetInt32("id_logro"),
                                    FechaObtencion = reader.GetDateTime("fecha_obtencion"),
                                    Sincronizado = true
                                });
                            }
                        }
                    }
                }
            }
        }

        private static async Task CargarAmistadesDesdeNube()
        {
            using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
            {
                await conexionNube.OpenAsync();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM AMISTAD", conexionNube);
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    using (SQLiteConnection local = new SQLiteConnection(rutaCompletaPersonal))
                    {
                        while (await reader.ReadAsync())
                        {
                            int idNube = reader.GetInt32("id");
                            if (local.Table<AmistadSQL>().FirstOrDefault(x => x.Id == idNube) == null)
                            {
                                string estadoNube = reader.GetString("estado").ToUpper();
                                Estados estadoEnum = (Estados)Enum.Parse(typeof(Estados), estadoNube);

                                local.Insert(new AmistadSQL
                                {
                                    Id = idNube,
                                    IdUsuarioSolicitante = reader.GetInt32("id_usuario_solicitante"),
                                    IdUsuarioReceptor = reader.GetInt32("id_usuario_receptor"),
                                    Estado = estadoEnum,
                                    FechaSolicitud = reader.GetDateTime("fecha_solicitud"),
                                    Sincronizada = true
                                });
                            }
                        }
                    }
                }
            }
        }

        public static void RefrescarEstadisticasPerfil(int idUsuario)
        {
            using (SQLiteConnection local = new SQLiteConnection(rutaCompletaPersonal))
            {
                PerfilSQL perfil = local.Table<PerfilSQL>().FirstOrDefault(p => p.IdUsuario == idUsuario);
                if (perfil != null)
                {
                    List<int> listaLogrosIds = local.Table<PerfilLogroSQL>()
                                                    .Where(ul => ul.IdPerfil == perfil.PerfilUid)
                                                    .Select(ul => ul.IdLogro)
                                                    .ToList();

                    int xpReal = 0;
                    if (listaLogrosIds.Count > 0)
                    {
                        xpReal = local.Table<LogroSQL>()
                                      .Where(l => listaLogrosIds.Contains(l.IdLogro))
                                      .Sum(l => l.XpPremio);
                    }

                    if (perfil.XpTotal != xpReal)
                    {
                        perfil.XpTotal = xpReal;
                        perfil.Nivel = (xpReal / 1000) + 1;
                        perfil.Sincronizada = false;
                        local.Update(perfil);
                    }
                }
            }
        }
        #endregion

        #region METODOS DE SQLITE A AIVEN

        public async static Task SincronizarHaciaAiven(string tipo)
        {

            try
            {
                // Si hay red continuamos, si no, no hacemos nada porque no hay conexión para subir datos
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    using (SQLiteConnection conexionLocal = new SQLiteConnection(rutaCompletaPersonal))
                    using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
                    {
                        await conexionNube.OpenAsync();

                        switch (tipo)
                        {
                            case "Usuario":
                                await SubirUsuariosPendientes(conexionLocal, conexionNube);
                                break;
                            case "Perfil":
                                await SubirPerfilesPendientes(conexionLocal, conexionNube);
                                break;
                            case "Partida":
                                await SubirPartidasPendientes(conexionLocal, conexionNube);
                                break;
                            case "Logro":
                                await SubirLogrosPendientes(conexionLocal, conexionNube);
                                break;
                            case "Todo":
                                await SubirUsuariosPendientes(conexionLocal, conexionNube);
                                await SubirPerfilesPendientes(conexionLocal, conexionNube);
                                await SubirPartidasPendientes(conexionLocal, conexionNube);
                                await SubirLogrosPendientes(conexionLocal, conexionNube);
                                break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sincro Error: {ex.Message}");
            }
        }


        private static async Task SubirUsuariosPendientes(SQLiteConnection conexionLocal, MySqlConnection conexionNube)
        {
            List<UsuarioSQL> pendientes = conexionLocal.Table<UsuarioSQL>().Where(u => u.Sincronizada == false).ToList();
            foreach (UsuarioSQL user in pendientes)
            {
                // 1. Sacamos la password real del movil 
                string passReal = await ApiSQLiteFAFA.ObtenerPasswordOculta(user.IdUsuario);


                // 2. Generamos el HASH para que Aiven no vea la password real
                string passParaNube = GenerarHash(passReal);

                // 3. Insertamos en Aiven
                string query = @"INSERT INTO USUARIO (username, email, password_hash) 
                         VALUES (@user, @email, @pass) 
                         ON DUPLICATE KEY UPDATE password_hash = @pass";

                using (MySqlCommand cmd = new MySqlCommand(query, conexionNube))
                {
                    cmd.Parameters.AddWithValue("@user", user.NombreUsuario ?? "Usuario");
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    cmd.Parameters.AddWithValue("@pass", passParaNube); // Mandamos el HASH

                    // Si la inserción/actualización fue exitosa, marcamos como sincronizado
                    if (await cmd.ExecuteNonQueryAsync() > 0)
                    {
                        user.Sincronizada = true;
                        conexionLocal.Update(user);
                    }
                }
            }
        }

        private static async Task SubirPerfilesPendientes(SQLiteConnection conexionLocal, MySqlConnection conexionNube)
        {
            List<PerfilSQL> pendientes = conexionLocal.Table<PerfilSQL>().Where(p => p.Sincronizada == false).ToList();
            foreach (PerfilSQL perfil in pendientes)
            {
                string query = @"INSERT INTO PERFIL (perfil_uid, id_usuario, nombre_usuario, nivel, xp_total, avatar_url) 
                         VALUES (@uid, @idU, @nom, @lvl, @xp, @ava) 
                         ON DUPLICATE KEY UPDATE nivel = @lvl, xp_total = @xp, avatar_url = @ava, nombre_usuario = @nom";

                using (MySqlCommand cmd = new MySqlCommand(query, conexionNube))
                {
                    cmd.Parameters.AddWithValue("@uid", perfil.PerfilUid);
                    cmd.Parameters.AddWithValue("@idU", perfil.IdUsuario);
                    cmd.Parameters.AddWithValue("@nom", perfil.NombreUsuario);
                    cmd.Parameters.AddWithValue("@lvl", perfil.Nivel);
                    cmd.Parameters.AddWithValue("@xp", perfil.XpTotal);
                    cmd.Parameters.AddWithValue("@ava", perfil.AvatarUrl ?? "");

                    if (await cmd.ExecuteNonQueryAsync() > 0)
                    {
                        perfil.Sincronizada = true;
                        conexionLocal.Update(perfil);
                    }
                }
            }
        }

        private static async Task SubirPartidasPendientes(SQLiteConnection conexionLocal, MySqlConnection conexionNube)
        {
            List<PartidaSQL> pendientes = conexionLocal.Table<PartidaSQL>().Where(p => p.Sincronizada == false).ToList();
            foreach (PartidaSQL partida in pendientes)
            {
                // En partidas no solemos usar ON DUPLICATE KEY porque cada partida es nueva
                string query = @"INSERT INTO PARTIDA (id_juego, id_perfil, puntuacion, tiempo_segundos, victoria, fecha_hora) 
                         VALUES (@idJ, @idP, @pts, @time, @vic, @fecha)";

                using (MySqlCommand cmd = new MySqlCommand(query, conexionNube))
                {
                    cmd.Parameters.AddWithValue("@idJ", partida.IdJuego);
                    cmd.Parameters.AddWithValue("@idP", partida.IdPerfil);
                    cmd.Parameters.AddWithValue("@pts", partida.Puntuacion);
                    cmd.Parameters.AddWithValue("@time", partida.TiempoSegundos);
                    cmd.Parameters.AddWithValue("@vic", partida.Victoria);
                    cmd.Parameters.AddWithValue("@fecha", partida.FechaHora);

                    if (await cmd.ExecuteNonQueryAsync() > 0)
                    {
                        partida.Sincronizada = true;
                        conexionLocal.Update(partida);
                    }
                }
            }
        }

        private static async Task SubirLogrosPendientes(SQLiteConnection conexionLocal, MySqlConnection conexionNube)
        {
            List<PerfilLogroSQL> pendientes = conexionLocal.Table<PerfilLogroSQL>().Where(l => l.Sincronizado == false).ToList();
            foreach (PerfilLogroSQL logro in pendientes)
            {
                string query = @"INSERT INTO USUARIO_LOGRO (perfil_uid, id_logro, fecha_obtencion) 
                         VALUES (@uid, @idL, @fecha)
                         ON DUPLICATE KEY UPDATE fecha_obtencion = @fecha";
                

                using (MySqlCommand cmd = new MySqlCommand(query, conexionNube))
                {
                    cmd.Parameters.AddWithValue("@uid", logro.IdPerfil);
                    cmd.Parameters.AddWithValue("@idL", logro.IdLogro);
                    cmd.Parameters.AddWithValue("@fecha", logro.FechaObtencion);

                    if (await cmd.ExecuteNonQueryAsync() > 0)
                    {
                        logro.Sincronizado = true;
                        conexionLocal.Update(logro);
                    }
                }
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

        public static async Task<Usuario> ExtraerDatosLoginPorEmail(string email)
        {
            Usuario usuarioEncontrado = null;

            // Usamos la cadena de conexión de tu servidor Aiven
            using (MySqlConnection conexion = new MySqlConnection(conexionString))
            {
                try
                {
                    await conexion.OpenAsync();

                    // Buscamos el ID y la Password (que está en HEX/Hash en la nube)
                    string query = "SELECT id_usuario, username, email, password_hash FROM USUARIO WHERE email = @email LIMIT 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@email", email);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                usuarioEncontrado = new Usuario
                                {
                                    IdUsuario = reader.GetInt32("id_usuario"),
                                    NombreUsuario = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    // IMPORTANTE: Aquí recogemos el HEX que guardamos al registrar
                                    Password = reader.GetString("password_hash")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log de error para depuración
                    System.Diagnostics.Debug.WriteLine("Error en Aiven: " + ex.Message);
                    throw new Exception("Error al conectar con la base de datos en la nube.");
                }
            }

            return usuarioEncontrado; // Si no existe, devolverá null
        }
        #endregion

        #region METODOS DE INSERCCION A AIVEN
        public static async Task<int> InsertarUsuarioEnNube(string email, string user, string pass)
        {
            using (MySqlConnection conn = new MySqlConnection(conexionString))
            {
                await conn.OpenAsync();
                // Comprobamos si el email ya existe en Aiven antes de insertar
                // (Esto evita el error de duplicados en la nube)

                string sql = "INSERT INTO USUARIO (email, username, password_hash) VALUES (@e, @u, @p)";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    cmd.Parameters.AddWithValue("@u", user);
                    cmd.Parameters.AddWithValue("@p", GenerarHash(pass)); // Idealmente el hash
                    await cmd.ExecuteNonQueryAsync();

                    return (int)cmd.LastInsertedId; // <--- ESTO DEVUELVE EL ID REAL
                }
            }
        }

        public static async Task InsertarPerfilDirectoEnNube(Perfil p)
        {
            using (MySqlConnection conexion = new MySqlConnection(conexionString))
            {
                await conexion.OpenAsync();

                // IMPORTANTE: Asegúrate de que los nombres de las columnas 
                // coincidan exactamente con tu tabla en Aiven
                string query = @"INSERT INTO PERFIL (perfil_uid, id_usuario, nombre_usuario, nivel, xp_total, avatar_url) 
                         VALUES (@uid, @idUsu, @nom, @niv, @xp, @ava)";

                using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                {
                    // Usamos los datos del objeto Perfil que ya viene con el ID de Aiven
                    cmd.Parameters.AddWithValue("@uid", p.PerfilUid);
                    cmd.Parameters.AddWithValue("@idUsu", p.IdUsuario);
                    cmd.Parameters.AddWithValue("@nom", p.NombreUsuario);
                    cmd.Parameters.AddWithValue("@niv", 1); // Nivel inicial
                    cmd.Parameters.AddWithValue("@xp", 0);  // XP inicial

                    // Manejo de nulos para el avatar
                    cmd.Parameters.AddWithValue("@ava", string.IsNullOrEmpty(p.AvatarUrl) ? (object)DBNull.Value : p.AvatarUrl);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        #endregion


    }
}
