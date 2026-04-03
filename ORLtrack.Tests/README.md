# ORLtrack.Tests

Тестовый проект для приложения учета учеников и занятий.

## Покрытие

### 1. Тестирование функциональной части

Проверяются ключевые бизнес-сценарии:

- добавление ученика и стартового пополнения;
- проведение занятия со списанием баланса;
- фиксация пропуска без списания;
- отметка пропуска как отработанного;
- фильтрация пропусков на странице ученика.

Файлы:

- `FunctionalPartTests.cs`
- `Infrastructure/PostgresTestDatabase.cs`
- `Infrastructure/HomeControllerTestHost.cs`

### 2. Тестирование пользовательского интерфейса и сценариев использования

Проверяются:

- наличие фильтра периода на странице учеников;
- переход на отдельную страницу ученика;
- наличие управления статусом `Отработано`;
- стили для прокручиваемых колонок и боковой панели;
- сквозной сценарий: пропуск -> отметка `Отработано` -> отображение в карточке ученика.

Файлы:

- `UserInterfaceAndUsageScenarioTests.cs`

## Запуск

Сборка:

```powershell
dotnet build .\ORLtrack.Tests\ORLtrack.Tests.csproj
```

Запуск тестов:

```powershell
dotnet run --project .\ORLtrack.Tests\ORLtrack.Tests.csproj
```

Если основное приложение в этот момент запущено и блокирует пересборку, можно выполнить:

```powershell
dotnet build .\ORLtrack.Tests\ORLtrack.Tests.csproj
dotnet .\ORLtrack.Tests\bin\Debug\net8.0\ORLtrack.Tests.dll
```
