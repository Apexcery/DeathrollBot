using System;
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
                    await ReplyAsync("The entered max roll could not be parsed (make sure it is a whole number).");
                    return;
                }
            }

            var random = new Random();

            var rollResult = random.Next(1, maxRollAsInt);

            if (rollResult == 1)
            {
                await ReplyAsync("You lost! :(");
                return;
            }

            await ReplyAsync($"You rolled: {rollResult} (max: {maxRollAsInt})");
        }
    }
}
