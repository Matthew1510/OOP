# LogiCoreSystem

Ядро информационной системы транспортно-логистической компании.

## Архитектура и структура

Решение разделено на два проекта:

- `Domain` — доменная модель и бизнес-правила; не зависит от консоли.
- `App` — Меню, демо-сценарий, вывод отчётов.

```text
LogiCoreSystem/
├── App/
│   ├── Console/
│   │   ├── Commands/          # команды CLI и ConsoleContext
│   │   └── ConsoleApplication.cs
│   ├── Program.cs
│   └── LogiCore.App.csproj
├── Domain/
│   ├── Collections/           # Repository<T>
│   ├── Common/                # настройки и общие интерфейсы
│   ├── Decorators/            # добавочные услуги к стоимости доставки
│   ├── Services/              # DeliveryService и валидатор грузов
│   ├── Strategies/            # тарифные стратегии
│   ├── Cargo.cs               # иерархия грузов
│   ├── Customer.cs
│   ├── Exceptions.cs
│   ├── Order.cs
│   ├── Route.cs
│   ├── Vehicle.cs             # иерархия транспорта
│   ├── VehicleFactory.cs
│   └── LogiCore.Domain.csproj
├── Docs/
│   ├── demo-input.txt         # команды автоматического демо
│   ├── APP-FLOW.md            # подробное описание запуска и сценария
│   └── LogiCore.puml          # UML-диаграмма
└── LogiCoreSystem.sln
```

## Запуск

```bash
dotnet restore
dotnet build
dotnet run --project App/LogiCore.App.csproj
```

При обычном запуске открывается интерактивный режим. Чтобы выполнить команды
из файла, передайте путь к нему аргументом приложения, например:

```bash
dotnet run --project App/LogiCore.App.csproj -- Docs/demo-input.txt
dotnet App/bin/Debug/net9.0/LogiCore.App.dll Docs/demo-input.txt
```

Этот короткий сценарий создаёт два заказа с разными типами грузов, показывает
ручное и автоматическое назначение транспорта и завершает обе доставки.

Доступны команды `help`, `interfaces`, `create`, `orders`, `vehicles`, `assign`,
`autochoose`, `start`, `complete`, `cancel`, `exit`.

## Соответствие требованиям ТЗ



| Требование | Реализация / что осталось |
| --- | --- |
| 3.1. Иерархия транспорта | абсьтрактныйткласс, 5 наследников, расчет стоимости, переопределение методов|
| 3.2. Грузы и совместимость | Пять типов грузов и их проверки  |
| 3.3. Заказ, клиент, маршрут | История заказов, `RoutePoint`, оператор `-`, маршрут, статусы и переходы. |
| 3.4. Диспетчер | Создание заказа, совместимость, подбор транспорта, переходы статусов и выручка — `DeliveryService`. События объявлены. |
| T1. Инкапсуляция | Инварианты, приватные сеттеры и read-only свойства в доменных классах; публичный API снабжён XML-комментариями . Некоторые конфигурационные/тарифные свойства доступны для изменения без проверки. |
| T2. Наследование и полиморфизм | `abstract`, `virtual`, `override`, `sealed` и вызовы `base.CanCarry` — `Vehicle.cs`, `Cargo.cs`. |
| T3. Интерфейсы и вариантность | Интерфейсы `IIdentifiable`, `IValidator<in T>`, `IReadOnlyRepository<out T>`, интерфейсы грузов и CLI — `ValidationContracts.cs`, `Cargo.cs`. Команда `interfaces` демонстрирует оба преобразования. |
| T4. Обобщения и собственная коллекция | `Repository<T>` используется для хранения заказов, клиентов и транспорта — `Repository.cs`, `ConsoleContext.cs`. |
| T5. Делегаты и события | Четыре события `DeliveryService` имеют подписчиков `ConsoleNotifier` и `EventFileLogger`; демо показывает уведомления, запись в `logs/events.log` и отписку логгера. |
| T6. Исключения | `LogisticsException` — базовый тип доменных ошибок; от него наследуются `RouteNotFoundException`, `InvalidOrderStateException`, ошибки валидации, несовместимости и перегруза — [`Exceptions.cs`](./Domain/Exceptions.cs). CLI обрабатывает доменные исключения и использует фильтры `when` для ошибок отсутствующего маршрута и перегруза транспорта. |
| T7. Паттерны | Factory Method, Strategy, Decorator, Singleton, Command и Observer применяются в сценариях ниже. Singleton создаёт экземпляр через `Lazy<T>`. |
| T10. Enum и структуры | Есть `VehicleState`, `OrderStatus`, `RoutePoint` и перегрузка оператора — `Vehicle.cs`, `Order.cs`, `Route.cs`. |
| Разделы 6, 7, 9. Демо, качество, сдача | `Docs/demo-input.txt` показывает короткий сценарий из двух заказов с ручным и автоматическим назначением транспорта. Полный сценарий с 6 ТС, 5 заказами, 10 грузами, отчётами и JSON round-trip отсутствует. |


## Паттерны проектирования

| Паттерн | Где и как используется |
| --- | --- |
| Factory Method | Абстрактная `VehicleFactory` и конкретные фабрики создают типы машин; `CargoFactory` создаёт выбранный в CLI тип груза — [`VehicleFactory.cs`](./Domain/VehicleFactory.cs). |
| Strategy | `DeliveryService` выбирает `StandardTariff` или `HeavyCargoTariff` по суммарному весу груза — [`TariffStrategies.cs`](./Domain/Strategies/TariffStrategies.cs), [`DeliveryService.cs`](./Domain/Services/DeliveryService.cs). |
| Decorator | `InsuranceDecorator` и `PriorityDecorator` добавляют выбранные пользователем услуги к тарифной цене; этапы цены выводятся в CLI — [`DeliveryCostDecorators.cs`](./Domain/Decorators/DeliveryCostDecorators.cs). |
| Singleton | `LogisticsSettings.Instance` предоставляет единый экземпляр настроек приложения — [`LogisticsSettings.cs`](./Domain/Common/LogisticsSettings.cs). |
| Command | Каждая CLI-операция реализует `IConsoleCommand`; приложение хранит команды через общий интерфейс и запускает выбранную — [`ConsoleApplication.cs`](./App/Console/ConsoleApplication.cs), [`Commands/`](./App/Console/Commands). |

## Примеры принципов SOLID



### S — Single Responsibility Principle

У CLI-команд разделены обязанности: [`CreateOrderCommand`](./App/Console/Commands/CreateOrderCommand.cs)
собирает пользовательский ввод и вызывает сервис; правила совместимости
сосредоточены отдельно в `CargoCompatibilityValidator`
([`DeliveryService.cs`](./Domain/Services/DeliveryService.cs)). Изменение
правил проверки груза не требует переписывать код чтения ввода команды.

### O — Open/Closed Principle

Стоимость рассчитывается через переопределяемый `Vehicle.CalculateDeliveryCost`,
а типовые фабрики транспорта создаются отдельными наследниками
`VehicleFactory` ([`Vehicle.cs`](./Domain/Vehicle.cs),
[`VehicleFactory.cs`](./Domain/VehicleFactory.cs)). Можно добавить новый класс
транспорта с собственной формулой, не меняя формулы существующих классов.
Однако статический выбор транспорта и `CargoFactory` содержат ветвления по
строковому типу, поэтому расширение через эти точки потребует правок.

### L — Liskov Substitution Principle

Сервис подбора работает с `Vehicle`, а не с конкретным грузовиком или самолётом:
он перебирает `Repository<Vehicle>`, вызывает общий `CanCarry` и
`CalculateDeliveryCost`. Конкретные виды транспорта можно передавать этому
алгоритму как базовый `Vehicle`; ограничения конкретного типа реализуются в
переопределениях.

### I — Interface Segregation Principle

Клиенты и грузы не обязаны реализовывать один большой интерфейс. Например,
`ITemperatureSensitive` нужен скоропортящемуся грузу, а
`IInsurable` предоставляет страховую стоимость. Для чтения коллекции выделен
отдельный `IReadOnlyRepository<T>` без операций изменения —
[`Cargo.cs`](./Domain/Cargo.cs), [`ValidationContracts.cs`](./Domain/Common/ValidationContracts.cs).

### D — Dependency Inversion Principle

`ConsoleApplication` хранит и запускает команды через `IConsoleCommand`, а не
через тип конкретной команды; тарифный расчёт зависит от `ITariffStrategy`.
Это даёт примеры зависимости от абстракций. При этом `DeliveryService` сейчас
сам создаёт конкретные тарифы, поэтому внедрение стратегии через конструктор
было бы более полным применением DIP.

## Демо и UML

- Автоматический ввод команд при запуске: [`Docs/demo-input.txt`](./Docs/demo-input.txt).
- Полное описание работы программы: [`Docs/APP-FLOW.md`](./Docs/APP-FLOW.md).
- Диаграмма основных классов: [`Docs/LogiCore.puml`](./Docs/LogiCore.puml).
