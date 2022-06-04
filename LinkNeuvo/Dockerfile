FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LinkNeuvo/LinkNeuvo.csproj", "LinkNeuvo/"]
RUN dotnet restore "LinkNeuvo/LinkNeuvo.csproj"
COPY . .
WORKDIR "/src/LinkNeuvo"
RUN dotnet build "LinkNeuvo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LinkNeuvo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LinkNeuvo.dll"]
