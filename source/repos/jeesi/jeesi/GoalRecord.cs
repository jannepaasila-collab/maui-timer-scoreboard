using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jeesi
{
    // GoalRecord-luokka tallentaa yksittäisen maalin tiedot, kuten maalintekijän, syöttäjän ja maalin ajan.
    public class GoalRecord
    {
        public Player? Scorer { get; set; }
        public Player? Assist { get; set; } 
        public DateTime GoalTime { get; set; }

        public override string ToString()
        {
            return $"{GoalTime:HH:mm:ss} - Maalintekijä: {Scorer.FirstName} {Scorer.LastName}" +
                   (Assist != null ? $", Syöttäjä: {Assist.FirstName} {Assist.LastName}" : "");
        }
    }
}
