FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SampleMicroservice.Shared/SampleMicroservice.Shared.csproj", "SampleMicroservice.Shared/"]
COPY ["src/WebAPI.Frontend/WebAPI.Frontend.csproj", "WebAPI.Frontend/"]
RUN dotnet restore "WebAPI.Frontend/WebAPI.Frontend.csproj"
COPY . .

WORKDIR "src/WebAPI.Frontend"
RUN dotnet build "WebAPI.Frontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPI.Frontend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WebAPI.Frontend.dll"]