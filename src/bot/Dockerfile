FROM mcr.microsoft.com/dotnet/sdk:7.0 AS production

WORKDIR /project

COPY . .
RUN dotnet restore -v d

COPY ./src/bot/ ./src/bot/

RUN dotnet build

WORKDIR /project/src/bot

CMD dotnet run
