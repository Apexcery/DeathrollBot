using System;
using System.Linq;
using System.Threading.Tasks;
using DeathrollBot.Interfaces;
using Discord;
using Discord.Commands;

namespace DeathrollBot.Commands
{
    public class Leaderboard : ModuleBase<SocketCommandContext>
    {
        private readonly ILeaderboardService _leaderboardService;

        public Leaderboard(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [Command("leaderboard"), Alias("lb", "top")]
        public async Task ShowLeaderboard()
        {
            var currentScores = await _leaderboardService.GetLeaderboard();

            if (!currentScores.Any())
            {
                await ReplyAsync("The leaderboard is currently empty, challenge someone with '.challenge <@user> <bet>'");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Leaderboard")
                .WithColor(Color.Orange);

            for (var i = 0; i < Math.Min(5, currentScores.Count); i++)
            {
                var user = Context.Guild.GetUser(currentScores[i].UserId);
                embed.AddField($"{user.Username}", $"Score: {currentScores[i].CurrentScore}");
            }

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
        }
    }
}