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
        private List<ServerScores> ServerScores = new();
        // private List<Score> Scores = new();

        public LeaderboardService()
        {
            if (!Directory.Exists("./data"))
                Directory.CreateDirectory("./data");

            if (!File.Exists("./data/scores.json"))
                SaveLeaderboard().Wait();
        }

        public async Task<List<Score>> GetLeaderboard(IGuild server)
        {
            await UpdateLeaderboard();
            var scores = ServerScores.FirstOrDefault(x => x.ServerId == server.Id)?.UserScores ?? new List<Score>();
            return scores.OrderByDescending(x => x.CurrentScore).ToList();
        }

        public async Task IncrementUser(IUser user, IGuild server)
        {
            var serverScores = ServerScores.FirstOrDefault(x => x.ServerId == server.Id);
            if (serverScores == null)
            {
                ServerScores.Add(new ServerScores
                {
                    ServerId = server.Id,
                    UserScores = new List<Score>()
                });

                serverScores = ServerScores.First(x => x.ServerId == server.Id);
            }
            var scores = serverScores.UserScores ?? new List<Score>();
            var indexToUpdate = ServerScores.FindIndex(x => x.ServerId == server.Id);
            if (scores.All(s => s.UserId != user.Id))
            {
                scores.Add(new Score
                {
                    UserId = user.Id,
                    Username = user.Username,
                    CurrentScore = 1
                });

                if (indexToUpdate != -1)
                    ServerScores[indexToUpdate].UserScores = scores;

                await SaveLeaderboard();

                return;
            }

            var scoreToIncrement = scores.First(x => x.UserId == user.Id);
            scoreToIncrement.CurrentScore += 1;
            
            if (indexToUpdate != -1)
                ServerScores[indexToUpdate].UserScores = scores;

            await SaveLeaderboard();
        }

        private async Task UpdateLeaderboard()
        {
            ServerScores = JsonConvert.DeserializeObject<List<ServerScores>>(await File.ReadAllTextAsync("./data/scores.json"));
        }

        private async Task SaveLeaderboard()
        {
            var stringToSave = JsonConvert.SerializeObject(ServerScores, Formatting.Indented);
            await File.WriteAllTextAsync("./data/scores.json", stringToSave);
        }
    }
}