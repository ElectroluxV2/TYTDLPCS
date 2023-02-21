git pull
python3 -m venv ./TyranoKurwusBot
source ./TyranoKurwusBot/bin/activate
dotnet run -c Production --project TyranoKurwusBot/TyranoKurwusBot.csproj
deactivate
