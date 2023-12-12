# Instructions

## To configure the bot modify appsettings.json file

## To configure the seq logger run

```bash
sudo docker run -d --name seq1 -e ACCEPT_EULA=Y -v ./logs:/data:rw -p 8081:80 --restart unless-stopped datalust/seq:5.1
```

## To start the bot run

```bash
dotnet run
```
