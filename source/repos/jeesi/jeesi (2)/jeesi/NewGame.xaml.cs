using System.Collections.ObjectModel;

namespace jeesi;

public partial class NewGame : ContentPage
{
    // NewGame-luokka edustaa n‰kym‰‰, jossa k‰ytt‰j‰ voi valita uuden pelin lajin ja asetukset.
    public NewGame()
	{
		InitializeComponent();
        BindingContext = App.Teams;
    }

    // Yhteinen metodi pelin avaamiseen modaalisena sivuna
    private async Task OpenGameAsync(string sport, int periods, int timePerPeriod, bool timeIncreases)
    {
        await Navigation.PushModalAsync(new GamePage(sport, periods, timePerPeriod, timeIncreases, App.Teams));
    }
    
    // Jalkapallon valintapainike
    private async void OnFootballClicked(object sender, EventArgs e)
    {
        await OpenGameAsync("Jalkapallo", 2, 45, true);
    }

    // J‰‰kiekon valintapainike
    private async void OnHockeyClicked(object sender, EventArgs e)
    {
        await OpenGameAsync("J‰‰kiekko", 3, 20, false);
    }

    // Salibandyn valintapainike
    private async void OnFloorballClicked(object sender, EventArgs e)
    {
        await OpenGameAsync("Salibandy", 3, 15, false);
    }

    // Modaalisen sivun sulkemispainike
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }


}