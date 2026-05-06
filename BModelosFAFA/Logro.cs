using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BModelosFAFA
{
    public class Logro
    {
        public int IdLogro { get; set; }
        public int IdJuego { get; set; }
        public string? Nombre { get; set; }
        public string? CondicionDesbloqueo { get; set; }
        public int XpPremio { get; set; }
    }
}
