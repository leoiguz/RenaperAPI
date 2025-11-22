using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RenaperWeb.Models
{
    public class PersonaViewModel
    {
        public string Apellido { get; set; }
        public string Nombres { get; set; }
        public int DNI { get; set; }
        public string Provincia { get; set; }
        // Agrega solo lo que quieras mostrar en la tabla
    }
}