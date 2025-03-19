using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.Json;
using System.Reflection;

namespace ProjectCircleBot
{
    class Program
    {
        private static DiscordSocketClient _client;
        private static InteractionService _commands;
        private static CommandHandler _commandHandler;
        private static string _logFilePath = "Logs/chat.txt"; // Auto-detected
        private static string _playerLogPath = "Logs/user.txt"; // Player join/leave log
        private static ulong _discordChannelId;
        
        static async Task Main(string[] args)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            });

            _client.Log += Log;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            
            var token = File.ReadAllText("config.txt").Trim();
            var jsonSettings = File.ReadAllText("settings.json");
            var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonSettings);

            if (!settings.ContainsKey("DISCORD_CHANNEL_ID"))
            {
                Console.WriteLine("ERROR: Missing DISCORD_CHANNEL_ID in settings.json");
                return;
            }
            _discordChannelId = ulong.Parse(settings["DISCORD_CHANNEL_ID"]);

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _commandHandler = new CommandHandler(_client);
            await _commandHandler.RegisterCommandsAsync(); // Ensure commands are registered

            Console.WriteLine("Bot is running...");
            await Task.WhenAll(MonitorLogFile(), MonitorPlayerLog()); // Start monitoring logs
            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private static async Task ReadyAsync()
        {
            Console.WriteLine("Bot is ready!");
        }

        private static async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            if (message.Content.StartsWith("/gamechat"))
            {
                string msgToGame = message.Content.Replace("/gamechat", "").Trim();
                await SendToGameChat(msgToGame);
                await message.Channel.SendMessageAsync($"Sent to game: {msgToGame}");
            }
        }

        private static async Task MonitorLogFile()
        {
            if (!File.Exists(_logFilePath))
            {
                Console.WriteLine($"Log file {_logFilePath} not found, creating it...");
                Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
                File.Create(_logFilePath).Close();
            }

            Console.WriteLine($"Watching chat log file: {_logFilePath}");
            using var reader = new StreamReader(new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            while (true)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(line))
                {
                    await ProcessLogLine(line);
                }
                await Task.Delay(100);
            }
        }

        private static async Task MonitorPlayerLog()
        {
            if (!File.Exists(_playerLogPath))
            {
                Console.WriteLine($"Player log file {_playerLogPath} not found, creating it...");
                Directory.CreateDirectory(Path.GetDirectoryName(_playerLogPath));
                File.Create(_playerLogPath).Close();
            }

            Console.WriteLine($"Watching player log file: {_playerLogPath}");
            using var reader = new StreamReader(new FileStream(_playerLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            while (true)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(line))
                {
                    await ProcessPlayerLog(line);
                }
                await Task.Delay(100);
            }
        }

        private static async Task ProcessLogLine(string line)
        {
            var match = Regex.Match(line, @"\[(\d{2}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3})\] (.+)");
            if (match.Success)
            {
                string timestamp = match.Groups[1].Value;
                string message = match.Groups[2].Value;
                Console.WriteLine($"Game Chat: {message}");
                var channel = _client.GetChannel(_discordChannelId) as IMessageChannel;
                if (channel != null)
                    await channel.SendMessageAsync($"**Game Chat**: {message}");
            }
        }

        private static async Task ProcessPlayerLog(string line)
        {
            if (line.Contains("fully connected"))
            {
                var name = ExtractName(line);
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var channel = _client.GetChannel(_discordChannelId) as IMessageChannel;
                if (channel != null)
                    await channel.SendMessageAsync($":white_check_mark: **{name}** joined the server at {timestamp}.");
            }
            else if (line.Contains("disconnected"))
            {
                var name = ExtractName(line);
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var channel = _client.GetChannel(_discordChannelId) as IMessageChannel;
                if (channel != null)
                    await channel.SendMessageAsync($":x: **{name}** left the server at {timestamp}.");
            }
        }

        private static string ExtractName(string logLine)
        {
            var firstQuote = logLine.IndexOf("\"");
            var lastQuote = logLine.LastIndexOf("\"");
            return logLine.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
        }

        private static async Task SendToGameChat(string message)
        {
            try
            {
                await File.AppendAllTextAsync("server-console.txt", $"say \"{message}\"\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to game: {ex.Message}");
            }
        }
    }
}