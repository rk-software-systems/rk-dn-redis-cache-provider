FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
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