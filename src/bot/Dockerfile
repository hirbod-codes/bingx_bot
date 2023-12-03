FROM mcr.microsoft.com/dotnet/sdk:7.0 AS production

WORKDIR /project

COPY ./bot.csproj ./
RUN dotnet restore -v d

COPY . .

RUN dotnet publish --no-restore -c Release -o /publish

WORKDIR /publish

CMD dotnet bot.dll 
