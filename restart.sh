docker stack down app
docker rm -f $(docker ps -aq)
docker image rm -f $(docker image ls -aq)
docker pull ghcr.io/hirbod-codes/bot:latest
docker pull ghcr.io/hirbod-codes/nginx:latest
docker pull ghcr.io/hirbod-codes/bot_client_web:latest
docker stack deploy -c swarm.yml app
