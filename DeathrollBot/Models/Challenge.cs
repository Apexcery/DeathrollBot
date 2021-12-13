using System;
using Discord;

namespace DeathrollBot.Models
{
    public class Challenge
    {
        public Guid ChallengeId;
        public IUser ChallengingUserId;
        public IUser ChallengedUserId;
        public string Bet;
        public DateTime ChallengeStarted;

        public IUser CurrentUsersTurnToRoll; // The user who's turn it currently is.
        public int CurrentMaxRoll = 999;
    }
}
