# MyStore - Order Management Microservice

Современная микросервисная экосистема для управления заказами и складскими остатками, построенная на принципах **Clean Architecture** и **CQRS**. 
Проект демонстрирует реализацию отказоустойчивой системы с синхронным (gRPC) и асинхронным (RabbitMQ) взаимодействием сервисов.

## Технологический стек

- **Runtime:** .NET 9 / ASP.NET Core
- **Communication:** gRPC (HTTP/2) + MassTransit / RabbitMQ (Async Events)
- **Database:** PostgreSQL (EF Core) + Transactional Outbox
- **Caching:** Redis (Distributed Cache)
- **Observability:** OpenTelemetry + Jaeger (Tracing) + Serilog + Seq (Structured Logging) + Grafana (Metrics)
- **API Doc:** Scalar (OpenAPI 3.1)
- **Testing:** xUnit, NSubstitute, FluentAssertions
- **Infrastructure:** Docker Compose

## Архитектура

Проект следует принципам **Чистой архитектуры**:
- **Domain:** Сущности и бизнес-логика.
- **Application:** CQRS паттерн (MediatR), валидация (FluentValidation) и интерфейсы.
- **Infrastructure:** Реализация БД, репозиториев и шины сообщений.
- **API:** Тонкие контроллеры и глобальная обработка исключений.
- **Warehouse** сервис складских остатков

### Ключевые паттерны и решения:
- **CQRS:** Разделение операций чтения и записи через MediatR.
- **Синхронный gRPC (Batching):** Запрос остатков товаров (`GetBatchStocksInfo`) между API и Warehouse по чистому HTTP/2 внутри Docker-сети.
- **Transactional Outbox:** Гарантированная доставка сообщений в RabbitMQ через БД.
- **Pipeline Behaviors:** Автоматическая валидация и логирование каждой команды.
- **Global Exception Handling:** Обработка ошибок через `IExceptionHandler`.
- **Cache-Aside:** Оптимизация производительности с помощью Redis.
- **Сквозная телеметрия:** OpenTelemetry связывает HTTP-запрос, gRPC-вызов и сообщения RabbitMQ в единый трейс.

## Быстрый запуск

1. **Клонируйте репозиторий:**
```bash
git clone https://github.com/mtsiberev/CoreProject.git
```
2. **Запустите инфраструктуру (Docker):**
```bash
docker-compose up -d
```   
3. **Откройте API:**   
Интерфейс Scalar будет доступен по адресу: 
http://localhost:5000/scalar/v1

## Карта портов и панелей мониторинга

После успешного запуска контейнеров вам доступны следующие интерфейсы:

| Сервис / Панель | Внешний URL | Описание |
| :--- | :--- | :--- |
| **MyStore.Api** | [http://localhost:5000/scalar/v1](http://localhost:5000/scalar/v1) | Документация API (Scalar) и отправка заказов |
| **Seq** | [http://localhost:8090](http://localhost:8090) | Структурированные логи всей системы |
| **Jaeger UI** | [http://localhost:16686](http://localhost:16686) | Распределенная трассировка gRPC и MassTransit |
| **Grafana** | [http://localhost:3000](http://localhost:3000) | Метрики производительности (`admin` / `admin`) |
| **RabbitMQ Management** | [http://localhost:15672](http://localhost:15672) | Панель брокера сообщений (`guest` / `guest`) |


## Автоматизация (Скрипты)
В папке /scripts подготовлены PowerShell-скрипты для упрощения разработки:
./add-migration.ps1 -name [Name] - создание миграции.
./update-db.ps1 - применение изменений к БД в Docker.

