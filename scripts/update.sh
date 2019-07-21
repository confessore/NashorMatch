#!/bin/sh

sudo service nashormatch.web stop
sudo service nashormatch.discord stop

cd /home/orfasanti/nashormatch
sudo git pull origin master

cd /home/orfasanti/nashormatch/src/NashorMatch.Web
sudo dotnet publish -c Release -o /var/aspnetcore/NashorMatch.Web

cd /home/orfasanti/nashormatch/src/NashorMatch.Discord
sudo dotnet publish -c Release -o /var/dotnetcore/NashorMatch.Discord

sudo service nashormatch.web start
sudo service nashormatch.discord start
