#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Integration/TestCluster/TestCluster.csproj", "Integration/TestCluster/"]
RUN dotnet restore "Integration/TestCluster/TestCluster.csproj"
COPY . .
WORKDIR "/src/Integration/TestCluster"
RUN dotnet build "TestCluster.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestCluster.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestCluster.dll"]