FROM registry.truesight.asia/aspnet:3.1-buster-slim AS base
WORKDIR /app
RUN apt-get update && apt-get install -y net-tools curl iputils-ping telnet nano vim libc6-dev libgdiplus

FROM registry.truesight.asia/dotnet-sdk:3.1-buster AS build
WORKDIR /src
COPY ["RideSharing.csproj", "./"]
RUN dotnet restore "RideSharing.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "RideSharing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RideSharing.csproj" -c Release -o /app/publish

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

COPY ["docker-entrypoint.sh", "."]

RUN chmod a+x docker-entrypoint.sh

CMD ["./docker-entrypoint.sh"]

EXPOSE 80
