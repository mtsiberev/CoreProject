# MyStore — Order Management Microservice

Современный микросервис управления заказами, построенный на принципах **Clean Architecture** и **CQRS**. 
Проект демонстрирует реализацию отказоустойчивой системы с асинхронным взаимодействием.

## Технологический стек

- **Runtime:** .NET 9 / ASP.NET Core
- **Database:** PostgreSQL (EF Core)
- **Messaging:** RabbitMQ + MassTransit (Async Events)
- **Caching:** Redis (Distributed Cache)
- **Observability:** Serilog + Seq (Structured Logging)
- **API Doc:** Scalar (OpenAPI 3.1)
- **Testing:** xUnit, NSubstitute, FluentAssertions
- **Infrastructure:** Docker Compose

## Архитектура

Проект следует принципам **Чистой архитектуры**:
- **Domain:** Сущности и бизнес-логика.
- **Application:** CQRS паттерн (MediatR), валидация (FluentValidation) и интерфейсы.
- **Infrastructure:** Реализация БД, репозиториев и шины сообщений.
- **API:** Тонкие контроллеры и глобальная обработка исключений.

### Ключевые паттерны и решения:
- **CQRS:** Разделение операций чтения и записи через MediatR.
- **Transactional Outbox:** Гарантированная доставка сообщений в RabbitMQ через БД.
- **Pipeline Behaviors:** Автоматическая валидация и логирование каждой команды.
- **Global Exception Handling:** Обработка ошибок через `IExceptionHandler`.
- **Cache-Aside:** Оптимизация производительности с помощью Redis.

## Быстрый запуск

1. **Клонируйте репозиторий:**
   ```bash
   git clone https://github.com

2. **Запустите инфраструктуру (Docker):**
   ```bash
   docker-compose up -d
   
3. **Откройте API:**   
Интерфейс Scalar будет доступен по адресу: 
https://localhost:5001/scalar/v1)

## Автоматизация (Скрипты)
В папке /scripts подготовлены PowerShell-скрипты для упрощения разработки:
./add-migration.ps1 -name [Name] — создание миграции.
./update-db.ps1 — применение изменений к БД в Docker.

## Мониторинг логов
Для просмотра логов перейдите в Seq: http://localhost:8090