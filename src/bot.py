
import discord
import asyncio
import os
import json
from datetime import datetime
from discord.ext import commands

# Load bot token from config
CONFIG_PATH = "config/config.json"

if not os.path.exists(CONFIG_PATH):
    print(f"Config file not found at {CONFIG_PATH}. Please create it.")
    exit(1)

with open(CONFIG_PATH, "r") as config_file:
    config = json.load(config_file)

TOKEN = config.get("bot_token")

# Bot setup
intents = discord.Intents.default()
intents.messages = True
intents.guilds = True
intents.members = True  # Required for tracking joins/leaves

bot = commands.Bot(command_prefix="!", intents=intents)

# Event: Bot Ready
@bot.event
async def on_ready():
    print(f"{bot.user.name} is online!")
    await bot.change_presence(activity=discord.Game("Monitoring PZ Server"))

# Event: Player Joins Server (Simulated)
@bot.event
async def on_member_join(member):
    channel = discord.utils.get(member.guild.text_channels, name="server-log")
    if channel:
        await channel.send(f"🔹 **{member.name}** has joined the server!")

# Event: Player Leaves Server (Simulated)
@bot.event
async def on_member_remove(member):
    channel = discord.utils.get(member.guild.text_channels, name="server-log")
    if channel:
        await channel.send(f"❌ **{member.name}** has left the server!")

# Command: Server Status
@bot.command(name="status")
async def server_status(ctx):
    uptime = datetime.now() - bot.start_time
    embed = discord.Embed(
        title="📊 Server Status",
        description=f"**Uptime:** {uptime}",
        color=discord.Color.green()
    )
    await ctx.send(embed=embed)

# Start bot
bot.start_time = datetime.now()
bot.run(TOKEN)
