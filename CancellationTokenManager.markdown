# Документация сервиса CancellationTokenManager

## Обзор
Сервис `CancellationTokenManager` управляет токенами отмены асинхронных операций в приложениях Unity, предоставляя токены, привязанные к жизненным циклам приложения, сцены и игровых объектов. Реализует интерфейс `ICancellationTokenProvider` и обеспечивает корректную очистку ресурсов.

## Возможности
- **Управление токенами**: Для контекстов приложения, сцены, игрового процесса, интерфейса и пользовательских контекстов.
- **Иерархическая связь**: Токены сцены отменяются при выходе из приложения.
- **Поддержка игровых объектов**: Токены, привязанные к сцене.
- **Автоматическая отмена**: При выходе из приложения или смене сцены.
- **Интеграция**: С `ISceneLoader` для отслеживания смены сцен.

## Компоненты

### CancellationTokenManager
Основной класс сервиса для создания, управления и удаления токенов.

#### Свойства
- **Name**: Название сервиса ("CancellationTokenManager").
- **Version**: Версия сервиса ("3.1.0").
- **ApplicationLifetimeToken**: Токен, отменяемый при выходе или перезапуске приложения.
- **SceneLifetimeToken**: Токен, сбрасываемый при смене сцены.

#### Методы
- **`GetContextToken(LifetimeContextType, LifetimeContextType?)`**: Возвращает или создаёт токен для контекста, опционально привязанный к родительскому.
- **`ResetContextToken(LifetimeContextType)`**: Сбрасывает токен контекста и отменяет дочерние.
- **`GetTokenForGameObject(GameObject)`**: Возвращает токен для игрового объекта, привязанный к сцене.
- **`GetCtsForGameObject(GameObject)`**: Возвращает `CancellationTokenSource` для игрового объекта.
- **`RemoveGameObjectEntry(GameObject)`**: Отменяет и удаляет токен игрового объекта.
- **`Dispose()`**: Очищает все токены и подписки.

#### Зависимости
- **ISceneLoader**: Для отслеживания смены сцен.
- **R3, Cysharp.Threading.Tasks**: Для реактивного программирования и асинхронных операций.
- **Common.Utils.Logging.Logger**: Для логирования.

### GameObjectCancellationTokenProvider
Компонент MonoBehaviour для управления токенами игровых объектов.

#### Методы
- **`SetCancellationTokenSource(CancellationTokenSource, ICancellationTokenProvider)`**: Назначает источник токена и ссылку на менеджер.
- **`OnDestroy()`**: Отменяет и освобождает токен, уведомляя менеджер.

### ICancellationTokenProvider
Интерфейс, определяющий контракт для управления токенами.

#### Свойства
- **ApplicationLifetimeToken**
- **SceneLifetimeToken**

#### Методы
- **`GetContextToken(LifetimeContextType, LifetimeContextType?)`**
- **`GetTokenForGameObject(GameObject)`**
- **`GetCtsForGameObject(GameObject)`**
- **`RemoveGameObjectEntry(GameObject)`**

#### Перечисление: LifetimeContextType
- **Application**: Жизненный цикл приложения.
- **Scene**: Жизненный цикл сцены (родитель: Application).
- **Gameplay**: Контекст игрового процесса (родитель: Scene).
- **UI**: Контекст интерфейса (родитель: Scene).
- **Custom**: Пользовательский контекст (родитель: Application).

## Использование

### Инициализация
```csharp
var sceneLoader = // Получить экземпляр ISceneLoader
var manager = new CancellationTokenManager(sceneLoader);
```

### Получение токенов
```csharp
// Токен приложения
var appToken = manager.ApplicationLifetimeToken;

// Токен сцены
var sceneToken = manager.SceneLifetimeToken;

// Токен контекста
var gameplayToken = manager.GetContextToken(LifetimeContextType.Gameplay);

// Токен игрового объекта
var goToken = manager.GetTokenForGameObject(gameObject);
```

### Обработка смены сцен
Менеджер автоматически сбрасывает токены, связанные со сценой, при её смене через `ISceneLoader`.

### Очистка
```csharp
manager.Dispose(); // Очищает все токены и подписки
```

## Иерархия контекстов
- **Application**: Корневой контекст, отменяется при выходе.
- **Scene**: Дочерний от Application, сбрасывается при смене сцены.
- **Gameplay/UI**: Дочерние от Scene, отменяются при смене сцены.
- **Custom**: Дочерний от Application, управляется пользователем.
- **GameObject**: Привязан к Scene, отменяется при уничтожении объекта или смене сцены.

## Логирование
- **Логи**: Использует `Common.Utils.Logging.Logger` для информационных, предупреждающих и отладочных логов.
- **Отслеживание**: Создание, отмена и удаление токенов.

## Примечания
- **Настройка**: Убедитесь, что `ISceneLoader` настроен для корректного отслеживания смены сцен.
- **Токены объектов**: Требуется компонент `GameObjectCancellationTokenProvider` для автоматической очистки.
- **Очистка**: Вызывайте `Dispose()` при завершении работы сервиса для предотвращения утечек памяти.

