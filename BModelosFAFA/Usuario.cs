using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosFAFA
{
    public class Usuario
    {
       
        public Usuario(string correo, string usuName)
        {
            Email = correo;
            NombreUsuario = usuName;
        }

        public Usuario()
        {
          
        }

        public int IdUsuario { get; set; }
        public string Email { get; set; }
        public string NombreUsuario { get; set; }

        public string Password { get; set; }


        // omitir el Password por seguridad al mover datos
    }
}
