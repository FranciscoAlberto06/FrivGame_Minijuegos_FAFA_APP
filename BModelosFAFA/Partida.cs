using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosFAFA
{
    public class Partida
    {
        public string IdPerfil { get; set; }
        public string NombreUsuario { get; set; }

        public int IdJuego { get; set; }
        public int Puntuacion { get; set; }
        public int TiempoSegundos { get; set; }
        public bool Victoria { get; set; }

        public string ColorFondoRanking { get; set; }

    }
}
