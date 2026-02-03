using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;

namespace jeesi
{
    // DataStorage-luokka tarjoaa staattisia metodeja sovelluksen datan tallennukseen ja lataukseen.
    public static class DataStorage
    {
        // Tiedostopolut eri data-objekteille (joukkueet, pelit ja pelaajat)
        private static readonly string TeamsFilePath = Path.Combine(FileSystem.AppDataDirectory, "teams.json");
        private static readonly string GamesFilePath = Path.Combine(FileSystem.AppDataDirectory, "games.json");
        private static readonly string PlayersFilePath = Path.Combine(FileSystem.AppDataDirectory, "players.json");

        // Staattinen konstruktori, joka tulostaa tiedostopolut konsoliin tai debug-ikkunaan.
        static DataStorage()
        {
            Debug.WriteLine($"TeamsFilePath: {TeamsFilePath}");
            Debug.WriteLine($"GamesFilePath: {GamesFilePath}");
            Debug.WriteLine($"PlayersFilePath: {PlayersFilePath}");
        }

        // Tallentaa joukkueet JSON-tiedostoon.
        public static void SaveTeams(List<Team> teams)
        {
            var json = JsonSerializer.Serialize(teams, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(TeamsFilePath, json);
        }

        // Lataa joukkueet tiedostosta.
        public static List<Team> LoadTeams()
        {
            if (!File.Exists(TeamsFilePath))
            {
                return new List<Team>();
            }

            try
            {
                var json = File.ReadAllText(TeamsFilePath);
                return JsonSerializer.Deserialize<List<Team>>(json) ?? new List<Team>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Virhe ladattaessa joukkueita: {ex.Message}");
                return new List<Team>();
            }
        }

        // Tallentaa pelit JSON-tiedostoon.
        public static void SaveGames(List<Game> games)
        {
            var json = JsonSerializer.Serialize(games, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GamesFilePath, json);
        }

        // Lataa pelit tiedostosta.
        public static List<Game> LoadGames()
        {
            if (!File.Exists(GamesFilePath))
            {
                return new List<Game>();
            }

            try
            {
                var json = File.ReadAllText(GamesFilePath);
                return JsonSerializer.Deserialize<List<Game>>(json) ?? new List<Game>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Virhe ladattaessa pelejä: {ex.Message}");
                return new List<Game>();
            }
        }

        // Tallentaa pelaajat JSON-tiedostoon.
        public static void SavePlayers(List<Player> players)
        {
            var json = JsonSerializer.Serialize(players, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PlayersFilePath, json);
        }

        // Lataa pelaajat tiedostosta.
        public static List<Player> LoadPlayers()
        {
            if (!File.Exists(PlayersFilePath))
            {
                return new List<Player>();
            }

            try
            {
                var json = File.ReadAllText(PlayersFilePath);
                return JsonSerializer.Deserialize<List<Player>>(json) ?? new List<Player>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Virhe ladattaessa pelaajia: {ex.Message}");
                return new List<Player>();
            }
        }

        // Poistaa joukkueiden tallennustiedoston.
        public static void ClearTeams()
        {
            if (File.Exists(TeamsFilePath))
            {
                File.Delete(TeamsFilePath);
            }
        }

        // Poistaa pelien tallennustiedoston.
        public static void ClearGames()
        {
            if (File.Exists(GamesFilePath))
            {
                File.Delete(GamesFilePath);
            }
        }

        // Poistaa pelaajien tallennustiedoston.
        public static void ClearPlayers()
        {
            if (File.Exists(PlayersFilePath))
            {
                File.Delete(PlayersFilePath);
            }
        }
    }
}
