# Instructions

## set docker mtu to 1420

## To configure the bot modify appsettings.json file or docker-compose environment variables

## To Build images
```bash
sudo docker buildx build --push --tag ghcr.io/hirbod-codes/bot:latest --platform linux/amd64,linux/arm64,linux/arm,darwin/amd64,darwin/arm64,darwin/arm,windows/amd64,windows/arm64,windows/arm -f src/bot/Dockerfile.production .
```

## In production

```bash
sudo docker compose -f docker-compose.production.yml up --build --remove-orphans
```

## To configure the seq logger run

```bash
sudo docker run --rm -e ACCEPT_EULA=Y -v ./src/bot/logs:/data:rw -p 8081:80 datalust/seq
```

## To start the bot run with required StrategyName configuration variable(you can also specify in appsettings.json)

```bash
sudo docker run -d --name seq1 -e ACCEPT_EULA=Y -v ./UTLogs:/data:rw -p 8081:80 --restart unless-stopped datalust/seq && dotnet build src/bot/ -o src/bot/bin/UTStrategy && dotnet src/bot/bin/UTStrategy/bot.dll StrategyName=UT Serilog__WriteTo__1__Args__serverUrl=http://localhost:8081

sudo docker run -d --name seq2 -e ACCEPT_EULA=Y -v ./IchimokuLogs:/data:rw -p 8082:80 --restart unless-stopped datalust/seq && dotnet build src/bot/ -o src/bot/bin/IchimokuStrategy && dotnet src/bot/bin/UTStrategy/bot.dll StrategyName=Ichimoku Serilog__WriteTo__1__Args__serverUrl=http://localhost:8082

sudo docker run -d --name seq3 -e ACCEPT_EULA=Y -v ./SMMALogs:/data:rw -p 8083:80 --restart unless-stopped datalust/seq && dotnet build src/bot/ -o src/bot/bin/SMMAStrategy && dotnet src/bot/bin/UTStrategy/bot.dll StrategyName=SMMA Serilog__WriteTo__1__Args__serverUrl=http://localhost:8083

sudo docker run -d --name seq4 -e ACCEPT_EULA=Y -v ./MACrossLogs:/data:rw -p 8084:80 --restart unless-stopped datalust/seq && dotnet build src/bot/ -o src/bot/bin/MACrossStrategy && dotnet src/bot/bin/UTStrategy/bot.dll StrategyName=MACross Serilog__WriteTo__1__Args__serverUrl=http://localhost:8084
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
