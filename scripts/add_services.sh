#!/bin/sh

sudo systemctl stop nashormatch.web.service
sudo systemctl stop nashormatch.discord.service

sudo systemctl disable nashormatch.web.service
sudo systemctl disable nashormatch.discord.service

read -p "NashorMatch Discord Application Id: " discordId
read -p "NashorMatch Discord Application Secret: " discordSecret
read -p "NashorMatch Discord Application Token: " discordToken
read -p "NashorMatch Riot API Key: " riotKey

sudo cp ./services/nashormatch.web.service ./services/nashormatch.web.service.backup
sudo cp ./services/nashormatch.discord.service ./services/nashormatch.discord.service.backup

sudo sed -i '/NashorMatchDiscordId=/s/$/'"$discordId"'/' ./services/nashormatch.web.service.backup
sudo sed -i '/NashorMatchDiscordSecret=/s/$/'"$discordSecret"'/' ./services/nashormatch.web.service.backup
sudo sed -i '/NashorMatchDiscordToken=/s/$/'"$discordToken"'/' ./services/nashormatch.discord.service.backup
sudo sed -i '/NashorMatchRiotKey=/s/$/'"$riotKey"'/' ./services/nashormatch.discord.service.backup

sudo mv ./services/nashormatch.web.service.backup /etc/systemd/system/nashormatch.web.service
sudo mv ./services/nashormatch.discord.service.backup /etc/systemd/system/nashormatch.discord.service

sudo systemctl enable nashormatch.web.service
sudo systemctl enable nashormatch.discord.service

sudo systemctl start nashormatch.web.service
sudo systemctl start nashormatch.discord.service
