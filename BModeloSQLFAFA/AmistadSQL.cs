using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosSQLFAFA
{
    
    public enum Estados : byte { PENDIENTE, ACEPTADA, RECHAZADO};

    [Table("Amistad")]
    public class AmistadSQL
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int IdUsuarioSolicitante { get; set; } // El que da a "Añadir"
        public int IdUsuarioReceptor { get; set; }    // El que recibe la notificación

        public Estados Estado { get; set; } // "PENDIENTE", "ACEPTADA", "RECHAZADA"  :)
        public DateTime FechaSolicitud { get; set; }


        // El booleano para saber si ya se subio a la nube
        public bool Sincronizada { get; set; }
    }
}

