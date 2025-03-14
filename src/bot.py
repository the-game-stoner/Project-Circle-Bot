import discord
from discord.ext import commands, tasks
import aiohttp
import json
import os
from dotenv import load_dotenv

# Load bot token from .env
load_dotenv()
BOT_TOKEN = os.getenv("BOT_TOKEN")

# Intents (for member tracking)
intents = discord.Intents.default()
intents.members = True
intents.guilds = True

bot = commands.Bot(command_prefix="!", intents=intents)

# Configurable settings
config = {
    "log_channel": None,  # Channel for join/leave messages
    "status_channel": None,  # Channel for status embed
    "server_ip": "YOUR_SERVER_IP",
    "server_port": "YOUR_SERVER_PORT",
    "update_interval": 60  # Time in seconds between status updates
}

# Load saved settings if exists
if os.path.exists("config/config.json"):
    with open("config/config.json", "r") as f:
        config.update(json.load(f))

# --- SLASH COMMAND: SET LOG CHANNEL ---
@bot.tree.command(name="setlogchannel", description="Set the channel for player join/leave messages")
async def set_log_channel(interaction: discord.Interaction):
    config["log_channel"] = interaction.channel.id
    with open("config/config.json", "w") as f:
        json.dump(config, f)
    await interaction.response.send_message(f"Log channel set to {interaction.channel.mention} ✅", ephemeral=True)

# --- SLASH COMMAND: ADD STATUS EMBED ---
@bot.tree.command(name="statusadd", description="Add the server status embed to this channel")
async def add_status(interaction: discord.Interaction):
    config["status_channel"] = interaction.channel.id
    with open("config/config.json", "w") as f:
        json.dump(config, f)
    
    embed = discord.Embed(title="🌍 Project Zomboid Server Status", color=discord.Color.green())
    embed.add_field(name="Server IP", value=config["server_ip"], inline=True)
    embed.add_field(name="Port", value=config["server_port"], inline=True)
    embed.add_field(name="Players Online", value="Fetching...", inline=False)
    
    message = await interaction.channel.send(embed=embed)
    config["status_message"] = message.id  # Store message ID for updates
    await interaction.response.send_message("Status embed added ✅", ephemeral=True)

# --- TASK: UPDATE STATUS EMBED ---
@tasks.loop(seconds=config["update_interval"])
async def update_status():
    if not config["status_channel"]:
        return
    
    channel = bot.get_channel(config["status_channel"])
    if not channel:
        return
    
    try:
        async with aiohttp.ClientSession() as session:
            async with session.get(f"http://{config['server_ip']}:{config['server_port']}/players") as resp:
                data = await resp.json()
                players_online = len(data)

        embed = discord.Embed(title="🌍 Project Zomboid Server Status", color=discord.Color.green())
        embed.add_field(name="Server IP", value=config["server_ip"], inline=True)
        embed.add_field(name="Port", value=config["server_port"], inline=True)
        embed.add_field(name="Players Online", value=str(players_online), inline=False)

        message = await channel.fetch_message(config["status_message"])
        await message.edit(embed=embed)

    except Exception as e:
        print(f"Error updating status: {e}")

# --- EVENTS: PLAYER JOIN / LEAVE ---
@bot.event
async def on_member_join(member):
    if config["log_channel"]:
        channel = bot.get_channel(config["log_channel"])
        if channel:
            await channel.send(f"✅ **{member.display_name}** has joined the server!")

@bot.event
async def on_member_remove(member):
    if config["log_channel"]:
        channel = bot.get_channel(config["log_channel"])
        if channel:
            await channel.send(f"❌ **{member.display_name}** has left the server.")

# --- BOT STARTUP ---
@bot.event
async def on_ready():
    print(f"{bot.user} is online!")
    await bot.tree.sync()  # Sync slash commands
    update_status.start()  # Start status updates

bot.run(BOT_TOKEN)
