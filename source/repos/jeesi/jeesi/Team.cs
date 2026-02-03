using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jeesi
{
    public class Team
    {
        public string TeamName { get; set; } = string.Empty; // Oletusarvo
        public ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>();
        public int Wins { get; set; } = 0;
        public int Draws { get; set; } = 0;
        public int Losses { get; set; } = 0;

        public override string ToString() => TeamName;
    }
}
