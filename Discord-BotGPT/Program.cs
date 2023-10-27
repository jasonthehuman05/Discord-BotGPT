using Discord;
using Discord.Commands.Builders;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace Discord_BotGPT
{
    internal class Program
    {
        //Files containing tokens
        public static DiscordSocketClient client;
        static string token = File.ReadAllText(".discord");
        static ulong botID;
        static List<ulong> channelsToUse;
        static Dictionary<ulong, List<Message>> MessageDatabase;

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
            if (File.Exists(".channels"))
            {
                //Load the channels file
                string[] channels = File.ReadAllLines(".channels");
                //Parse it and store the data to be used
                foreach (string channel in channels)
                {
                    ulong id = ulong.Parse(channel.Trim());
                    channelsToUse.Add(id);
                }
            }
            //Create the discord client and wire up all needed events
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            });
            
            client.Log += DiscordLog;
            client.Ready += BotReady;

            //Create event to process command
            client.SlashCommandExecuted += CommandExecuted; ;

            //Attempt first log in
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            //Do nothing until program is closed
            await Task.Delay(-1);
            await client.StopAsync();
            client.Dispose();
        }

        private static async Task CommandExecuted(SocketSlashCommand arg)
        {
            SocketSlashCommandDataOption[] value = arg.Data.Options.ToArray();
            string name = (string)value[0];

            if(arg.CommandName == "createconversation")
            {
                //Create conversation
                ITextChannel channel = arg.Channel as ITextChannel;
                var newThread = await channel.CreateThreadAsync(
                    name: name,
                    autoArchiveDuration: ThreadArchiveDuration.OneDay,
                    type: ThreadType.PublicThread
                );
                newThread.SendMessageAsync("OMG OMG");
            }
            arg.RespondAsync("working on it...");
        }

        /// <summary>
        /// Runs when the bot has loaded
        /// </summary>
        /// <returns></returns>
        private static async Task BotReady()
        {
            Console.WriteLine("Bot Ready!");
            client.MessageReceived += MessageReceived;
            botID = client.CurrentUser.Id;
            //Create commands
            CreateCommands();
            LoadMessagesFromChannelsAsync();
        }

        /// <summary>
        /// Loads the messages from the channels used by the bot
        /// </summary>
        private static async Task LoadMessagesFromChannelsAsync()
        {
            foreach (ulong id in channelsToUse)
            {
                //Get all messages
                IMessageChannel channel = client.GetChannel(id) as IMessageChannel;
                ulong beforeMessageId = 0;
                List<IMessage> messages = new List<IMessage>();
                while (true)
                {
                    var messageBatch = await channel.GetMessagesAsync(100);
                    if (messageBatch.Count() == 0)
                    {
                        break; // No more messages
                    }

                    messages.AddRange(messageBatch);
                    beforeMessageId = messageBatch[messageBatch.Count() - 1].Id;
                }
                
            }
        }

        private static void CreateCommands()
        {
            SlashCommandBuilder createThreadCommandBuilder = new SlashCommandBuilder();
            createThreadCommandBuilder.WithName("createconversation");
            createThreadCommandBuilder.WithDescription("Creates a thread to chat with GPT-4");
            createThreadCommandBuilder.AddOption("title", ApplicationCommandOptionType.String, "Name of the thread", isRequired: true);

            SlashCommandProperties createThreadSCP = createThreadCommandBuilder.Build();


            client.CreateGlobalApplicationCommandAsync(createThreadSCP);
        }

        private static Task MessageReceived(SocketMessage arg)
        {
            Console.WriteLine($"{arg.Author}//{arg.Channel} ::: {arg.Content}");
            //Check to see if this is a message that should be processed
            if(arg.Author.Id == botID)
            {
                //its the bot, ignore it
            }
            else
            {
                //Is it in a channel we should be reading from?
                if (channelsToUse.Contains(arg.Channel.Id))
                {
                    //We should use this message
                    AddMessageToDatabase(arg.Content);

                }
            }
            return Task.CompletedTask;
        }

        private static void AddMessageToDatabase(string content)
        {
            throw new NotImplementedException();
        }

        private static Task DiscordLog(LogMessage arg) //Log any messages from the discord gateway
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
    }
}