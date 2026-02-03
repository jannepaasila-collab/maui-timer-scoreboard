using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Timers;
using Microsoft.Maui.Controls;
using System.Diagnostics;


namespace jeesi;

// GamePage hallitsee pelin kulkua, joukkueita ja ajanottoa.
public partial class GamePage : ContentPage, INotifyPropertyChanged
{
    // Tapahtuma, joka ilmoittaa, kun jokin ominaisuus muuttuu
    public new event PropertyChangedEventHandler? PropertyChanged = delegate { };

    // K‰yttˆliittym‰‰n sidottava pelin tiedot.
    private string _gameInfo = string.Empty;
    public string GameInfo
    {
        get => _gameInfo;
        set
        {
            if (_gameInfo != value) 
            {
                _gameInfo = value;
                OnPropertyChanged(nameof(GameInfo));
            }            
        }
    }

    // Pelin nykyinen tilanne ja osallistuvat joukkueet.
    private Game? CurrentGame { get; set; } 
    public ObservableCollection<Team> Teams { get; set; }
    public ObservableCollection<Game> Games { get; set; } = new ObservableCollection<Game>();

    // Pelin parametrit.
    private int Periods;
    private int TimePerPeriod;
    private bool TimeIncreases;
    private int CurrentPeriod;
    private int TimeRemaining;

    // Ajastin peliaikaa varten.
    private System.Timers.Timer timer;

    // GamePage-konstruktori pelin alustamiseen ja joukkueiden asettamiseen.
    public GamePage(string sport, int periods, int timePerPeriod, bool timeIncreases, ObservableCollection<Team> teams)
    {
        if (string.IsNullOrEmpty(sport))
        {
            throw new ArgumentException("Laji ei voi olla tyhj‰", nameof(sport));
        }
        if (teams == null || teams.Count == 0)
        {
            throw new ArgumentException("Joukkueita ei ole saatavilla", nameof(teams));
        }
        InitializeComponent();

        // Debug-tietoa pelin parametreista.
        Debug.WriteLine($"Laji: {sport}");
        Debug.WriteLine($"Erien m‰‰r‰: {periods}");
        Debug.WriteLine($"Aika per er‰: {timePerPeriod}");
        Debug.WriteLine($"Aika kasvaa: {timeIncreases}");
        Debug.WriteLine($"Joukkueiden m‰‰r‰: {teams?.Count ?? 0}");

        // Joukkueiden ja pelien asettaminen.
        Teams = teams;  
        Games = new ObservableCollection<Game>(DataStorage.LoadGames());
        BindingContext = this;

        GameInfo = $"{sport}: Er‰ {CurrentPeriod}/{Periods}";

        // Sidotaan joukkueet Picker-komponentteihin
        HomeTeamPicker.ItemsSource = Teams;
        AwayTeamPicker.ItemsSource = Teams;

        // Erien ja ajan parametrit
        this.Periods = periods;
        this.TimePerPeriod = timePerPeriod;
        this.TimeIncreases = timeIncreases;
        this.CurrentPeriod = 1;
        this.TimeRemaining = TimeIncreases ? 0 : TimePerPeriod * 60;

        GoalRecordsListView.ItemsSource = CurrentGame?.GoalRecords;

        // Ajastimen asetukset
        timer = new System.Timers.Timer(1000);
        timer.Elapsed += OnTimerElapsed;

        UpdateGameInfo();
    }

    // Ilmoittaa sidontaj‰rjestelm‰lle, ett‰ ominaisuus on muuttunut.
    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // P‰ivitt‰‰ pelin tiedot k‰yttˆliittym‰‰n.
    private void UpdateGameInfo()
    {
        if (CurrentGame?.HomeTeam != null && CurrentGame?.AwayTeam != null)
        {
            GameInfo = $"Peli k‰ynniss‰: {CurrentGame.HomeTeam.TeamName} {CurrentGame.HomeScore} - {CurrentGame.AwayScore} {CurrentGame.AwayTeam.TeamName}";

            HomeTeamLabel.Text = $"{CurrentGame.HomeTeam.TeamName}: {CurrentGame.HomeScore}";
            AwayTeamLabel.Text = $"{CurrentGame.AwayTeam.TeamName}: {CurrentGame.AwayScore}";
        }
        else
        {
            GameInfo = $"Er‰ {CurrentPeriod}/{Periods}";
        }
    }

    // Aloittaa pelin ja lukitsee joukkuevalinnat.
    private void OnStartGameClicked(object sender, EventArgs e)
    {
        if (HomeTeamPicker.SelectedItem is Team homeTeam && AwayTeamPicker.SelectedItem is Team awayTeam)
        {
            HomeTeamPicker.IsEnabled = false;
            AwayTeamPicker.IsEnabled = false;


            CurrentGame = new Game
            {
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                StartTime = DateTime.Now
            };

            GoalRecordsListView.ItemsSource = CurrentGame.GoalRecords;

            HomeTeamLabel.Text = $"{homeTeam.TeamName}: {CurrentGame.HomeScore}";
            AwayTeamLabel.Text = $"{awayTeam.TeamName}: {CurrentGame.AwayScore}";

            UpdateGameInfo();

        }
        else
        {
            DisplayAlert("Virhe", "Valitse molemmat joukkueet.", "OK");
        }
    }

    // K‰ynnist‰‰ ajastimen.
    private void OnStartClicked(object sender, EventArgs e)
    {
        if (HomeTeamPicker.SelectedItem == null || AwayTeamPicker.SelectedItem == null)
        {
            DisplayAlert("Virhe", "Valitse joukkueet ennen kellon k‰ynnist‰mist‰.", "OK");
            return;
        }

        if (HomeTeamPicker.IsEnabled || AwayTeamPicker.IsEnabled)
        {
            DisplayAlert("Virhe", "Lukitse joukkueet ennen kellon k‰ynnist‰mist‰.", "OK");
            return;
        }

        if (!timer.Enabled)
        {
            timer.Start();
        }
    }

    // Pys‰ytt‰‰ ajastimen.
    private void OnStopClicked(object sender, EventArgs e)
    {
        if (timer.Enabled)
        {
            timer.Stop();
        }
    }

    // Painike pelin asetusten nollaamiseen (ei nollaa lajivalintaa)
    private void OnResetClicked(object sender, EventArgs e)
    {
        CurrentGame = null;
        HomeTeamPicker.SelectedItem = null;
        AwayTeamPicker.SelectedItem = null;

        CurrentPeriod = 1;
        TimeRemaining = TimeIncreases ? 0 : TimePerPeriod * 60;
        UpdateTimerLabel();

        // Nollataan myˆs TeamLabel-arvot
        HomeTeamLabel.Text = "Kotijoukkue: 0";
        AwayTeamLabel.Text = "Vierasjoukkue: 0";

        HomeTeamPicker.IsEnabled = true;
        AwayTeamPicker.IsEnabled = true;
    }

    // Lopettaa pelin ja tallentaa tulokset.
    private async void OnEndGameClicked(object sender, EventArgs e)
    {
        if (CurrentGame != null)
        {
            string? endReason = null;

            if (TimeRemaining > 0)
            {
                bool confirm = await DisplayAlert(
                    "Vahvistus",
                    "Oletko varma, ett‰ haluat lopettaa pelin ennen varsinaisen peliajan loppua?",
                    "Kyll‰",
                    "Ei");

                if (!confirm) return;

                endReason = "Peli keskeytettiin, Lopputulos j‰‰ voimaan.";
            }

            timer.Stop();
            CurrentGame.EndTime = DateTime.Now;
            CurrentGame.EndReason = endReason;
            Games.Add(CurrentGame);
            DataStorage.SaveGames(Games.ToList());
            SaveGame();

            string resultMessage = $"Lopputulos: {CurrentGame.HomeTeam?.TeamName} {CurrentGame.HomeScore} - {CurrentGame.AwayScore} {CurrentGame.AwayTeam?.TeamName}";

            if (!string.IsNullOrEmpty(endReason))
            {
                resultMessage = $"{endReason}\n{resultMessage}";
            }

            await DisplayAlert("Peli p‰‰ttyi", resultMessage, "OK");

            ResetGame(); // Nollataan peli ja visuaaliset elementit
        }
        else
        {
            await DisplayAlert("Virhe", "Peli‰ ei ole aloitettu.", "OK");
        }
    }

    private void ResetGame()
    {
        CurrentGame = null;
        HomeTeamPicker.SelectedItem = null;
        AwayTeamPicker.SelectedItem = null;

        CurrentPeriod = 1;
        TimeRemaining = TimeIncreases ? 0 : TimePerPeriod * 60;
        UpdateTimerLabel();

        // Nollataan myˆs TeamLabel- ja GameInfoLabel-arvot
        HomeTeamLabel.Text = "Kotijoukkue: 0";
        AwayTeamLabel.Text = "Vierasjoukkue: 0";
        GameInfoLabel.Text = "Peli ei ole k‰ynniss‰";
        GameStatsLabel.Text = "Ei viel‰ maaleja"; // Tyhjenn‰ tilastot

        HomeTeamPicker.IsEnabled = true;
        AwayTeamPicker.IsEnabled = true;
    }

    // Ajastimen konfiguraatio
    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (TimeIncreases)
        {
            TimeRemaining++;
        }
        else
        {
            TimeRemaining--;
        }

        if ((TimeRemaining == 0 && !TimeIncreases) || (TimeRemaining == TimePerPeriod * 60 && TimeIncreases))
        {
            timer.Stop();

            int currentPeriodToShow = CurrentPeriod;

            Dispatcher.Dispatch(() =>
            {
                DisplayAlert("Er‰ p‰‰ttyi", $"Er‰ {currentPeriodToShow} p‰‰ttyi!", "OK");
            });

            if (CurrentPeriod < Periods)
            {
                CurrentPeriod++;
                TimeRemaining = TimeIncreases ? 0 : TimePerPeriod * 60;

                Dispatcher.Dispatch(() =>
                {
                    GameInfoLabel.Text = $"Er‰ {CurrentPeriod}/{Periods}";
                    UpdateTimerLabel();
                });
            }
            else
            {
                Dispatcher.Dispatch(() =>
                {
                    EndGameAutomatically();
                });
            }
        }

        Dispatcher.Dispatch(UpdateTimerLabel);
    }

    private void UpdateTimerLabel()
    {
        var minutes = TimeRemaining / 60;
        var seconds = TimeRemaining % 60;
        TimerLabel.Text = $"{minutes:D2}:{seconds:D2}";
    }

    // Kellon nopeutus testaamisen helpottamiseksi
    private void OnSpeedUpClicked(object sender, EventArgs e) 
    {
        if (timer.Interval > 1)
        {
            timer.Interval -= 999;
            UpdateSpeedLabel();
        }
    }

    // Kellon hidastus normaaliksi
    private void OnSlowDownClicked(object sender, EventArgs e)
    {
        if (timer.Interval < 1000)
        {
            timer.Interval += 999;
            UpdateSpeedLabel();
        }
    }
    // P‰ivitt‰‰ SpeedLabelin n‰ytt‰m‰‰n ajastimen nopeuden
    private void UpdateSpeedLabel() 
    {
        SpeedLabel.Text = $"Ajastimen nopeus: {timer.Interval} ms";
    }

    // Kotijoukkueen maalipainike ja muut siihen tarvittavat toiminnot tallennuksineen
    private async void OnAddHomeGoalClicked(object sender, EventArgs e)
    {
        if (timer.Enabled)
        {
            timer.Stop();
        }

        if (CurrentGame?.HomeTeam?.Players == null || CurrentGame.HomeTeam.Players.Count == 0)
        {
            await DisplayAlert("Virhe", "Kotijoukkueen pelaajia ei ole saatavilla.", "OK");
            return;
        }

        var scorer = await SelectPlayerAsync(CurrentGame.HomeTeam.Players, "Valitse maalintekij‰ kotijoukkueelle");
        if (scorer == null)
        {
            await DisplayAlert("Virhe", "Maalintekij‰‰ ei valittu.", "OK");
            return;
        }

        var assist = await SelectPlayerAsync(CurrentGame.HomeTeam.Players, "Valitse syˆtt‰j‰ kotijoukkueelle (valinnainen)");
        if (assist != null && assist == scorer)
        {
            await DisplayAlert("Virhe", "Syˆtt‰j‰ ei voi olla sama kuin maalintekij‰.", "OK");
            return;
        }

        var goalRecord = new GoalRecord
        {
            Scorer = scorer,
            Assist = assist,
            GoalTime = DateTime.Now
        };

        CurrentGame.GoalRecords.Add(goalRecord);
        CurrentGame.HomeScore++;
        UpdateGameInfo();
        UpdateGameStatsLabel();

        scorer.Goals++;
        if (assist != null)
        {
            assist.Assists++;
        }

        UpdateGameInfo();

        await DisplayAlert("Maali", $"Kotijoukkue teki maalin!\n{goalRecord}", "OK");

        if (!timer.Enabled)
        {
            timer.Start();
        }
    }
    // P‰ivitt‰‰ pelin tilanteen "tulostaululle"
    private void UpdateGameStatsLabel()
    {
        if (GameStatsLabel == null || CurrentGame?.GoalRecords == null || CurrentGame.GoalRecords.Count == 0)
        {
            GameStatsLabel.Text = "Ei viel‰ maaleja.";
            return;
        }

        var statsBuilder = new StringBuilder();
        foreach (var goal in CurrentGame.GoalRecords)
        {
            string assistInfo = goal.Assist != null ? $", Syˆtt‰j‰: {goal.Assist.FirstName} {goal.Assist.LastName}" : ", Ei syˆtt‰j‰‰";
            statsBuilder.AppendLine($"{goal.GoalTime:HH:mm:ss} - Maalintekij‰: {goal.Scorer.FirstName} {goal.Scorer.LastName}{assistInfo}");
        }

        GameStatsLabel.Text = statsBuilder.ToString();
    }

    // T‰m‰ metodi n‰ytt‰‰ k‰ytt‰j‰lle valikon pelaajan valitsemista varten ja palauttaa valitun pelaajan.
    private async Task<Player?> SelectPlayerAsync(IEnumerable<Player> players, string title)
    {
        var options = players.Select(p => $"{p.FirstName} {p.LastName}").ToArray();

        if (options.Length == 0)
        {
            await DisplayAlert("Virhe", "Pelaajia ei ole valittavissa.", "OK");
            return null;
        }

        string selectedOption = await DisplayActionSheet(title, "Peruuta", null, options);

        if (string.IsNullOrEmpty(selectedOption) || selectedOption == "Peruuta")
        {
            return null;
        }

        return players.FirstOrDefault(p => $"{p.FirstName} {p.LastName}" == selectedOption);
    }



    // Vierasjoukkueen maalipainike ja muut siihen tarvittavat toiminnot tallennuksineen
    private async void OnAddAwayGoalClicked(object sender, EventArgs e)
    {
        if (timer.Enabled)
        {
            timer.Stop();
        }

        if (CurrentGame?.AwayTeam?.Players == null || CurrentGame.AwayTeam.Players.Count == 0)
        {
            await DisplayAlert("Virhe", "Vierasjoukkueen pelaajia ei ole saatavilla.", "OK");
            return;
        }

        var scorer = await SelectPlayerAsync(CurrentGame.AwayTeam.Players, "Valitse maalintekij‰ vierasjoukkueelle");
        if (scorer == null)
        {
            await DisplayAlert("Virhe", "Maalintekij‰‰ ei valittu.", "OK");
            return;
        }

        var assist = await SelectPlayerAsync(CurrentGame.AwayTeam.Players, "Valitse syˆtt‰j‰ vierasjoukkueelle (valinnainen)");
        if (assist != null && assist == scorer)
        {
            await DisplayAlert("Virhe", "Syˆtt‰j‰ ei voi olla sama kuin maalintekij‰.", "OK");
            return;
        }

        var goalRecord = new GoalRecord
        {
            Scorer = scorer,
            Assist = assist,
            GoalTime = DateTime.Now
        };

        CurrentGame.GoalRecords.Add(goalRecord);
        CurrentGame.AwayScore++;
        UpdateGameInfo();
        UpdateGameStatsLabel();
        
        scorer.Goals++;
        if (assist != null)
        {
            assist.Assists++;
        }

        UpdateGameInfo();
        
        await DisplayAlert("Maali", $"Vierasjoukkue teki maalin!\n{goalRecord}", "OK");

        if (!timer.Enabled)
        {
            timer.Start();
        }
    }
    // Toiminnot ja tietojen tallennus kun peli p‰‰ttyy normaalisti ilman ett‰ se keskeytet‰‰n
    private void EndGameAutomatically()
    {
        if (CurrentGame != null)
        {
            CurrentGame.EndTime = DateTime.Now;

            if (CurrentGame.HomeTeam != null)
            {
                var homeTeam = App.Teams.FirstOrDefault(t => t.TeamName == CurrentGame.HomeTeam.TeamName);
                if (homeTeam != null)
                {
                    homeTeam.Players = CurrentGame.HomeTeam.Players;
                }
            }

            if (CurrentGame.AwayTeam != null)
            {
                var awayTeam = App.Teams.FirstOrDefault(t => t.TeamName == CurrentGame.AwayTeam.TeamName);
                if (awayTeam != null)
                {
                    awayTeam.Players = CurrentGame.AwayTeam.Players;
                }
            }

            DataStorage.SaveTeams(App.Teams.ToList());

            Games.Add(CurrentGame);
            DataStorage.SaveGames(Games.ToList());

            string resultMessage = $"Peli p‰‰ttyi normaalisti!\n" +
                                   $"Lopputulos: {CurrentGame.HomeTeam?.TeamName} {CurrentGame.HomeScore} - {CurrentGame.AwayScore} {CurrentGame.AwayTeam?.TeamName}";
            DisplayAlert("Peli p‰‰ttyi", resultMessage, "OK");

            ResetGame();
        }
        else
        {
            DisplayAlert("Virhe", "Peli‰ ei ole aloitettu.", "OK");
        }
    }

    // Sulje modaalinen sivu
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        try
        {
            if (Application.Current?.MainPage?.Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync();
            }
            else
            {
                await DisplayAlert("Virhe", "Ei lˆydy suljettavia sivuja.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Virhe", $"Navigointi ep‰onnistui: {ex.Message}", "OK");
        }
    }

    // N‰ytt‰‰ pelin tilanteen
    public string HomeTeamStatus
    {
        get => CurrentGame != null ? $"{CurrentGame.HomeTeam?.TeamName}: {CurrentGame.HomeScore}" : "Kotijoukkue: 0";
    }

    public string AwayTeamStatus
    {
        get => CurrentGame != null ? $"{CurrentGame.AwayTeam?.TeamName}: {CurrentGame.AwayScore}" : "Vierasjoukkue: 0";
    }

    // Tallentaa pelin tiedot 
    private void SaveGame()
    {
        if (CurrentGame == null)
        {
            return;
        }

        CurrentGame.EndTime = DateTime.Now;

        DataStorage.SaveGames(Games.ToList());

        if (CurrentGame.HomeTeam != null)
        {
            var homeTeam = App.Teams.FirstOrDefault(t => t.TeamName == CurrentGame.HomeTeam.TeamName);
            if (homeTeam != null)
            {
                homeTeam.Players = CurrentGame.HomeTeam.Players;
            }
        }

        if (CurrentGame.AwayTeam != null)
        {
            var awayTeam = App.Teams.FirstOrDefault(t => t.TeamName == CurrentGame.AwayTeam.TeamName);
            if (awayTeam != null)
            {
                awayTeam.Players = CurrentGame.AwayTeam.Players;
            }
        }

        DataStorage.SaveTeams(App.Teams.ToList());

        var mainPage = Application.Current?.MainPage as MainPage;
        if (mainPage != null)
        {
            mainPage.LoadGames();
        }

        if (CurrentGame?.HomeTeam == null || CurrentGame.AwayTeam == null)
        {
            DisplayAlert("Virhe", "Kotijoukkue tai vierasjoukkue puuttuu.", "OK");
            return;
        }
    }
}


