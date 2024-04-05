FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SampleMicroservice.Shared/SampleMicroservice.Shared.csproj", "SampleMicroservice.Shared/"]
COPY ["src/WebAPI.Backend/WebAPI.Backend.csproj", "WebAPI.Backend/"]
RUN dotnet restore "WebAPI.Backend/WebAPI.Backend.csproj"
COPY . .

WORKDIR "src/WebAPI.Backend"
RUN dotnet build "WebAPI.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPI.Backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WebAPI.Backend.dll"]