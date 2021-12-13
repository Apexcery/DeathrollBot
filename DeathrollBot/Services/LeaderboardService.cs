using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeathrollBot.Interfaces;
using DeathrollBot.Models;
using Discord;
using Newtonsoft.Json;

namespace DeathrollBot.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private List<Score> Scores = new();

        public LeaderboardService()
        {
            if (!Directory.Exists("./data"))
                Directory.CreateDirectory("./data");

            if (!File.Exists("./data/scores.json"))
                SaveLeaderboard().Wait();
        }

        public async Task<List<Score>> GetLeaderboard()
        {
            await UpdateLeaderboard();
            return Scores.OrderByDescending(x => x.CurrentScore).ToList();
        }

        public async Task IncrementUser(IUser user)
        {
            if (Scores.All(s => s.UserId != user.Id))
            {
                Scores.Add(new Score
                {
                    UserId = user.Id,
                    CurrentScore = 1
                });

                await SaveLeaderboard();

                return;
            }

            var scoreToIncrement = Scores.First(x => x.UserId == user.Id);
            scoreToIncrement.CurrentScore += 1;

            await SaveLeaderboard();
        }

        private async Task UpdateLeaderboard()
        {
            Scores = JsonConvert.DeserializeObject<List<Score>>(await File.ReadAllTextAsync("./data/scores.json"));
        }

        private async Task SaveLeaderboard()
        {
            var stringToSave = JsonConvert.SerializeObject(Scores, Formatting.Indented);
            await File.WriteAllTextAsync("./data/scores.json", stringToSave);
        }
    }
}