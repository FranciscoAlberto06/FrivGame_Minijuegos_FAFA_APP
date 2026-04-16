using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosSQLFAFA
{
    [Table("Juego")]
    public class JuegoSQL
    {
        [PrimaryKey, AutoIncrement]
        public int IdJuego { get; set; }
        public string Nombre { get; set; }
        public string Reglas { get; set; }
    }
}
