using BModelosFAFA;
using BModelosSQLFAFA;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Storage;
using System.Text;
using System.Threading.Tasks;

namespace BGestionFAFA
{ 
    public class ApiSQLiteFAFA
    {
        // Crear conexion
        static SQLiteConnection conexion = null;

        // Nombre del archivo db
        static string nombreArchivo = "myDB.db3";

        // Ruta de la carpeta personal + archivo
        static string rutaCompletaPersonal = "";

        #region COMPROBACIONS INICIALES
        public static void ComprobarRutasSQL(string directorioTrabajo)
        {

            // 1.- Comprobamos si existe la ruta, SINO la creamos
            if (!Directory.Exists(directorioTrabajo))
            {
                Directory.CreateDirectory(directorioTrabajo);
            }

            // 2.- Combinamos la ruta del direcotiro mas el nombre del archivo
            rutaCompletaPersonal = Path.Combine(directorioTrabajo, nombreArchivo);

            // 3.- Comprobamos si el archivo existe sino existe creamos el archivo
            if (!File.Exists(rutaCompletaPersonal))
            {
                // Importante cerrar la creacion al final
                File.Create(rutaCompletaPersonal).Close();
                
            }

            // 4.- Una vez comprobada o creada iniciamos verificamos que se puede conectar y que este creada las tablas necesarias
            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                CrearTablasNecesaria(conexion);
            }



        }

        private async static Task CrearTablasNecesaria(SQLiteConnection conexion)
        {
            // Esto hara que se crren la tablas necesarias si no estan creadas
            conexion.CreateTable<UsuarioSQL>();

            conexion.CreateTable<UsuarioLogroSQL>();
            conexion.CreateTable<AmistadSQL>();
            conexion.CreateTable<JuegoSQL>();
            conexion.CreateTable<PartidaSQL>();
            conexion.CreateTable<PerfilSQL>();
            conexion.CreateTable<LogroSQL>();

            // 1. Conteo síncrono (usando el modelo de la tabla SQL)
            int conteoJuegos = conexion.Table<JuegoSQL>().Count();

            if (conteoJuegos == 0)
            {
                // 2. Definimos la lista con el modelo "Juego" (el que entiende tu método)
                // USAMOS List<Juego> para que el foreach funcione
                List<Juego> juegosIniciales = new List<Juego>
                {
                    new Juego { Nombre = "TOPOS", ImagenURL = "portada_topos.png", ColorHex = "#DC143C" },
                    new Juego { Nombre = "WORDLE", ImagenURL = "portada_adivinalapalabra.png", ColorHex = "#00FA9A" },
                    new Juego { Nombre = "PAREJAS", ImagenURL = "portada_buscarparejas.png", ColorHex = "#00BFFF" },
                    new Juego { Nombre = "2048", ImagenURL = "portada_2048.png", ColorHex = "#FFFF00" }
                };

                // 3. Ahora sí, recorremos la lista de modelos "Juego"
                foreach (Juego j in juegosIniciales)
                {
                    // 4. Se lo pasamos a tu método que ya se encarga de convertirlo a JuegoSQL e insertar
                    InsertarJuego(j);
                }
            }
        }

        #endregion


        #region METODOS DE INSERCIÓN
        public static void InsertarJuego(Juego juegoNormal)
        {
            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                JuegoSQL juegoSQL = new JuegoSQL
                {
                    // IdJuego no se pasa porque es AutoIncrement
                    Nombre = juegoNormal.Nombre,
                    ImagenURL = juegoNormal.ImagenURL,
                    ColorHex = juegoNormal.ColorHex,
                };
                conexion.Insert(juegoSQL);
            }
        }

        // Este metodo ademas de agregar usuario devolvera su id para poder crear el perfil a continuacion
        public static int InsertarUsuarioYDevolverID(Usuario usuario)
        {
            int idUsuActual;

            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                // Lo pasos a usuario sql
                UsuarioSQL usuarioSQL = new UsuarioSQL
                {
                    Email = usuario.Email,
                    Sincronizada = false,
                };
                // Insertamos dicho usuario
                conexion.Insert(usuarioSQL);

                // Extraemos id que luego nos sera necesario
                idUsuActual = usuarioSQL.IdUsuario;

            }

            // Devolvemos el id del usuario creado
            return idUsuActual;
        }

        public static void InsertarLogro(Logro logro)
        {
            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                LogroSQL logroSQL = new LogroSQL
                {
                    IdJuego = logro.IdJuego,
                    Nombre = logro.Nombre,
                    CondicionDesbloqueo = logro.CondicionDesbloqueo,
                    XpPremio = logro.XpPremio,
                    Sincronizada = false,
                };
                conexion.Insert(logroSQL);
            }
        }

        public static void InsertarPerfil(Perfil perfil)
        {
            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                PerfilSQL perfilSQL = new PerfilSQL
                {
                   
                    IdUsuario = perfil.IdUsuario,
                    NombreUsuario = perfil.NombreUsuario,
                    Nivel = perfil.Nivel,
                    XpTotal = perfil.XpTotal,
                    Sincronizada = false,
                    AvatarUrl = perfil.AvatarUrl
                };

                //Generamos su UID unico y personal seugno su nombre de usuario
                perfilSQL.PerfilUid = perfilSQL.GenerarUidPerfil(perfil.NombreUsuario);
                conexion.Insert(perfilSQL);
            }
        }

        public static int InsertarPartidaYDevolverIDPartida(Partida partida)
        {
            PartidaSQL partidaSQL;
            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                partidaSQL = new PartidaSQL
                {
                    IdJuego = partida.IdJuego,
                    IdPerfil = partida.IdPerfil,
                    Puntuacion = partida.Puntuacion,
                    TiempoSegundos = partida.TiempoSegundos,
                    Victoria = partida.Victoria,
                    Sincronizada = false
                };
                conexion.Insert(partidaSQL);
            }
            return partidaSQL.IdPartida;
        }

        public static void InsertarPartida(Partida partida)
        {
            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                PartidaSQL partidaSQL = new PartidaSQL
                {
                    IdJuego = partida.IdJuego,
                    IdPerfil = partida.IdPerfil,
                    Puntuacion = partida.Puntuacion,
                    TiempoSegundos = partida.TiempoSegundos,
                    Victoria = partida.Victoria,
                    Sincronizada = false
                };
                conexion.Insert(partidaSQL);
            }
        }

        public static void InsertarAmistad(Usuario usuSolicitante, Usuario usuReceptor)
        {
            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                AmistadSQL amistadSQL = new AmistadSQL
                {
                    IdUsuarioSolicitante = usuSolicitante.IdUsuario,
                    IdUsuarioReceptor = usuReceptor.IdUsuario,
                    Estado = Estados.PENDIENTE, // Asegura que el enum coincida
                    FechaSolicitud = DateTime.Now,
                    Sincronizada = false,
                };
                conexion.Insert(amistadSQL);
            }
        }

        public static void InsertarUsuarioLogro(Usuario usu, Logro logro)
        {
            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                UsuarioLogroSQL usuarioLogroSQL = new UsuarioLogroSQL
                {
                    IdUsuario = usu.IdUsuario,
                    IdLogro = logro.IdLogro,
                    FechaObtencion = DateTime.Now,
                    Sincronizado = false
                };
                conexion.Insert(usuarioLogroSQL);
            }
        }
        #endregion

        #region GESTION DE CONTRASEÑAS

        public static async void GuardarContrasenaOculta(int idUsu, string password)
        {
                // Le ponemos un id   
                string claveSegura = $"password_mail_{idUsu}";
                // Guardamos el id con la clave segura
                await SecureStorage.Default.SetAsync(claveSegura, password);
            
  
        }

        public static async Task<string> ObtenerPasswordOculta(int? cuentaId)
        {

            // 1.- Formamos el id oculto que creamos
            string claveSegura = $"password_mail_{cuentaId}";

            // 2.- Sacamos mediante esa clave de la contraseña original
            string passwordRecuperada = await SecureStorage.Default.GetAsync(claveSegura);

            // 3.- La devolvemos
            return passwordRecuperada;
            
        }
        public async void ModificarContrasenaOculta(int cuentaId, string nuevaPassword)
        {
            // Usamos la misma llave que usamos la primera vez
            string claveSegura = $"password_mail_{cuentaId}";

            // Al hacer SetAsync sobre una llave que ya existe, el SecureStorage actualiza el valor automaticamente
            await SecureStorage.Default.SetAsync(claveSegura, nuevaPassword);
        }

        public void EliminarContrasena(int cuentaId)
        {
            // Usamos la misma etiqueta/llave que creamos al guardar
            string claveSegura = $"password_mail_{cuentaId}";

            // Eliminamos esa contraseña en especifico
            SecureStorage.Default.Remove(claveSegura);
        }
        #endregion

        #region METODOS COMPROBACION EXISTENCIA

        public static void ComprobarInicioSesion(string email, string password)
        {
            // 1. Comprobamos si existe dicho correo
            ComprobarSiExisteUsuario(email);

            

        }

        public static void ComprobarSiExisteUsuario(string email)
        {
            // Abrimos conexion
            using(conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                // Y Sacamos de nuestra tabla un usuario que tenga x emal
                bool usuEncontrado = conexion.Table<UsuarioSQL>().Where(u => u.Email == email).Any();

                // Si encontramos a alguien con ese email saltamos excpecion de que ya existe alguien con el
                if (usuEncontrado)
                {
                    throw new Exception("ERROR: Ya existe un usuario con ese correo");
                }
            }


        }

        public static void ComprobarSiExisteNombreDePerfil(string nombreUsu)
        {

            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                bool perfilEcontrado = conexion.Table<PerfilSQL>().Where(p => p.NombreUsuario == nombreUsu).Any();

                if (perfilEcontrado)
                {
                    throw new Exception("ERROR: Ya existe ese nombre de usuario");
                }

            }

        }

        #endregion

        #region METODOS DE EXTRACCION

        public static int ExtraerIdUsuarioPorEmail(string email)
        {
            int idUsu;

            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                idUsu = conexion.Table<UsuarioSQL>().Where(u => u.Email == email).Select(u => u.IdUsuario).FirstOrDefault();
            }

            return idUsu;

        }

        public static Perfil ExtraerPerfilPorId(int idUsuIniciado)
        {

            PerfilSQL sql;
            Perfil perfilObtenido;

            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {

                 sql = conexion.Table<PerfilSQL>().Where(p => p.IdUsuario == idUsuIniciado).FirstOrDefault();

            }

            perfilObtenido = new Perfil
            {
                NombreUsuario = sql.NombreUsuario,
                PerfilUid = sql.PerfilUid,
                IdUsuario = sql.IdUsuario,
                Nivel = sql.Nivel,
                XpTotal = sql.XpTotal,
                AvatarUrl = sql.AvatarUrl,
               
                
            };

            return perfilObtenido;
        }

        public static List<Juego> ExtraerTodosLosJuegos()
        {

            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                // 1. Sacamos la lista de la tabla SQL
                List<JuegoSQL> listaSQL = conexion.Table<JuegoSQL>().ToList();

                // 2. La convertimos a una lista de Juegos "normales"
                List<Juego> listaNormal = new List<Juego>();

                foreach (JuegoSQL itemSQL in listaSQL)
                {
                    // Creamos el objeto Juego y le pasamos los datos del SQL
                    Juego juegoNormal = new Juego
                    {
                        IdJuego = itemSQL.IdJuego,
                        Nombre = itemSQL.Nombre,
                        ImagenURL = itemSQL.ImagenURL,
                        ColorHex = itemSQL.ColorHex
                        // Si tu clase Juego tiene más propiedades (como UId), añádelas aquí
                    };

                    listaNormal.Add(juegoNormal);
                }

                return listaNormal;
            }

        }

        public static List<Partida> ExtraerrMejoresMarcasPorJuego(int idJuegoEnviado)
        {
            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                // Traemos todas las partidas de ese juego
                List<PartidaSQL> todasLasPartidasDelJuego = conexion.Table<PartidaSQL>()
                                                      .Where(p => p.IdJuego == idJuegoEnviado)
                                                      .ToList();

                // Sacamos las mejores marcas de cada jugador segun lo que se valore en el juego
                List<Partida> rankingMejoresMarcas = ExtraerMejoresMarcasPorJugador(todasLasPartidasDelJuego, idJuegoEnviado);

                return rankingMejoresMarcas;
            }
        }

        private static List<Partida> ExtraerMejoresMarcasPorJugador(List<PartidaSQL> todasLasPartidasDelJuego, int idjuego)
        {
            if (todasLasPartidasDelJuego == null || todasLasPartidasDelJuego.Count == 0)
            {
                return new List<Partida>();
            }
            // Variable que va a guardar la mejores marcas de cada jugador, segun el juego
            List<PartidaSQL> rankingMejoresMarcasJuego = new List<PartidaSQL>();

            // En funcion del id del juego, vamos a ver que se la valora mas de los datos de la partidas
            switch (idjuego)
            {
                case 1 or 4:
                    // En el caso de TOPOS, lo que mas valoramos es la puntuacion, asi que ordenamos por puntuacion
                    rankingMejoresMarcasJuego = todasLasPartidasDelJuego
                                                                      .GroupBy(p => p.IdPerfil) // Agrupamos por Jugador (IdPerfil)
                                                                      .Select(grupo => grupo.OrderByDescending(p => p.Puntuacion).First()) // De cada jugador, nos quedamos solo con su partida con más puntos
                                                                      .OrderByDescending(p => p.Puntuacion) // Ordenamos a todos los finalistas de mayor a menor puntos
                                                                      .Take(250) // Nos quedamos solo con los 250 mejores
                                                                      .ToList();
                    break;
                case 2:
                    // 1. Procesamos las partidas para calcular el balance de cada jugador
                    rankingMejoresMarcasJuego = todasLasPartidasDelJuego
                        .GroupBy(p => p.IdPerfil)
                        .Select(grupo => new PartidaSQL // Creamos objetos Partida directamente
                        {
                            IdPerfil = grupo.Key,
                            // Calculamos: (Número de True) - (Número de False)
                            Puntuacion = grupo.Count(p => p.Victoria == true) - grupo.Count(p => p.Victoria == false),
                            IdJuego = 2 // O el ID que corresponda a Wordle
                        })
                        .OrderByDescending(p => p.Puntuacion)
                        .ToList();


                    break;

            }

            // Lista en la que guardamaos la mejores marcas resultantes pero en el modelo no sql
            List<Partida> listaNormalizada = new List<Partida>();

            foreach (PartidaSQL itemSQL in rankingMejoresMarcasJuego)
            {
                listaNormalizada.Add(new Partida
                {
                    IdPerfil = itemSQL.IdPerfil,
                    IdJuego = itemSQL.IdJuego,
                    Puntuacion = itemSQL.Puntuacion,
                    TiempoSegundos = itemSQL.TiempoSegundos,
                    Victoria = itemSQL.Victoria,
                });
            }

            return listaNormalizada;
        }



        #endregion

        #region METODOS DE ACTUALIZACION
        // Este metodo se usara para actualiazar a visotira cuando se gane una aprtida de wordle
        public static void ActualizarPartidaAVictoria(int idPartidaActual)
        {
            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                // 1. Buscamos la partida original que tiene tu IdPerfil
                PartidaSQL partida = conexion.Table<PartidaSQL>().FirstOrDefault(x => x.IdPartida == idPartidaActual);

                // 2. Si la encuentra, SOLO le cambiamos la victoria
                if (partida != null)
                {
                    partida.Victoria = true;

                    // 3. Hacemos el Update del objeto entero, así no se borra tu IdPerfil
                    conexion.Update(partida);
                }
            }

        }
        #endregion

    }
}
