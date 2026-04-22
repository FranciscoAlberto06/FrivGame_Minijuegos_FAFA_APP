using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosSQLFAFA
{
    [Table("Logro")]
    public class LogroSQL
    {
        [PrimaryKey, AutoIncrement]
        public int IdLogro { get; set; }
        public int IdJuego { get; set; }
        public string Nombre { get; set; }
        public string CondicionDesbloqueo { get; set; }
        public int XpPremio { get; set; }


        // El booleano para saber si ya se subio a la nube
        public bool Sincronizada { get; set; }
    }
}
