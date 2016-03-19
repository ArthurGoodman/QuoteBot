using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace TwitchBot {
    public class Bot {
        public static Bot Instance { get; private set; }

        private IrcClient ircClient;
        private Settings settings;
        private Account account;

        private Random random = new Random();

        private Dictionary<string, Command> commands = new Dictionary<string, Command>();

        private Stopwatch stopwatch = new Stopwatch();

        public string Channel { get { return ircClient.Channel; } }
        public List<string> Mods { get; private set; }

        public Bot() {
            Instance = this;

            stopwatch.Start();

            Mods = new List<string>();

            Initialize();
        }

        private void Initialize() {
            settings = Settings.Load("settings.xml");

            account = Account.Load(settings.AccountFile);

            WebClient client = new WebClient();
            string cluster = client.DownloadString("https://decapi.me/twitch/clusters?channel=" + settings.Channel);
            Console.WriteLine("cluster: " + cluster);

            string server = cluster == "main" ? "irc.twitch.tv" : "irc.chat.twitch.tv";
            ircClient = new IrcClient(server, 6667, account.Username, account.Password);

            SetupCommands();
        }

        public void Run() {
            ircClient.JoinRoom(settings.Channel);

            ircClient.SendChatMessage(".color " + settings.Color);

            if (settings.Greeting != "")
                ircClient.SendChatMessage(settings.Greeting);

            while (true) {
                IrcMessage ircMessage = ircClient.ReadIrcMessage();

                if (ircMessage.Command == "MODE") {
                    if (ircMessage.Args[1] == "+o")
                        Mods.Add(ircMessage.Args[2]);
                    else if (ircMessage.Args[1] == "-o")
                        Mods.Add(ircMessage.Args[2]);
                } else if (ircMessage.Command == "PING")
                    ircClient.SendIrcMessage("PONG :" + ircMessage.Trailing);

                if (ircMessage.Command != "PRIVMSG") {
                    Console.WriteLine(ircMessage.ToString());
                    continue;
                }

                ChatMessage chatMessage = new ChatMessage(ircMessage);

                Console.WriteLine(chatMessage.ToString());

                string message = chatMessage.Message;
                string username = chatMessage.Username;

                if (stopwatch.Elapsed.TotalMilliseconds < settings.Interval)
                    continue;

                foreach (KeyValuePair<string, Command> command in commands)
                    if (message == command.Key || message.StartsWith(command.Key + " ")) {
                        command.Value.Execute(username, message);
                        break;
                    }
            }
        }

        public void Say(string message) {
            stopwatch.Restart();
            ircClient.SendChatMessage(message);
        }

        private string ReadRandomLine(string fileName) {
            try {
                string[] lines = File.ReadAllLines(fileName).Where(line => line != "").ToArray();

                return lines.Length == 0 ? "" : lines[random.Next() % lines.Length];
            } catch (Exception e) {
                System.Console.WriteLine(e.Message);
                return "";
            }
        }

        private void SetupCommands() {
            commands.Add("!quote", new BuiltinCommand(0, Command.AccessLevel.Regular, (string username, string[] args) => {
                Say(ReadRandomLine(settings.QuotesFile));
                return 0;
            }));

            commands.Add("!addquote", new BuiltinCommand(1, Command.AccessLevel.Mod, (string username, string[] args) => {
                if (args.Length == 1) {
                    Say(username + " -> A quote cannot be empty.");
                    return 1;
                }

                File.AppendAllText(settings.QuotesFile, Environment.NewLine + args[1]);

                Say(username + " -> Quote added.");
                return 0;
            }));
        }
    }
}
