using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jeesi
{
    // Game-luokka tallentaa yksittäisen pelin tiedot, kuten joukkueet, maalimäärät ja pelitapahtumat.
    public class Game
    {
        public Team? HomeTeam { get; set; }
        public Team? AwayTeam { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int HomeScore { get; set; } = 0;
        public int AwayScore { get; set; } = 0;
        public List<GoalRecord> GoalRecords { get; set; } = new List<GoalRecord>();
        public string EndReason { get; set; } = "Peli päättyi varsinaisen peliajan jälkeen";

    }
}
