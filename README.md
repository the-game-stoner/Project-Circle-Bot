# Project Circle Bot
A simple Discord bot for Project Zomboid servers, providing join/leave notifications, server status updates, and player count tracking.

## Features
- Automatic join/leave notifications in a specified Discord channel.
- Regularly updated server status embed (player count, connection info, uptime).
- Slash commands for manual checks and configuration.
- Runs inside the PZ server container for easy setup.

## Installation
### 1. Clone the Repository
On your PZ server, navigate to the working directory and clone the bot:
- git clone https://github.com/YOUR_GITHUB_USERNAME/Project-Circle-Bot.git
- cd Project-Circle-Bot

### 2. Configure the Bot
2.1 Copy the example config file:
- cp config/.env.example config/.env

2.2 Open config/.env and update it with your Discord Bot Token and server details.
- DISCORD_BOT_TOKEN=your-bot-token-here
- SERVER_IP=your-server-ip
- SERVER_PORT=your-server-port

### 3. Install Dependencies
Make sure Python 3.9+ is installed, then run:
- pip install -r requirements.txt

### 4. Start the Bot
Run the bot manually to test it:
python bot.py
If everything works, the bot should come online in Discord! 🎉

### 5. Set Up Auto-Start with PZ Server
Edit your server startup command to include:

export PATH="./jre64/bin:$PATH" ; \
export LD_LIBRARY_PATH="./linux64:./natives:.:./jre64/lib/amd64:${LD_LIBRARY_PATH}" ; \
JSIG="libjsig.so" ; \
LD_PRELOAD="${LD_PRELOAD}:${JSIG}" \
./ProjectZomboid64 -port {{SERVER_PORT}} -udpport {{STEAM_PORT}} -cachedir=/home/container/.cache -servername "{{SERVER_NAME}}" -adminusername {{ADMIN_USER}} -adminpassword "{{ADMIN_PASSWORD}}" & \
sleep 10 && \
python3 /home/container/Project-Circle-Bot/bot.py

This ensures the bot starts every time the PZ server starts.

## Commands
Command &	Description
- /status	Show current server status & player count
- /setstatuschannel	Set which channel gets the auto-updating server embed
- /setlogchannel	Set which channel receives join/leave logs
- /help	Show available commands

## Troubleshooting
### Bot isn’t coming online?
1. Make sure your bot token is correct in config/.env.
2. Double-check that the bot has permission to see & send messages in the assigned Discord channels.
If running manually, check for errors in the terminal.

### Bot isn’t restarting with the server?
Try restarting the server manually and watching the logs.
If needed, set up a process manager (tmux, screen, or systemd) to auto-restart the bot if it crashes.

## Contributing
If you’d like to improve the bot, feel free to fork the repo and submit a pull request!
