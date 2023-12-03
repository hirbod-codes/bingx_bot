FROM mcr.microsoft.com/dotnet/sdk:7.0 AS production

WORKDIR /project

COPY ./bingx_test.csproj ./
RUN dotnet restore -v d

COPY . .

RUN dotnet publish --no-restore -c Release -o /publish

WORKDIR /publish

CMD dotnet user_management.dll 
