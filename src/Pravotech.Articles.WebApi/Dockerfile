# Базовый runtime образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Образ для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj'и, чтобы корректно отработал restore
COPY ["src/Pravotech.Articles.WebApi/Pravotech.Articles.WebApi.csproj", "src/Pravotech.Articles.WebApi/"]
COPY ["src/Pravotech.Articles.Infrastructure/Pravotech.Articles.Infrastructure.csproj", "src/Pravotech.Articles.Infrastructure/"]
COPY ["src/Pravotech.Articles.Application/Pravotech.Articles.Application.csproj", "src/Pravotech.Articles.Application/"]
COPY ["src/Pravotech.Articles.Domain/Pravotech.Articles.Domain.csproj", "src/Pravotech.Articles.Domain/"]

RUN dotnet restore "src/Pravotech.Articles.WebApi/Pravotech.Articles.WebApi.csproj"

# Копируем весь код и публикуем
COPY . .
WORKDIR "/src/src/Pravotech.Articles.WebApi"
RUN dotnet publish "Pravotech.Articles.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Pravotech.Articles.WebApi.dll"]
