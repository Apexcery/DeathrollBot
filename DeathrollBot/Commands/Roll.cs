﻿using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace DeathrollBot.Commands
{
    public class Roll : ModuleBase<SocketCommandContext>
    {
        [Command("roll"), Alias("random")]
        public async Task RandomRoll(string maxRoll = "")
        {
            var maxRollAsInt = 999;
            if (!string.IsNullOrEmpty(maxRoll))
            {
                var parsed = int.TryParse(maxRoll, out maxRollAsInt);
                if (!parsed)
                {
                    await ReplyAsync("The entered max roll could not be parsed (make sure it is a number).");
                    return;
                }
            }

            var random = new Random();

            var rollResult = random.Next(maxRollAsInt);

            await ReplyAsync($"You rolled: {rollResult} (max: {maxRollAsInt})");
        }
    }
}
