ARG CONFIGURATION=Release
ARG DOCKERFILE_VERSION=1.0.0
ARG SDK=3.1.101
ARG RUNTIME=3.1

FROM mcr.microsoft.com/dotnet/core/sdk:${SDK} AS builder
ARG CONFIGURATION
ARG DOCKERFILE_VERSION
ARG NUGET_CONFIG_FILE=NuGet.config
ENV NUGET_CONFIG_FILE=${NUGET_CONFIG_FILE}
LABEL dockerfile="version $DOCKERFILE_VERSION"

COPY ./global.json ./global.json
COPY ./nuget.config ./NuGet.config

RUN (if [ ! -f "$NUGET_CONFIG_FILE" ]; then wget --no-verbose https://gitlab.trgdev.com/thor/auto-devops/raw/master/NuGet.config; fi )

COPY ["./src/WSClient/WSClient.csproj", "./WSClient/WSClient.csproj"]
COPY ["./src/WSServer/WSServer.csproj", "./WSServer/WSServer.csproj"]
COPY ["./src/WSServer.Contracts/WSServer.Contracts.csproj", "./WSServer.Contracts/WSServer.Contracts.csproj"]
COPY ["./src/WS.sln", "./WS.sln"]

RUN dotnet restore --configfile $NUGET_CONFIG_FILE -v:minimal -warnaserror

COPY src/ .

RUN dotnet build --no-restore -c ${CONFIGURATION} -v:minimal
RUN dotnet publish --no-build -c ${CONFIGURATION} -o /app/server WSServer/WSServer.csproj

FROM mcr.microsoft.com/dotnet/core/runtime:${RUNTIME} AS runtime
ARG DOCKERFILE_VERSION
ENV LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8 \
    LANGUAGE=en_US:en \
    TZ=UTC
WORKDIR /app/server
EXPOSE 8085

COPY --from=builder /app /app

ENTRYPOINT ["dotnet", "WSServer.dll"]