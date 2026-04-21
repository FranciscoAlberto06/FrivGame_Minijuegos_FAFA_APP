using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosFAFA
{
    public class Perfil
    {
        public Perfil(int idusu, string avatar, string usu) 
        {
        
            IdUsuario = idusu;
            Nivel = 1;
            XpTotal = 0;
            AvatarUrl = avatar;
            NombreUsario = usu;
        
        }

        public int IdUsuario { get; set; }
        public int Nivel { get; set; }
        public int XpTotal { get; set; }
        public string AvatarUrl { get; set; }
        public string NombreUsario { get; set; }
    }
}
