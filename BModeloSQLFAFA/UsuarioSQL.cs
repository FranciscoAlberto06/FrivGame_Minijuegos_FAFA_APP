using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosSQLFAFA
{
    [Table("Usuario")]
    public class UsuarioSQL
    {
        [PrimaryKey, AutoIncrement]
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
