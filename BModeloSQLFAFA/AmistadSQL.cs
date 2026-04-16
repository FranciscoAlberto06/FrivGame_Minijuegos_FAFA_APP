using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosSQLFAFA
{
    [Table("Amistad")]
    public class AmistadSQL
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int IdUsuarioSolicitante { get; set; } // El que da a "Añadir"
        public int IdUsuarioReceptor { get; set; }    // El que recibe la notificación

        public string Estado { get; set; } // "PENDIENTE", "ACEPTADA", "RECHAZADA"
        public DateTime FechaSolicitud { get; set; }
    }
}
}
