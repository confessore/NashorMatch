#!/bin/sh

sudo systemctl stop nashormatch.web.service
sudo systemctl stop nashormatch.discord.service

sudo systemctl disable nashormatch.web.service
sudo systemctl disable nashormatch.discord.service

sudo rm /etc/systemd/system/nashormatch.web.service
sudo rm /etc/systemd/system/nashormatch.discord.service
