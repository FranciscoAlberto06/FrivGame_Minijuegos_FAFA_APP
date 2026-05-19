using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BGestionFAFA
{
    public class ApiWordleFAFA
    {

        private static List<string> listaPalabras;

        // Generador de numeros aleatorios
        private static Random generadorAleatorio = new Random();
        private static string palabraConAcentos = "";

        // Limpiamos los acentos usando Replace
        public static string QuitarAcentos(string texto)
        {
            // Lo pasamos a mayusculas y le quitamos los espacios al principio y al final
            string textoLimpio = texto.ToUpper().Trim();

            // Reemplazamos cada vocal acentuada por su equivalente sin acento, y tambien las vocales con dieresis
            return textoLimpio.Replace('Á', 'A')
                              .Replace('É', 'E')
                              .Replace('Í', 'I')
                              .Replace('Ó', 'O')
                              .Replace('Ú', 'U')
                              .Replace('Ä', 'A')
                              .Replace('Ë', 'E')
                              .Replace('Ï', 'I')
                              .Replace('Ö', 'O')
                              .Replace('Ü', 'U');
        }

        // 2. Descargamos y preparamos nuestra unica lista 
        public async static Task InicializarDiccionario()
        {

            try
            {
                // 1. Inicializamos la lista de palabras
                listaPalabras = new List<string>();

                // 2. Abrimos el archivo incrustado en Resources/Raw/diccionario.txt de manera local
                using (Stream stream = await FileSystem.OpenAppPackageFileAsync("diccionario.txt"))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string linea;
                        // Leemos línea por línea de manera eficiente
                        while ((linea = await reader.ReadLineAsync()) != null)
                        {
                            // Reemplazamos cualquier residuo de salto de línea invisible antes de medir su longitud
                            string palabra = linea.Replace("\r", "").Replace("\n", "").Trim();

                            // Ahora el filtro de longitud medirá los caracteres reales exactos
                            if (palabra.Length >= 4 && palabra.Length <= 6)
                            {
                                string palabraLimpia = QuitarAcentos(palabra).ToUpper();
                                listaPalabras.Add(palabraLimpia);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Si el archivo no se encuentra o está mal configurado saltará aquí
                System.Diagnostics.Debug.WriteLine("Error al cargar el diccionario local: " + ex.Message);
            }

        }

        // Metodo para extraer una palabra aleatoria
        public async  static Task<string> ObtenerPalabraAleatoria()
        {// Solo descargamos si la lista está vacía
            if (listaPalabras == null || listaPalabras.Count == 0)
            {
                // Cargamos nuestra lista de palabras sacadas de una fuente online 
                await InicializarDiccionario();
            }


            // Sacamos un numero aletiorio mediante nuestro generador en el rango del numero de palabras que tenemos en nuestra lista
            int indiceAleatorio = generadorAleatorio.Next(0, listaPalabras.Count);

            // Devolvemos la palabras aletaoria elegida en la lsita
            return listaPalabras[indiceAleatorio];
        }

        // Metodo para comprobar si el intento del usuario es una palabra real
        public static bool ComprobarSiExiste(string intentoPalabraActual)
        {

            // Comprobamos si la palabra esta en nuestra lista de palabras validas
            return listaPalabras.Contains(intentoPalabraActual);
        }



        public static string CargarPalabraOffline()
        {
            // Creamos un listado de palabras local para cuando no haya conexion a internet
            listaPalabras = ["CODIGO", "MAUI", "MOVIL", "JUEGO", "PERRO", "PLATO"];

            // Seleccionamos una palabra random dentro del rango de opcione que tiene nuestra lista de palabras
            return listaPalabras[generadorAleatorio.Next(listaPalabras.Count)];

        }

        public static async Task<string> ObtenerSignificado(string palabra)
        {
            // Valor por defecto en caso de que no se pueda obtener el significado
            string resultado = "No disponible el significado de " + palabra;

            // Usamos HttpClient para hacer la petición a la API de Wikipedia
            using (HttpClient cliente = new HttpClient())
            {
                // El User-Agent es necesario para que la RAE no bloquee la peticion
                cliente.DefaultRequestHeaders.Add("User-Agent", "MauiWordleApp/1.0");

                // URL de la API de la RAE para obtener uno de los significados de la pagina de la palabra
                string url = "https://rae-api.com/api/words/" + palabra.ToLower();

                try
                {

                    // Hacemos la petición GET a la API
                    HttpResponseMessage respuesta = await cliente.GetAsync(url);


                    // Si la respuesta es exitosa, procesamos el JSON recibido
                    if (respuesta.IsSuccessStatusCode)
                    {
                        // Leemos el contenido de la respuesta como una cadena JSON
                        string jsonRecibido = await respuesta.Content.ReadAsStringAsync();

                        // Usamos JsonDocument para parsear el JSON y extraer el campo "extract" que contiene el resumen de la palabra
                        using (JsonDocument documento = JsonDocument.Parse(jsonRecibido))
                        {
                            // Navegamos por la estructura: data -> meanings[0] -> senses[0]
                            JsonElement senseObtenidos = documento.RootElement
                                        .GetProperty("data")
                                        .GetProperty("meanings")[0]
                                        .GetProperty("senses");

                            // Comprobamos que no sea nullo el valor devuelto
                            if (senseObtenidos.ValueKind != JsonValueKind.Null)
                            { 
                                if (senseObtenidos.GetArrayLength() >= 3)
                                {
                                    resultado = $"Significados de {palabra.ToUpper()}:";
                                    for (int i = 0; i < 3; i++)
                                    {
                                        if (senseObtenidos[i].TryGetProperty("description", out JsonElement definicion))
                                        {
                                            string definicionTexto = definicion.GetString();
                                            if (!string.IsNullOrEmpty(definicionTexto))
                                            {
                                                resultado += $"\n{i + 1}. {definicionTexto}";
                                            }
                                        }
                                    }
                                }
                                else if (senseObtenidos.GetArrayLength() > 0)
                                {
                                    // Si hay al menos un significado, lo mostramos
                                    string descripcion = senseObtenidos[0].GetProperty("description").GetString();

                                    if (!string.IsNullOrEmpty(descripcion))
                                    {
                                        resultado = $"{palabra.ToUpper()}: " + descripcion;
                                    }
                                }
                            }
                        }

                    }

                }
                catch (Exception)
                {
                    resultado = "Error al conectar para obtener el significado.";
                }

                return resultado;
            }
        }



    }
}
