# Instructions

## To configure the bot modify appsettings.json file

## To configure the seq logger run

```bash
sudo docker run -d --name seq1 -e ACCEPT_EULA=Y -v ./logs:/data:rw -p 8081:80 --restart unless-stopped datalust/seq:5.1
```

## To start the bot run with required StrategyName configuration variable(you can also specify in appsettings.json)

```bash
sudo docker run -d --name seq1 -e ACCEPT_EULA=Y -v ./UTLogs:/data:rw -p 8081:80 --restart unless-stopped datalust/seq:5.1 && dotnet build src/bot/ -o src/bot/bin/UTStrategy && dotnet src/bot/bin/UTStrategy/bot.dll StrategyName=UT Serilog__WriteTo__1__Args__serverUrl=http://localhost:8081

sudo docker run -d --name seq2 -e ACCEPT_EULA=Y -v ./IchimokuLogs:/data:rw -p 8082:80 --restart unless-stopped datalust/seq:5.1 && dotnet build src/bot/ -o src/bot/bin/IchimokuStrategy && dotnet src/bot/bin/UTStrategy/bot.dll StrategyName=Ichimoku Serilog__WriteTo__1__Args__serverUrl=http://localhost:8082

sudo docker run -d --name seq3 -e ACCEPT_EULA=Y -v ./SMMALogs:/data:rw -p 8083:80 --restart unless-stopped datalust/seq:5.1 && dotnet build src/bot/ -o src/bot/bin/SMMAStrategy && dotnet src/bot/bin/UTStrategy/bot.dll StrategyName=SMMA Serilog__WriteTo__1__Args__serverUrl=http://localhost:8083

sudo docker run -d --name seq4 -e ACCEPT_EULA=Y -v ./MACrossLogs:/data:rw -p 8084:80 --restart unless-stopped datalust/seq:5.1 && dotnet build src/bot/ -o src/bot/bin/MACrossStrategy && dotnet src/bot/bin/UTStrategy/bot.dll StrategyName=MACross Serilog__WriteTo__1__Args__serverUrl=http://localhost:8084
#   .
#   .
#   .
```

## To publish run

```bash
dotnet publish -c Release -r linux-x64 --sc true -o bin/Release/linux-x64 && \
dotnet publish -c Release -r win-x64 --sc true -o bin/Release/win-x64 && \
dotnet publish -c Release -r win-x86 --sc true -o bin/Release/win-x86
```
