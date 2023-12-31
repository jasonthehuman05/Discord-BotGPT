﻿using Discord;
using Discord.WebSocket;
using Discord_BotGPT.GptInteraction;

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
        static GptHandler gpt;

        static void Main(string[] args)
        {
            gpt = new GptHandler();
            MessageDatabase = new Dictionary<ulong, List<Message>>();
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
            channelsToUse = new List<ulong>();

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
            else { File.Create(".channels"); }

            //Create the discord client and wire up all needed events
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            });
            
            client.Log += DiscordLog;
            client.Ready += BotReady;

            //Create event to process command
            client.SlashCommandExecuted += CommandExecuted;

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
            string inputParameter = (string)value[0];

            if(arg.CommandName == "createconversation")
            {
                arg.RespondAsync("working on it...");
                //Create conversation
                ITextChannel channel = arg.Channel as ITextChannel;
                var newThread = await channel.CreateThreadAsync(
                    name: inputParameter,
                    autoArchiveDuration: ThreadArchiveDuration.OneDay,
                    type: ThreadType.PublicThread
                );
                //newThread.SendMessageAsync("OMG OMG");
                //Add channel id to the file
                channelsToUse.Add(newThread.Id);
                //reload the channel file
                RebuildChannelFile();
            }
            if (arg.CommandName == "changemodel")
            {
                if(arg.User.Username == "jason.the.human")
                {
                    //Only run for me
                    gpt.model = inputParameter;
                    arg.RespondAsync($"SET MODEL TO {inputParameter}.");
                }
                else
                {
                    arg.RespondAsync("You can not do this");
                }
            }
        }

        private static void RebuildChannelFile()
        {
            //Get data as string
            string data = "";
            foreach(ulong id in channelsToUse)
            {
                data += id.ToString() + "\n";
            }
            data.Trim();
            File.WriteAllText(".channels", data);
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
                IMessageChannel sc = client.GetChannel(id) as IMessageChannel;

                List<IMessage> retrievedMessages = new List<IMessage>();

                if(sc != null)
                {
                    IAsyncEnumerable<IReadOnlyCollection<IMessage>> msgs = sc.GetMessagesAsync();
                    IEnumerable<IMessage> messageCollection = await msgs.FlattenAsync();

                    foreach (IMessage msg in messageCollection)
                    {
                        //Console.WriteLine($"{msg.Author.Username} ::: {msg.Content}");
                        retrievedMessages.Add(msg);
                    }
                    while (true)
                    {
                        //Console.ReadLine();
                        if (retrievedMessages.Count % 100 == 0)
                        {
                            List<IMessage> newMessages = await RetrieveBatch(sc, retrievedMessages.Last());
                            foreach (IMessage msg in newMessages)
                            {
                                retrievedMessages.Add(msg);
                            }
                        }
                        else { break; }
                    }
                    List<Message> messages = new List<Message>();

                    foreach (IMessage msg in retrievedMessages)
                    {
                        //Convert each message
                        Message message = new Message();
                        message.Content = msg.Content;
                        message.Role = msg.Author.Id == botID ? "assistant" : "user";
                        messages.Add(message);
                    }
                    messages.Reverse();
                    MessageDatabase.Add(id, messages);
                    foreach (Message message in messages)
                    {
                        Console.WriteLine(message.Content);
                    }
                }
            }
        }

        public static async Task<List<IMessage>> RetrieveBatch(IMessageChannel channel, IMessage lastMessage)
        {
            List<IMessage> currentBatch = new List<IMessage>();
            IAsyncEnumerable<IReadOnlyCollection<IMessage>> msgs = channel.GetMessagesAsync(fromMessage: lastMessage, dir: Direction.Before);
            IEnumerable<IMessage> messageCollection = await msgs.FlattenAsync();

            foreach (IMessage msg in messageCollection)
            {
                //Console.WriteLine($"{msg.Author.Username} ::: {msg.Content}");
                currentBatch.Add(msg);
            }

            return currentBatch;
        }

        private static void CreateCommands()
        {
            SlashCommandBuilder createThreadCommandBuilder = new SlashCommandBuilder();
            createThreadCommandBuilder.WithName("createconversation");
            createThreadCommandBuilder.WithDescription("Creates a thread to chat with GPT-4");
            createThreadCommandBuilder.AddOption("title", ApplicationCommandOptionType.String, "Name of the thread", isRequired: true);

            SlashCommandProperties createThreadSCP = createThreadCommandBuilder.Build();


            client.CreateGlobalApplicationCommandAsync(createThreadSCP);


            SlashCommandBuilder changeModelCommandBuilder = new SlashCommandBuilder();
            changeModelCommandBuilder.WithName("changemodel");
            changeModelCommandBuilder.WithDescription("Change the model to use with the bot");
            changeModelCommandBuilder.AddOption("modelname", ApplicationCommandOptionType.String, "Model Name", isRequired: true);

            SlashCommandProperties changeModelSCP = changeModelCommandBuilder.Build();


            client.CreateGlobalApplicationCommandAsync(changeModelSCP);
        }

        private static async Task MessageReceived(SocketMessage arg)
        {
            Console.WriteLine($"{arg.Author}//{arg.Channel} ::: {arg.Content}");
            //Check to see if this is a message that should be processed
            if (channelsToUse.Contains(arg.Channel.Id))
            {
                //its in a channel we should use
                
                //Do we need to process it
                if (arg.Author.Id == botID)
                {
                    //We should use this message, but its from the bot, so no processing needs doing
                    AddMessageToDatabase(arg.Content, arg.Channel.Id);

                }
                else
                {
                    //it is a user message. Process it
                    AddMessageToDatabase(arg.Content, arg.Channel.Id);
                    string reply = await SendMessagesToBotAsync(arg.Content, arg.Channel.Id);
                    arg.Channel.SendMessageAsync(reply);
                }
            }
        }

        private static async Task<string> SendMessagesToBotAsync(string content, ulong ChannelID)
        {
            GPTResponse responseMessage = await gpt.SendMessage(MessageDatabase[ChannelID].ToArray());
            string reply = responseMessage.choices[0].message.content;
            return reply;
        }

        private static void AddMessageToDatabase(string content, ulong id)
        {
            if (!MessageDatabase.Keys.Contains(id))
            {
                MessageDatabase.Add(id, new List<Message>());
            }
            MessageDatabase[id].Add(new Message()
            {
                Role="user",
                Content=content
            });
        }

        private static Task DiscordLog(LogMessage arg) //Log any messages from the discord gateway
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
    }
}