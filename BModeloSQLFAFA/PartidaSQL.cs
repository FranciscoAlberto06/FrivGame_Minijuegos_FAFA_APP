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
        public int IdPartida { get; set; } // Identificador único para evitar choques en el servidor

        public int IdJuego { get; set; }

        public string IdPerfil { get; set; }
        public int Puntuacion { get; set; }
        public int TiempoSegundos { get; set; }
        public bool Victoria { get; set; }
        public DateTime FechaHora { get; set; } = DateTime.Now; // Que se asigne la fecha y hora actual al crear una nueva partida

        // El booleano para saber si ya se subio a la nube
        public bool Sincronizada { get; set; }
    }
}
