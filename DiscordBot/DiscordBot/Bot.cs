using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using System.IO;
using Newtonsoft.Json;
using DiscordBot.Commands;

namespace DiscordBot
{
    class Bot
    {
        private TimeSpan delayTimer = new TimeSpan(0, 0, 0, 0, -1);
        public DiscordClient client
        {
            get;
            private set;
        }
        public CommandsNextExtension commands
        {
            get;
            private set;
        }

        public async Task RunAsync()
        {
            var json = string.Empty;

            //reads tokens n shit
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            DiscordConfiguration config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            };

            client = new DiscordClient(config);

            client.Ready += OnClientReady;
            CommandsNextConfiguration commandsConfiguration = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.prefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                DmHelp = true,
                IgnoreExtraArguments = false,
                UseDefaultCommandHandler = true
            };

            commands = client.UseCommandsNext(commandsConfiguration);
            commands.RegisterCommands<BasicCommands>();

            await client.ConnectAsync();

            await Task.Delay(delayTimer);
        }

        private Task OnClientReady(DiscordClient k, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
