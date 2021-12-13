using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeathrollBot.Interfaces;
using DeathrollBot.Models;
using Discord;
using Discord.Commands;

namespace DeathrollBot.Commands
{
    public class Roll : ModuleBase<SocketCommandContext>
    {
        private readonly ILeaderboardService _leaderboardService;

        public static List<Challenge> CurrentPendingChallenges = new();
        public static List<Challenge> CurrentChallenges = new();

        public Roll(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [Command("roll"), Alias("random", "dice")]
        public async Task RandomRoll(string maxRoll = "")
        {
            var currentChallenge = CurrentChallenges.FirstOrDefault(x => x.ChallengingUserId == Context.User || x.ChallengedUserId == Context.User);
            if (currentChallenge != null)
            {
                await RandomRollForChallenge(currentChallenge);
                return;
            }

            var maxRollAsInt = 999;
            if (!string.IsNullOrEmpty(maxRoll))
            {
                var parsed = int.TryParse(maxRoll, out maxRollAsInt);
                if (!parsed)
                {
                    await ReplyAsync("The entered max roll could not be parsed (make sure it is a whole number).");
                    return;
                }

                if (maxRollAsInt < 1)
                {
                    await ReplyAsync("The entered max roll should be more than 1.");
                    return;
                }
            }

            var random = new Random();

            var rollResult = random.Next(1, maxRollAsInt);

            await ReplyAsync($"You rolled: {rollResult} (max: {maxRollAsInt}){(rollResult == 1 ? " You Lost! :(" : "")}");
        }

        private async Task RandomRollForChallenge(Challenge currentChallenge)
        {
            if (currentChallenge.CurrentUsersTurnToRoll != Context.User)
            {
                await ReplyAsync("It is not your turn to roll!");
                return;
            }

            var random = new Random();

            var rollResult = random.Next(1, currentChallenge.CurrentMaxRoll);

            var stringToReply = $"You rolled: {rollResult} (max: {currentChallenge.CurrentMaxRoll})";
            if (rollResult == 1)
            {
                IUser winner = null;

                if (currentChallenge.ChallengingUserId == Context.User)
                    winner = currentChallenge.ChallengedUserId;
                else if (currentChallenge.ChallengedUserId == Context.User)
                    winner = currentChallenge.ChallengingUserId;

                stringToReply += $"\n{winner?.Mention} is the winner of '{currentChallenge.Bet}'!";

                CurrentChallenges.Remove(currentChallenge);

                await _leaderboardService.IncrementUser(winner);
            }

            currentChallenge.CurrentMaxRoll = rollResult;
            currentChallenge.CurrentUsersTurnToRoll = currentChallenge.ChallengedUserId == Context.User
                ? currentChallenge.ChallengingUserId
                : currentChallenge.ChallengedUserId;

            await ReplyAsync(stringToReply);
        }

        [Command("challenge")]
        public async Task ChallengeUser(IUser user, [Remainder] string betString)
        {
            if (CurrentChallenges.Any(challenge => challenge.ChallengingUserId == Context.User ||
                                                   challenge.ChallengedUserId == Context.User))
            {
                await ReplyAsync("You are already challenging a user.");
                return;
            }

            CurrentPendingChallenges.Add(new Challenge
            {
                ChallengeId = Guid.NewGuid(),
                ChallengingUserId = Context.Guild.GetUser(Context.User.Id),
                ChallengedUserId = Context.Guild.GetUser(user.Id),
                Bet = betString,
                ChallengeStarted = DateTime.UtcNow
            });

            var prefix = Environment.GetEnvironmentVariable("prefix");
            await ReplyAsync($"{user.Mention}, you have been issued a challenge for '{betString}'\nUse '{prefix}accept' or '{prefix}deny' to respond.");
        }

        [Command("accept"), Alias("confirm", "yes")]
        public async Task AcceptChallenge()
        {
            var challengeToRespondTo = CurrentPendingChallenges.Where(challenge => challenge.ChallengedUserId == Context.User)
                                                               .OrderByDescending(x => x.ChallengeStarted)
                                                               .FirstOrDefault();

            if (challengeToRespondTo == null)
            {
                await ReplyAsync("Could not find a challenge to respond to.");
                return;
            }

            var ChallengingUser = challengeToRespondTo.ChallengingUserId;

            CurrentPendingChallenges.Remove(challengeToRespondTo);

            challengeToRespondTo.CurrentUsersTurnToRoll = ChallengingUser;

            CurrentChallenges.Add(challengeToRespondTo);

            await ReplyAsync($"{ChallengingUser.Mention}, your challenge has been accepted! Roll the dice!");
        }

        [Command("deny"), Alias("reject", "no")]
        public async Task DenyChallenge()
        {
            var challengeToRespondTo = CurrentPendingChallenges.Where(challenge => challenge.ChallengedUserId == Context.User)
                .OrderByDescending(x => x.ChallengeStarted)
                .FirstOrDefault();

            if (challengeToRespondTo == null)
            {
                await ReplyAsync("Could not find a challenge to respond to.");
                return;
            }

            var ChallengingUser = challengeToRespondTo.ChallengingUserId;

            CurrentPendingChallenges.Remove(challengeToRespondTo);

            await ReplyAsync($"{ChallengingUser.Mention}, your challenge has been rejected!");
        }
    }
}
