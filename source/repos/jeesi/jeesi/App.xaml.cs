using System.Collections.ObjectModel;

namespace jeesi
{
    // Sovelluksen App-luokka, joka toimii koko sovelluksen juuriluokkana ja vastaa sovelluksen elinkaaren hallinnasta.
    public partial class App : Application
    {
        public static ObservableCollection<Team> Teams { get; set; } = new ObservableCollection<Team>();

        public App()
        {
            InitializeComponent();

            try
            {
                if (Teams.Count == 0) 
                {
                    var loadedTeams = DataStorage.LoadTeams() ?? new List<Team>();
                    foreach (var team in loadedTeams)
                    {
                        Teams.Add(team);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe joukkueiden latauksessa: {ex.Message}");
            }

            MainPage = new NavigationPage(new MainPage());
        }

        public static event Action? GamesUpdated;

        public static void NotifyGamesUpdated()
        {
            GamesUpdated?.Invoke();
        }

    }
}
