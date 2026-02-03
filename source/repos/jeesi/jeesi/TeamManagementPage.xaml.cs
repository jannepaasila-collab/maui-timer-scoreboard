using System.Collections.ObjectModel;
using System.Diagnostics;

namespace jeesi;

// T‰m‰ luokka edustaa joukkueiden hallintasivua.
// Sivulla voi lis‰t‰, poistaa ja hallita joukkueita sek‰ niiden pelaajia.
public partial class TeamManagementPage : ContentPage
{
    // ObservableCollection, joka sis‰lt‰‰ kaikki joukkueet.
    // ObservableCollection p‰ivitt‰‰ automaattisesti k‰yttˆliittym‰n, kun siihen tehd‰‰n muutoksia.
    public ObservableCollection<Team> Teams { get; set; } = App.Teams;

    private Team? _selectedTeam;
    public Team? SelectedTeam
    {
        get => _selectedTeam;
        set
        {
            _selectedTeam = value;
            OnPropertyChanged(nameof(SelectedTeam));
            PlayersListView.ItemsSource = SelectedTeam?.Players ?? new ObservableCollection<Player>();
        }
    }

    private Player? _selectedPlayer;
    public Player? SelectedPlayer
    {
        get => _selectedPlayer;
        set
        {
            _selectedPlayer = value;
            OnPropertyChanged(nameof(SelectedPlayer));
        }
    }


    // Oletuskonstruktori. K‰ytet‰‰n, kun sivu avataan ilman parametreja.
    public TeamManagementPage()
    {
        InitializeComponent();
        BindingContext = this;
        Debug.WriteLine($"Joukkueita alussa: {App.Teams.Count}");
        LoadTeams();
        Debug.WriteLine($"Joukkueita latauksen j‰lkeen: {App.Teams.Count}");
    }

    // Ylikuormitettu konstruktori, joka ottaa joukkueet parametrina.
    public TeamManagementPage(ObservableCollection<Team> teams)
    {
        InitializeComponent();
        App.Teams = teams;
        Teams = App.Teams; 
        BindingContext = this; 
    }

    // Lis‰‰ uuden joukkueen nimell‰.
    private void AddTeam(string teamName)
    {
        if (!string.IsNullOrWhiteSpace(teamName))
        {
            if (!App.Teams.Any(t => t.TeamName == teamName))
            {
                App.Teams.Add(new Team { TeamName = teamName });
                Debug.WriteLine($"Lis‰tty joukkue: {teamName}");
                Debug.WriteLine($"Joukkueita nyt: {App.Teams.Count}");
                DataStorage.SaveTeams(App.Teams.ToList());
            }

            TeamNameEntry.Text = string.Empty;
        }
        else
        {
            DisplayAlert("Virhe", "Joukkueen nimi ei voi olla tyhj‰.", "OK");
        }
    }

    // K‰ytt‰j‰n painallus "Lis‰‰ joukkue" -painikkeeseen.
    private void OnAddTeamClicked(object sender, EventArgs e)
    {
        AddTeam(TeamNameEntry.Text);
    }

    // Joukkueen lis‰‰minen, kun k‰ytt‰j‰ painaa Enter-n‰pp‰int‰.
    private void OnTeamNameEntryCompleted(object sender, EventArgs e)
    {
        AddTeam(TeamNameEntry.Text);
    }

    // Lis‰‰ pelaajan valittuun joukkueeseen.
    private void AddPlayer()
    {
        if (SelectedTeam == null)
        {
            DisplayAlert("Virhe", "Valitse joukkue, johon pelaaja lis‰t‰‰n.", "OK");
            return;
        }

        if (!string.IsNullOrWhiteSpace(PlayerFirstNameEntry.Text) &&
            !string.IsNullOrWhiteSpace(PlayerLastNameEntry.Text) &&
            int.TryParse(PlayerNumberEntry.Text, out int playerNumber))
        {
            if (SelectedTeam.Players.Any(p => p.PlayerNumber == playerNumber))
            {
                DisplayAlert("Virhe", "Pelaajanumero on jo k‰ytˆss‰ t‰ss‰ joukkueessa.", "OK");
                return;
            }

            SelectedTeam.Players.Add(new Player
            {
                FirstName = PlayerFirstNameEntry.Text,
                LastName = PlayerLastNameEntry.Text,
                PlayerNumber = playerNumber,
                TeamName = SelectedTeam.TeamName
            });

            DataStorage.SaveTeams(App.Teams.ToList());

            PlayerFirstNameEntry.Text = string.Empty;
            PlayerLastNameEntry.Text = string.Empty;
            PlayerNumberEntry.Text = string.Empty;

            PlayersListView.ItemsSource = null; 
            PlayersListView.ItemsSource = SelectedTeam.Players; 

            Debug.WriteLine($"Pelaaja lis‰tty: {PlayerFirstNameEntry.Text} {PlayerLastNameEntry.Text}, Numero: {playerNumber}, Joukkue: {SelectedTeam.TeamName}");
        }
        else
        {
            DisplayAlert("Virhe", "Pelaajan tiedot eiv‰t ole oikein. Tarkista ja yrit‰ uudelleen.", "OK");
        }
    }

    // Lataa joukkueet tiedostosta ja lis‰‰ ne sovelluksen Teams-listaan.
    private void LoadTeams()
    {
        try
        {
            Debug.WriteLine($"Joukkueita ennen latausta: {App.Teams.Count}");

            if (App.Teams.Count == 0)
            {
                var loadedTeams = DataStorage.LoadTeams();
                Debug.WriteLine($"Ladattuja joukkueita tiedostosta: {loadedTeams.Count}");
                foreach (var team in loadedTeams)
                {
                    if (!App.Teams.Any(t => t.TeamName == team.TeamName))
                    {
                        App.Teams.Add(team);
                    }
                }
            }

            Debug.WriteLine($"Joukkueita latauksen j‰lkeen: {App.Teams.Count}");
        }
        catch (Exception ex)
        {
            DisplayAlert("Virhe", $"Joukkueiden lataaminen ep‰onnistui: {ex.Message}", "OK");
        }
    }

    // K‰ytt‰j‰n painallus "Lis‰‰ pelaaja" -painikkeeseen.
    private void OnAddPlayerClicked(object sender, EventArgs e)
    {
        AddPlayer();
    }

    // Pelaajan lis‰‰minen, kun k‰ytt‰j‰ painaa Enter-n‰pp‰int‰.
    private void OnPlayerEntryCompleted(object sender, EventArgs e)
    {
        AddPlayer();
    }

    // Sulkee modaalisen sivun.
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Virhe sivun sulkemisessa: {ex.Message}");
        }
    }

    // Poistaa valitun joukkueen.
    private async void OnDeleteTeamClicked(object sender, EventArgs e)
    {
        if (SelectedTeam == null)
        {
            await DisplayAlert("Virhe", "Valitse joukkue ennen poistamista.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Vahvistus", $"Haluatko varmasti poistaa joukkueen: {SelectedTeam.TeamName}?", "Kyll‰", "Ei");

        if (confirm)
        {
            App.Teams.Remove(SelectedTeam);
            DataStorage.SaveTeams(App.Teams.ToList()); 
            SelectedTeam = null;
            PlayersListView.ItemsSource = null; 
            await DisplayAlert("Poistettu", "Joukkue poistettiin onnistuneesti.", "OK");
        }
    }

    // Poistaa valitun pelaajan.
    private async void OnDeletePlayerClicked(object sender, EventArgs e)
    {
        if (SelectedPlayer == null)
        {
            await DisplayAlert("Virhe", "Valitse pelaaja ennen poistamista.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Vahvistus", $"Haluatko varmasti poistaa pelaajan: {SelectedPlayer.FirstName} {SelectedPlayer.LastName}?", "Kyll‰", "Ei");

        if (confirm && SelectedTeam != null)
        {
            SelectedTeam.Players.Remove(SelectedPlayer);
            DataStorage.SaveTeams(App.Teams.ToList());
            SelectedPlayer = null;
            PlayersListView.ItemsSource = SelectedTeam.Players;
            await DisplayAlert("Poistettu", "Pelaaja poistettiin onnistuneesti.", "OK");
        }
    }

}
