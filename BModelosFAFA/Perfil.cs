using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosFAFA
{
    public class Perfil
    {
        private int _xptotal;
        private int _nivel;

        public Perfil(int idusu, string usu, string avatar) 
        {
        
            IdUsuario = idusu;
            Nivel = 1;
            XpTotal = 0;
            AvatarUrl = avatar;
            NombreUsuario = usu;
        
        }
        public Perfil()
        {



        }

        public string PerfilUid { get; set; }

        public int IdUsuario { get; set; }
        public int Nivel { 
            get {


                return _nivel;

            } 
            set {
            
                _nivel = value;
            
            }
        
        
        }
        public int XpTotal {
            get
            {
                return _xptotal;
            }
            set {

                // Primero asignamos el valor para acumular primero la nueva xp que llega
                _xptotal = value;

                // Mientras que el xp total sea mayor a 1000 le bajamos y subimos sus niveles nuevos
                while (_xptotal >= 1000)
                {
                    _nivel++;
                    _xptotal -= 1000;
                }

            }
        }
        public string? AvatarUrl { get; set; }
        public string NombreUsuario { get; set; }

        // Esta propiedad no se guarda en la bd solo se usa para poder controlar el largo del progress bar segun la exp
        public float BarraProgreso => _xptotal / 1000f;

    }
}
