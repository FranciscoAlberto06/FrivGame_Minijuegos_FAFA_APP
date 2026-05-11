using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BModelosFAFA
{
    public class Usuario
    {

        string _email;
        string _nombreUsuario;
        string _password;

        public Usuario(string correo, string usuName)
        {
            Email = correo;
            NombreUsuario = usuName;
        }

        public Usuario()
        {
          
        }

        public int IdUsuario { get; set; }
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El email no puede estar vacío.");

                ComprobarEmail(value.Trim());
                _email = value.Trim();
            }
        }
        public string NombreUsuario {
            get 
            {

                return _nombreUsuario;

            }
            set
            {
                // 1. Validar nulo o vacío
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre de usuario no puede estar vacío.");

                string valorLimpio = value.Trim();

                // 2. Validar rango (mínimo 3, máximo 20 por ejemplo)
                if (valorLimpio.Length < 3 || valorLimpio.Length > 20)
                    throw new ArgumentException("El nombre de usuario debe tener entre 3 y 20 caracteres.");

                _nombreUsuario = valorLimpio;
            }
        
        }

        private void ComprobarEmail(string email)
        {
            // Expresión regular para validar el formato del correo electronico
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            // Validar el formato del correo electrónico utilizando la expresión regular
            if (!Regex.IsMatch(email, patron))
            {
                throw new Exception("El formato del correo electrónico no es válido.");
            }
        
        }

        public string Password {
            get; set;
        }


    }
}
