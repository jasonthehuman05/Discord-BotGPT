using Discord;
using Discord.Commands.Builders;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using System.Net.Http.Headers;

namespace Discord_BotGPT
{
    internal class Program
    {
        //Files containing tokens
        public static DiscordSocketClient client;
        static string token = File.ReadAllText(".discord");

        static void Main(string[] args)
        {
            Console.WriteLine("GPT Bot");
            Console.WriteLine("Starting Bot...");
            MainAsyncProcess();
            while (true);
        }


        /// <summary>
        /// Main process that runs then bot
        /// </summary>
        static async void MainAsyncProcess()
        {
            //Create the discord client and wire up all needed events
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            });
            
            client.Log += DiscordLog;
            client.Ready += BotReady;

            //Create event to process command
            client.SlashCommandExecuted += commandHandler.CommandExecuted;

            //Attempt first log in
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            //Do nothing until program is closed
            await Task.Delay(-1);
            await client.StopAsync();
            client.Dispose();
        }

        /// <summary>
        /// Runs when the bot has loaded
        /// </summary>
        /// <returns></returns>
        private static async Task BotReady()
        {
            Console.WriteLine("Bot Ready!");
            client.MessageReceived += MessageReceived;
            //Create commands
            CreateCommands();
        }

        private static void CreateCommands()
        {
            SlashCommandBuilder createThreadCommandBuilder = new SlashCommandBuilder();
            createThreadCommandBuilder.WithName("createconversation");
            createThreadCommandBuilder.WithDescription("Creates a thread to chat with GPT-4");
            createThreadCommandBuilder.AddOption("title", ApplicationCommandOptionType.String, "Name of the thread", isRequired: true);

            SlashCommandProperties createThreadSCP = createThreadCommandBuilder.Build();
        }

        private static Task MessageReceived(SocketMessage arg)
        {
            Console.WriteLine($"{arg.Author}//{arg.Channel} ::: {arg.Content}");
            return Task.CompletedTask;
        }

        private static Task DiscordLog(LogMessage arg) //Log any messages from the discord gateway
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
    }
}