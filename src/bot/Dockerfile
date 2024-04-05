FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0 AS base

ARG TARGETPLATFORM
ARG TARGETOS
ARG TARGETARCH
ARG TARGETVARIANT
ARG BUILDPLATFORM
ARG BUILDOS
ARG BUILDARCH
ARG BUILDVARIANT
RUN echo "Building on $BUILDPLATFORM, targeting $TARGETPLATFORM"
RUN echo "Building on ${BUILDOS} and ${BUILDARCH} with optional variant ${BUILDVARIANT}"
RUN echo "Targeting ${TARGETOS} and ${TARGETARCH} with optional variant ${TARGETVARIANT}"

WORKDIR /app

COPY ./trading_bot.sln ./
COPY ./src/bot/bot.csproj ./src/bot/
COPY ./src/StrategyTester/StrategyTester.csproj ./src/StrategyTester/
COPY ./Tests/unit_tests/unit_tests.csproj ./Tests/unit_tests/

FROM base AS dev

WORKDIR /app

RUN dotnet restore

RUN dotnet dev-certs https --clean
RUN dotnet dev-certs https --trust

COPY ./src/bot/ ./src/bot/

WORKDIR /app/src/bot/

RUN dotnet build --no-restore -o /build

EXPOSE 5001 5000

CMD dotnet /build/bot.dll

FROM base AS publish

WORKDIR /app

COPY ./src/bot/ ./src/bot/

WORKDIR /app/src/bot/

RUN dotnet dev-certs https --clean
RUN dotnet dev-certs https --trust

RUN dotnet publish -c Release -o /publish -a $TARGETARCH

EXPOSE 5001

CMD dotnet /publish/bot.dll
