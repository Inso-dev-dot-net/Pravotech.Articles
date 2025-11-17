# Pravotech Articles API

REST API для каталога статей с автоматическим формированием разделов по наборам тегов.  
Тестовое задание. Backend: ASP.NET Core 8 + PostgreSQL + Docker.

## Функционал

- CRUD операции со статьями
- Уникальные теги, регистронезависимые (например, `Kafka` и `kAfKa` считаются одним тегом)
- До 256 тегов на статью, **оригинальный порядок тегов сохраняется**
- Автоматическое создание разделов по множеству тегов статьи
- Разделы сортируются по количеству статей (по убыванию)
- Статьи внутри раздела сортируются по `UpdatedAtUtc` → `CreatedAtUtc` (по убыванию)
- Полностью работает в Docker

## Технологии

| Компонент | Использовано |
|----------|--------------|
| Backend | ASP.NET Core 8 (Web API) |
| ORM | Entity Framework Core 8 (Code First + Migrations) |
| Database | PostgreSQL 16 |
| Admin UI | pgAdmin 4 |
| Docs | Swagger |
| Tests | xUnit + FluentAssertions |
| Deploy | Docker + docker-compose |

## Запуск через Docker

Требуется Docker Desktop.

```bash
cd docker
docker compose up --build
```

После запуска сервисы доступны:

| Сервис | URL |
|--------|-----|
| Swagger UI | http://localhost:5000/swagger |
| API Base URL | http://localhost:5000 |
| pgAdmin | http://localhost:8080 |
| PostgreSQL | localhost:5432 |

### Данные для pgAdmin

| Параметр | Значение |
|----------|----------|
| Email | admin@sometest.com |
| Password | testpwnotsecure |

Подключение к базе в pgAdmin:

| Параметр | Значение |
|----------|----------|
| Host | `postgres` |
| Port | 5432 |
| User | postgres |
| Password | postgres |
| Database | articles_db |

## API

### DTO статьи

```jsonc
{
  "id": "guid",
  "title": "string, 1..256",
  "createdAtUtc": "datetime",
  "updatedAtUtc": "datetime|null",
  "tags": ["Tag1", "Tag2", "..."]
}
```

### Endpoints `/api/articles`

| Method | Route | Описание |
|--------|-------|----------|
| GET | `/api/articles/{id}` | Получить статью |
| POST | `/api/articles` | Создать статью |
| PUT | `/api/articles/{id}` | Обновить статью |
| DELETE | `/api/articles/{id}` | Удалить статью |

### Endpoints `/api/sections`

| Method | Route | Описание |
|--------|-------|----------|
| GET | `/api/sections` | Список разделов |
| GET | `/api/sections/{id}/articles` | Статьи раздела |

## Тесты

```bash
dotnet test
```
Поведение в Domain + Api по тексту технического задания

## Архитектура

Слои реализованы в стиле DDD:

| Слой | Назначение |
|------|------------|
| Domain | Entities, ValueObjects, Domain Services |
| Application | Контракты, команды, DTO |
| Infrastructure | EF Core, миграции, PostgreSQL |
| WebApi | Контроллеры, DI, Swagger |
