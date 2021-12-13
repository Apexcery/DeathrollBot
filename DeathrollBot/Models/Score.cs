using System.Collections.Generic;

namespace DeathrollBot.Models
{
    public class ServerScores
    {
        public ulong ServerId;
        public List<Score> UserScores;
    }

    public class Score
    {
        public ulong UserId;
        public string Username;
        public int CurrentScore;
    }
}