using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;

    public CommandHandler(DiscordSocketClient client)
    {
        _client = client;
        _commands = new InteractionService(_client);

        _client.Ready += async () => await RegisterCommandsAsync();
    }

    public async Task RegisterCommandsAsync()
    {
        try
        {
            await Task.Delay(5000); // Ensure the bot is fully ready before registering commands
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            await _commands.RegisterCommandsGloballyAsync();
            Console.WriteLine("Slash commands registered successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering commands: {ex.Message}");
        }
    }
}