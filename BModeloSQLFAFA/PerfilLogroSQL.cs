using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosSQLFAFA
{
    [Table("UsuarioLogro")]
    public class PerfilLogroSQL
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string IdPerfil { get; set; }
        public int IdLogro { get; set; }

        public DateTime FechaObtencion { get; set; }

        public bool Sincronizado { get; set; }

    }
}
