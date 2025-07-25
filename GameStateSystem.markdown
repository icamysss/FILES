# Документация системы управления игровыми состояниями

## Обзор
Система управления игровыми состояниями (`GameStateSystem`) управляет жизненным циклом игровых состояний, обеспечивая их переключение, обновление и обработку параметров. Используется для координации состояний игры, таких как инициализация, игровой процесс, пауза и обработка ошибок, с поддержкой асинхронных операций и реактивного программирования.

## Возможности
- **Переключение состояний**: Плавное изменение игровых состояний с передачей параметров.
- **Реактивное отслеживание**: Уведомления об изменениях состояния через реактивное свойство.
- **Асинхронная обработка**: Управление жизненным циклом состояний с использованием `UniTask` и токенов отмены.
- **Обработка ошибок**: Автоматический переход в состояние ошибки при сбоях.
- **Логирование**: Подробное логирование переходов и ошибок для отладки.

## Компоненты

### `BaseStateParam`
Абстрактный базовый класс для параметров состояний, определяющий контракт для сравнения объектов по значению.

#### Свойства
- **None**: Отсутствуют.

#### Методы
- **`EqualsCore`**: Сравнивает значимые поля наследника. Реализуется в наследниках.
- **`GetHashCodeCore`**: Вычисляет хэш-код на основе полей, используемых в `EqualsCore`.
- **`Equals`**: Переопределённое сравнение объектов с учётом типа.
- **`GetHashCode`**: Возвращает хэш-код объекта.
- **`operator ==` / `!=`**: Операторы сравнения для объектов `BaseStateParam`.

#### Зависимости
- **System**: Для базовых операций сравнения и хэширования.

#### Подкомпоненты
- **`ErrorStateParam`**: Хранит информацию об исключении для состояния `Error`.
  - **Свойства**: `Error` (хранит объект `Exception`).
  - **Методы**: `EqualsCore` (сравнивает сообщения ошибок), `GetHashCodeCore` (хэширует сообщение ошибки).

### `StateInfo`
Структура, содержащая информацию о состоянии игры и его параметрах.

#### Свойства
- **`NewState`**: Тип состояния (`GameStateEnum`).
- **`Params`**: Параметры состояния (`BaseStateParam`).

#### Методы
- **`ToString`**: Возвращает строковое представление состояния и параметров.

#### Зависимости
- **Common.Services.GameState.States**: Для `GameStateEnum` и `BaseStateParam`.

### `IGameStateService`
Интерфейс сервиса для изменения и отслеживания состояния игры.

#### Свойства
- **`CurrentStateRx`**: Реактивное свойство для текущего состояния (`StateInfo`).

#### Методы
- **`ChangeState`**: Изменяет текущее состояние игры с опциональными параметрами.

#### Зависимости
- **R3**: Для реактивного программирования.
- **Common.Services.GameState.States**: Для `StateInfo` и `GameStateEnum`.

### `GameStateService`
Реализация `IGameStateService` для управления игровыми состояниями.

#### Свойства
- **`Name`**: Имя сервиса (`GameStateService`).
- **`Version`**: Версия сервиса (`2.0.0`).
- **`CurrentStateRx`**: Реактивное свойство для текущего состояния.

#### Методы
- **`GameStateService`**: Конструктор, инициализирующий начальное состояние.
- **`ChangeState`**: Изменяет состояние, игнорируя дублирующиеся запросы, с логированием.

#### Зависимости
- **R3**: Для реактивного свойства `CurrentStateRx`.
- **Common.Utils.Logging**: Для логирования.
- **Common.Services.GameState.States**: Для `StateInfo` и `GameStateEnum`.

### `IGameStateProcessor`
Интерфейс процессора игровых состояний.

#### Методы
- **`StartLoop`**: Запускает цикл обработки состояний.

#### Зависимости
- **Cysharp.Threading.Tasks**: Для асинхронной обработки.

### `GameStateProcessor`
Реализация процессора игровых состояний, управляющая переходами и обновлениями.

#### Свойства
- **`Name`**: Имя сервиса (`GameStateProcessor`).
- **`Version`**: Версия сервиса (`2.2.0`).

#### Методы
- **`GameStateProcessor`**: Конструктор, регистрирующий состояния и подписку.
- **`StartLoop`**: Запускает цикл обработки изменений и обновлений.
- **`Dispose`**: Освобождает ресурсы, завершая операции.
- **`OnStateChanged`**: Обрабатывает запросы на изменение состояния.
- **`ProcessStateChangeRequests`**: Асинхронно обрабатывает очередь изменений.
- **`HandleStateTransition`**: Выполняет переход между состояниями.
- **`RunStateMachine`**: Выполняет цикл обновления текущего состояния.
- **`HandleFatalError`**: Обрабатывает фатальные ошибки, переходя в `Error`.

#### Зависимости
- **Common.Services.GameState.States**: Для `IGameState`, `StateInfo`, `GameStateEnum`.
- **Common.Services.CancellationTokenManager**: Для токенов отмены.
- **Common.Utils.Logging**: Для логирования.
- **R3**: Для подписки на изменения состояния.
- **Cysharp.Threading.Tasks**: Для асинхронных операций.
- **System.Linq**: Для работы с коллекциями.

### `IGameState`
Интерфейс для игровых состояний, определяющий их жизненный цикл.

#### Свойства
- **`StateEnum`**: Тип состояния (`GameStateEnum`).

#### Методы
- **`OnEnter`**: Вызывается при входе в состояние.
- **`Update`**: Вызывается для обновления состояния.
- **`OnExit`**: Вызывается при выходе из состояния.

#### Зависимости
- **Cysharp.Threading.Tasks**: Для асинхронных методов.
- **System.Threading**: Для токенов отмены.

#### Подкомпоненты
- **`GameStateEnum`**: Перечисление состояний (`None`, `Initializing`, `Loading`, `NewGame`, `Playing`, `Paused`, `GameOver`, `Exiting`, `Error`).

### `BaseGameState`
Абстрактный базовый класс для игровых состояний, реализующий `IGameState`.

#### Свойства
- **`StateEnum`**: Тип состояния (абстрактное).

#### Методы
- **`OnEnter`**: Логирует вход и возвращает завершенную задачу.
- **`Update`**: Возвращает завершенную задачу.
- **`OnExit`**: Логирует выход и возвращает завершенную задачу.

#### Зависимости
- **Common.Utils.Logging**: Для логирования.
- **Cysharp.Threading.Tasks**: Для асинхронных операций.

## Использование

### Инициализация
```csharp
var states = new IGameState[] { new PlayingState(), new PausedState() };
var ctProvider = new CancellationTokenProvider();
var gameStateService = new GameStateService();
var processor = new GameStateProcessor(gameStateService, ctProvider, states);
await processor.StartLoop();
```

### Изменение состояния
```csharp
gameStateService.ChangeState(GameStateEnum.Playing, new PlayingStateParam());
```

### Завершение
```csharp
processor.Dispose();
```

## Иерархия/Структура
- **`GameStateService`**: Управляет текущим состоянием и уведомляет о его изменениях через `CurrentStateRx`.
- **`GameStateProcessor`**: Обрабатывает переходы и обновления, координируя вызовы `OnEnter`, `Update`, `OnExit`.
- **Состояния** (`IGameState`): Реализуют логику конкретных состояний, наследуясь от `BaseGameState`.
- **Параметры** (`BaseStateParam`): Передаются при переходе в состояние, наследуются для конкретных данных.
- **Жизненный цикл**:
  1. Инициализация сервиса и процессора.
  2. Запуск цикла обработки (`StartLoop`).
  3. Обработка изменений состояния через `ChangeState`.
  4. Вызов `OnEnter`, `Update`, `OnExit` для каждого состояния.
  5. Завершение через `Dispose`.

## Логирование
- **Логгер**: Используется `Logger` из `Common.Utils.Logging`.
- **Уровни**:
  - `LogDevelopment`: Для отладочной информации (инициализация, регистрация состояний).
  - `LogInfo`: Для переходов и ключевых событий.
  - `LogWarning`: Для блокировок или отмен операций.
  - `LogError`: Для фатальных ошибок с переходом в `Error`.

## Примечания
- **Токены отмены**: Используйте `ICancellationTokenProvider` для управления жизненным циклом приложения.
- **Реактивность**: Подписывайтесь на `CurrentStateRx` для отслеживания изменений.
- **Потокобезопасность**: `GameStateProcessor` обрабатывает состояния последовательно, избегая конфликтов.
- **Обработка ошибок**: При сбоях система переходит в `Error` с параметром `ErrorStateParam`.