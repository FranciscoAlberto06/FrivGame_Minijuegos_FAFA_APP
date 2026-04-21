using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosSQLFAFA
{
    [Table("Perfil")]
    public class PerfilSQL
    {
        [PrimaryKey]
        // Al ser un ID personalizado de texto, NO usamos AutoIncrement
        public string PerfilUid { get; set; }
        public int IdUsuario { get; set; }
        public int Nivel { get; set; }
        public int XpTotal { get; set; }
        public string AvatarUrl { get; set; }

        public string NombreUsuario { get; set; }


        //public PerfilSQL()

        public string GenerarUidPerfil(string username)
        {
            string prefijo = username.Substring(0, Math.Min(3, username.Length)).ToUpper();

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            return $"{prefijo}-{timestamp}";
            // Resultado: "PEPE-20240520143005"
        }

    }
}
