using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosSQLFAFA
{
    [Table("Partida")]
    public class PartidaSQL
    {
        [PrimaryKey]
        public string Uuid { get; set; } // Identificador único para evitar choques en el servidor

        public int IdUsuario { get; set; }
        public int IdJuego { get; set; }

        public int Puntuacion { get; set; }
        public int TiempoSegundos { get; set; }
        public bool Victoria { get; set; }
        public DateTime FechaHora { get; set; }

        // El booleano para saber si ya se subió a la nube
        public bool Sincronizada { get; set; }
    }
}
