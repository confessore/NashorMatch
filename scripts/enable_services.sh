#!/bin/sh

sudo systemctl enable nashormatch.web.service
sudo systemctl enable nashormatch.discord.service

sudo systemctl start nashormatch.web.service
sudo systemctl start nashormatch.discord.service
