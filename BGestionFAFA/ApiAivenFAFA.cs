using BModelosSQLFAFA;
using MySqlConnector;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    using (SQLiteConnection local = new SQLiteConnection(rutaCompletaPersonal))
                    {
                        while (await reader.ReadAsync())
                        {
                            int idNube = reader.GetInt32("id_usuario");
                            if (local.Table<UsuarioSQL>().FirstOrDefault(x => x.IdUsuario == idNube) == null)
                            {
                                local.Insert(new UsuarioSQL
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
                            if (local.Table<PerfilSQL>().FirstOrDefault(x => x.PerfilUid == uidNube) == null)
                            {
                                local.Insert(new PerfilSQL
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
                            if (local.Table<JuegoSQL>().FirstOrDefault(x => x.IdJuego == idNube) == null)
                            {
                                local.Insert(new JuegoSQL
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
                            if (local.Table<LogroSQL>().FirstOrDefault(x => x.IdLogro == idNube) == null)
                            {
                                local.Insert(new LogroSQL
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
        }

        private static async Task CargarPartidasDesdeNube()
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
                            if (local.Table<PartidaSQL>().FirstOrDefault(x => x.IdPartida == idNube) == null)
                            {
                                local.Insert(new PartidaSQL
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
        public async static Task SubirDatosCompletosHaciaAiven()
        {
            try
            {
                NetworkAccess accesoRed = Connectivity.Current.NetworkAccess;
                if (accesoRed != NetworkAccess.Internet) return;

                using (SQLiteConnection local = new SQLiteConnection(rutaCompletaPersonal))
                {
                    // 1. SUBIR USUARIOS NUEVOS O ACTUALIZADOS
                    List<UsuarioSQL> usuariosPendientes = local.Table<UsuarioSQL>().Where(u => u.Sincronizada == false).ToList();
                    foreach (UsuarioSQL user in usuariosPendientes)
                    {
                        bool exito = await InsertarOActualizarUsuarioNube(user);
                        if (exito)
                        {
                            user.Sincronizada = true;
                            local.Update(user);
                        }
                    }

                    // 2. SUBIR PERFILES (Actualizaciones de Nivel, XP o Avatares)
                    List<PerfilSQL> perfilesPendientes = local.Table<PerfilSQL>().Where(p => p.Sincronizada == false).ToList();
                    foreach (PerfilSQL perfil in perfilesPendientes)
                    {
                        bool exito = await InsertarOActualizarPerfilNube(perfil);
                        if (exito)
                        {
                            perfil.Sincronizada = true;
                            local.Update(perfil);
                        }
                    }

                    // 3. SUBIR PARTIDAS JUGADAS OFFLINE
                    List<PartidaSQL> partidasPendientes = local.Table<PartidaSQL>().Where(p => p.Sincronizada == false).ToList();
                    foreach (PartidaSQL partida in partidasPendientes)
                    {
                        bool exito = await InsertarPartidaNube(partida);
                        if (exito)
                        {
                            partida.Sincronizada = true;
                            local.Update(partida);
                        }
                    }

                    // 4. SUBIR LOGROS DESBLOQUEADOS
                    List<PerfilLogroSQL> logrosPendientes = local.Table<PerfilLogroSQL>().Where(l => l.Sincronizado == false).ToList();
                    foreach (PerfilLogroSQL logro in logrosPendientes)
                    {
                        bool exito = await InsertarUsuarioLogroNube(logro);
                        if (exito)
                        {
                            logro.Sincronizado = true;
                            local.Update(logro);
                        }
                    }

                    //// 5. SUBIR AMISTADES (Solicitudes o respuestas)
                    //List<AmistadSQL> amistadesPendientes = local.Table<AmistadSQL>().Where(a => a.Sincronizada == false).ToList();
                    //foreach (AmistadSQL amistad in amistadesPendientes)
                    //{
                    //    bool exito = await InsertarOActualizarAmistadNube(amistad);
                    //    if (exito)
                    //    {
                    //        amistad.Sincronizada = true;
                    //        local.Update(amistad);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR AL SUBIR A AIVEN: {ex.Message}");
            }
        }



        private static async Task<bool> InsertarOActualizarUsuarioNube(UsuarioSQL user)
        {
            using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
            {
                await conexionNube.OpenAsync();
                string query = "INSERT INTO USUARIO (id_usuario, username, email, password_hash) " +
                               "VALUES (@id, @user, @email, @pass) " +
                               "ON DUPLICATE KEY UPDATE username = @user, email = @email, password_hash = @pass";

                MySqlCommand cmd = new MySqlCommand(query, conexionNube);
                cmd.Parameters.AddWithValue("@id", user.IdUsuario);
                cmd.Parameters.AddWithValue("@user", user.NombreUsuario);
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@pass", user.Password);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        private static async Task<bool> InsertarOActualizarPerfilNube(PerfilSQL perfil)
        {
            using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
            {
                await conexionNube.OpenAsync();
                string query = "INSERT INTO PERFIL (perfil_uid, id_usuario, nombre_usuario, nivel, xp_total, avatar_url) " +
                               "VALUES (@uid, @idU, @nom, @lvl, @xp, @ava) " +
                               "ON DUPLICATE KEY UPDATE nivel = @lvl, xp_total = @xp, avatar_url = @ava, nombre_usuario = @nom";

                MySqlCommand cmd = new MySqlCommand(query, conexionNube);
                cmd.Parameters.AddWithValue("@uid", perfil.PerfilUid);
                cmd.Parameters.AddWithValue("@idU", perfil.IdUsuario);
                cmd.Parameters.AddWithValue("@nom", perfil.NombreUsuario);
                cmd.Parameters.AddWithValue("@lvl", perfil.Nivel);
                cmd.Parameters.AddWithValue("@xp", perfil.XpTotal);
                cmd.Parameters.AddWithValue("@ava", string.IsNullOrEmpty(perfil.AvatarUrl) ? "" : perfil.AvatarUrl);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        private static async Task<bool> InsertarPartidaNube(PartidaSQL partida)
        {
            using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
            {
                await conexionNube.OpenAsync();
                string query = "INSERT INTO PARTIDA (id_juego, id_perfil, puntuacion, tiempo_segundos, victoria, fecha_hora) " +
                               "VALUES (@idJ, @idP, @pts, @time, @vic, @fecha)";

                MySqlCommand cmd = new MySqlCommand(query, conexionNube);
                cmd.Parameters.AddWithValue("@idJ", partida.IdJuego);
                cmd.Parameters.AddWithValue("@idP", partida.IdPerfil);
                cmd.Parameters.AddWithValue("@pts", partida.Puntuacion);
                cmd.Parameters.AddWithValue("@time", partida.TiempoSegundos);
                cmd.Parameters.AddWithValue("@vic", partida.Victoria);
                cmd.Parameters.AddWithValue("@fecha", partida.FechaHora);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        private static async Task<bool> InsertarUsuarioLogroNube(PerfilLogroSQL logro)
        {
            try
            {
                using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
                {
                    await conexionNube.OpenAsync();
                    string query = "INSERT INTO USUARIO_LOGRO (perfil_uid, id_logro, fecha_obtencion) " +
                                   "VALUES (@idP, @idL, @fecha)";

                    MySqlCommand cmd = new MySqlCommand(query, conexionNube);
                    cmd.Parameters.AddWithValue("@idP", logro.IdPerfil);
                    cmd.Parameters.AddWithValue("@idL", logro.IdLogro);
                    cmd.Parameters.AddWithValue("@fecha", logro.FechaObtencion);

                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                // Error 1062 es "Duplicate Entry". Si ya existe en la nube, damos la tarea local por completada.
                return true;
            }
        }

        private static async Task<bool> InsertarOActualizarAmistadNube(AmistadSQL amistad)
        {
            using (MySqlConnection conexionNube = new MySqlConnection(conexionString))
            {
                await conexionNube.OpenAsync();
                string query = "INSERT INTO AMISTAD (id_usuario_solicitante, id_usuario_receptor, estado, fecha_solicitud) " +
                               "VALUES (@idSol, @idRec, @est, @fecha) " +
                               "ON DUPLICATE KEY UPDATE estado = @est";

                MySqlCommand cmd = new MySqlCommand(query, conexionNube);
                cmd.Parameters.AddWithValue("@idSol", amistad.IdUsuarioSolicitante);
                cmd.Parameters.AddWithValue("@idRec", amistad.IdUsuarioReceptor);
                cmd.Parameters.AddWithValue("@est", amistad.Estado.ToString());
                cmd.Parameters.AddWithValue("@fecha", amistad.FechaSolicitud);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
        #endregion


    }
}
