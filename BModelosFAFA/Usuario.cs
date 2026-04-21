using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosFAFA
{
    public class Usuario
    {
       
        public Usuario(string correo)
        {
            Email = correo;
        }


        public int IdUsuario { get; set; }
        public string Email { get; set; }
        // omitir el Password por seguridad al mover datos
    }
}
