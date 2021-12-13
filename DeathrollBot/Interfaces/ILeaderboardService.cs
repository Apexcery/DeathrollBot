using System.Collections.Generic;
using System.Threading.Tasks;
using DeathrollBot.Models;
using Discord;

namespace DeathrollBot.Interfaces
{
    public interface ILeaderboardService
    {
        public Task<List<Score>> GetLeaderboard();
        public Task IncrementUser(IUser user);
    }
}