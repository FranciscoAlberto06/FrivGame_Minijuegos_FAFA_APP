using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace BGestionFAFA
{
    public class ApiAivenFAFA
    {
        // Link de conexión a la base de datos en Aiven 
        static string connectionString = "mysql-14c4eb7-finalproyect-dam.i.aivencloud.com;Port=15348;Database=defaultdb;Uid=avnadmin;Pwd=tu-REQUIRED;SslMode=Required";

        // Metodo que va cargar todo los datos nuevo que no tenga sqlite 
        public static void CargarDatosNuevosDesdeAiven()
        {
            MySqlConnection conexion = new MySqlConnection(connectionString);

            // Ahora vamos a pasar todos lo datos que hay nuestro sqlite a la nube, para que se guarden ahi y se puedan recuperar en otros dispositivos
            // se haciendo lo siguiente: 


        }
    }
}
