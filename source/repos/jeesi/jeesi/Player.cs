using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jeesi
{
    // Player-luokka edustaa yksittäistä pelaajaa joukkueessa ja sisältää tiedot pelaajan tilastoista.
    public class Player
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int PlayerNumber { get; set; }
        public int Goals { get; set; } = 0;
        public int Assists { get; set; } = 0; 
        public string TeamName { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public string Stats => $"Maalit: {Goals}, Syötöt: {Assists}";

        public override string ToString()
        {
            return $"{FirstName} {LastName} - Maalit: {Goals}, Syötöt: {Assists}";
        }
    }
}
