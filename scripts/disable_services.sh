#!/bin/sh

sudo systemctl stop nashormatch.web.service
sudo systemctl stop nashormatch.discord.service

sudo systemctl disable nashormatch.web.service
sudo systemctl disable nashormatch.discord.service
