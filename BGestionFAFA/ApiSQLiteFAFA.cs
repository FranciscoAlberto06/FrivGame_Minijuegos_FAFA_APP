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

        private static void CrearTablasNecesaria(SQLiteConnection conexion)
        {
            // Esto hara que se crren la tablas necesarias si no estan creadas
            conexion.CreateTable<UsuarioSQL>();
            // Esto asegura que el primer id que se reparta no empiece por 0
            conexion.Execute("INSERT OR IGNORE INTO sqlite_sequence (name, seq) VALUES ('Usuario', 0)");
            conexion.CreateTable<UsuarioLogroSQL>();
            conexion.CreateTable<AmistadSQL>();
            conexion.CreateTable<JuegoSQL>();
            conexion.CreateTable<PartidaSQL>();
            conexion.CreateTable<PerfilSQL>();
            conexion.CreateTable<LogroSQL>();

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
                    Reglas = juegoNormal.Reglas
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
                    XpPremio = logro.XpPremio
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
                    Nivel = perfil.Nivel,
                    XpTotal = perfil.XpTotal,
                    AvatarUrl = perfil.AvatarUrl
                };

                //Generamos su UID unico y personal seugno su nombre de usuario
                perfilSQL.PerfilUid = perfilSQL.GenerarUidPerfil(perfil.NombreUsario);
                conexion.Insert(perfilSQL);
            }
        }

        public static void InsertarPartida(Partida partida)
        {
            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                PartidaSQL partidaSQL = new PartidaSQL
                {
                    // Si el Uuid viene vacío desde la app, lo generamos aquí
                    Uuid = string.IsNullOrEmpty(partida.Uuid) ? Guid.NewGuid().ToString() : partida.Uuid,
                    IdUsuario = partida.IdUsuario,
                    IdJuego = partida.IdJuego,
                    Puntuacion = partida.Puntuacion,
                    TiempoSegundos = partida.TiempoSegundos,
                    Victoria = partida.Victoria,
                    FechaHora = partida.FechaHora,
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

            // 2. Sino salta excepcion comprobamos que la contreseña sea correcta
            // 2.1. Sacamos contraseña original

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

        public static int ExtraerIdUsuario(string email)
        {
            int idUsu;

            using (conexion = new SQLiteConnection(rutaCompletaPersonal))
            {
                idUsu = conexion.Table<UsuarioSQL>().Where(u => u.Email == email).Select(u => u.IdUsuario).FirstOrDefault();
            }

            return idUsu;

        }

        #endregion

    }
}
