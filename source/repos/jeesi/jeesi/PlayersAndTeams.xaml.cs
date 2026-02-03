using System.Collections.ObjectModel;

namespace jeesi;

public partial class PlayersAndTeams : ContentPage
{
    public PlayersAndTeams()
    {
        InitializeComponent();
        BindingContext = App.Teams;
    }
    // Avaa modaalisena sivuna käyttäen App.Teams
    private async void OnOpenTeamManagementClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new TeamManagementPage(App.Teams));
    }

    // Navigointi joukkueiden hallintasivulle käyttäen App.Teams
    private async void OnTeamManagementClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TeamManagementPage(App.Teams));
    }

}
