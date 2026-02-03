
using System.Collections.ObjectModel;
using System.Text;

namespace jeesi
{
    // MainPage toimii sovelluksen pääsivuna, joka näyttää viimeisimmät pelit, pistepörssin ja mahdollistaa navigoinnin muihin sivuihin.
    public partial class MainPage : ContentPage
    {
        // ObservableCollection pitää sisällään tiedot peleistä ja pelaajista, jotka näkyvät käyttöliittymässä.
        public ObservableCollection<Game> Games { get; set; } = new ObservableCollection<Game>();
        public ObservableCollection<Player> TopPlayers { get; set; } = new ObservableCollection<Player>();

        // Konstruktori: alustaa pääsivun ja lataa pelit ja pelaajat.
        public MainPage()
        {
            InitializeComponent();
            LoadGames(); // Lataa tallennetut pelitiedot.
            LoadPlayers(); // Lataa ja järjestää pelaajat tilastojen perusteella.
        }

        // Avaa uuden pelin luontiin tarkoitetun sivun modaalisena ikkunana.
        private async void OnOpenGameClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NewGame());
        }

        // Päivittää pelit, kun GamesUpdated-tapahtuma kutsutaan.
        private void OnGamesUpdated()
        {
            LoadGames();
        }

        // Poistaa GamesUpdated-tapahtuman rekisteröinnin, kun sivu suljetaan.
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            App.GamesUpdated -= OnGamesUpdated;
        }

        // Avaa joukkueiden hallintasivun modaalisena, jossa voidaan lisätä, poistaa ja muokata joukkueita.      
        private async void OnOpenTeamManagementClicked(object sender, EventArgs e)
        {
            // Avaa modaalisena sivuna
            await Navigation.PushModalAsync(new TeamManagementPage(App.Teams));
        }

        // Käynnistää uuden pelin annetun lajin perusteella.
        private async void OnSportSelected(string sport)
        {
            try
            {
                if (App.Teams == null || App.Teams.Count == 0)
                {
                    await DisplayAlert("Virhe", "Joukkueita ei ole ladattu. Lisää joukkueita ennen pelin aloittamista.", "OK");
                    return;
                }

                await Navigation.PushModalAsync(new GamePage(sport, 3, 20, false, App.Teams));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe OnSportSelected-metodissa: {ex.Message}");
                await DisplayAlert("Virhe", $"Tapahtui virhe: {ex.Message}", "OK");
            }
        }

        // Lataa pelit tiedostosta ja päivittää ne käyttöliittymään.
        public void LoadGames()
        {
            Games.Clear();
            foreach (var game in DataStorage.LoadGames())
            {
                Games.Add(game);
            }

            UpdateResultsLabel();
        }

        // Lataa pelaajat kaikista joukkueista ja järjestää tilastojen mukaan.
        private void LoadPlayers()
        {
            TopPlayers.Clear();

            var allPlayers = App.Teams.SelectMany(team => team.Players);

            var sortedPlayers = allPlayers
                .OrderByDescending(p => p.Goals)  
                .ThenByDescending(p => p.Assists);

            foreach (var player in sortedPlayers)
            {
                TopPlayers.Add(player);
            }

            PlayersListView.ItemsSource = TopPlayers;
        }

        // Lataa pelaajat ja päivittää ListView:n, kun sivu avataan.
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadPlayers();
            PlayersListView.ItemsSource = TopPlayers;
        }

        // Päivittää tulokset sisältävän tekstikomponentin (ResultsLabel)
        private void UpdateResultsLabel()
        {
            if (Games.Count == 0)
            {
                ResultsLabel.Text = "Ei pelattuja pelejä.";
                return;
            }

            
            var recentGames = Games
                .OrderByDescending(game => game.StartTime) 
                .Take(10); 

            StringBuilder resultsBuilder = new StringBuilder();
            foreach (var game in recentGames)
            {
                if (game.HomeTeam != null && game.AwayTeam != null) 
                {
                    string result = $"{game.StartTime:dd.MM.yyyy HH:mm} - {game.HomeTeam.TeamName} {game.HomeScore} vs. {game.AwayScore} {game.AwayTeam.TeamName}";
                    if (!string.IsNullOrEmpty(game.EndReason))
                    {
                        result += $" ({game.EndReason})";
                    }

                    resultsBuilder.AppendLine(result);
                }
            }

            ResultsLabel.Text = resultsBuilder.ToString();
            ResultsLabel.FontSize = 12; 
        }
    }
}


    