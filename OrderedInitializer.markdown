# Руководство по сервису упорядоченной инициализации

## Обзор
Класс `OrderedInitializerBase<TService>` предоставляет инфраструктуру для асинхронной упорядоченной инициализации сервисов с групповым параллелизмом. Сервисы группируются по приоритету (порядку) и инициализируются последовательно по группам, с параллельным выполнением внутри каждой группы.

## Особенности
- **Упорядоченное выполнение**: Сервисы группируются по приоритету и инициализируются последовательно.
- **Параллельная инициализация**: Сервисы внутри одной группы инициализируются одновременно.
- **Поддержка отмены**: Инициализацию можно прервать с помощью `CancellationToken`.
- **Защита от дубликатов**: Предотвращает повторную инициализацию сервисов в группе.
- **Защита от повторного запуска**: Гарантирует однократное выполнение инициализатора.
- **Логирование**: Подробное логирование для отладки и мониторинга.

## Использование

### 1. Определение интерфейса сервиса
Сервисы должны реализовывать интерфейс `IOrderedInitializable`:

```csharp
public interface IOrderedInitializable
{
    UniTask InitializeWithOrderAsync();
    string Name { get; }
}
```

Пример реализации сервиса:

```csharp
public class LocalizationService : IOrderedInitializable
{
    public string Name => "LocalizationService";
    public async UniTask InitializeWithOrderAsync()
    {
        // Логика инициализации
        await UniTask.Delay(1000); // Имитация асинхронной работы
        Logger.LogInfo(this, "LocalizationService инициализирован.");
    }
}
```

### 2. Настройка порядка инициализации
Создайте актив `InitializationOrderSettings` в Unity для задания приоритетов сервисов:

1. Перейдите в `Assets > Create > Config > Initialization Order Settings`.
2. В инспекторе Unity добавьте записи:
   - **Ключ сервиса**: Уникальный идентификатор сервиса (например, `LocalizationService`).
   - **Порядок**: Целое число приоритета (меньшие значения инициализируются первыми).
   - **Порядок по умолчанию**: Запасной приоритет для неуказанных сервисов (по умолчанию: 999).

Пример конфигурации в `InitializationOrderSettings`:

```csharp
// Пример настроек в инспекторе Unity
Ключ сервиса: LocalizationService, Порядок: 10
Ключ сервиса: SaveLoadService, Порядок: 10
Ключ сервиса: SceneLoader, Порядок: 20
```

### 3. Создание инициализатора
Создайте пользовательский инициализатор, наследующий `OrderedInitializerBase<TService>`. Пример реализации `ProjectServiceInitializer`:

```csharp
using System.Collections.Generic;
using Common.Services.Initialization.Ordered;

namespace _Farm.Core.Initialization
{
    public class ProjectServiceInitializer : OrderedInitializerBase<IOrderedInitializable>
    {
        public ProjectServiceInitializer(
            IEnumerable<IOrderedInitializable> orderedInitializables,
            InitializationOrderSettings orderSettings)
            : base(orderedInitializables, orderSettings)
        {
        }
    }
}
```

### 4. Регистрация в DI-контейнере
Настройте зависимости в `ProjectInstaller` для регистрации сервисов и инициализатора:

```csharp
using _Farm.Core.Initialization;
using Common.Services;
using Common.Services.Initialization;
using Common.Services.Initialization.Ordered;
using Common.Services.Localization;
using Common.Services.SaveLoadService;
using Common.Services.SceneLoader;
using Reflex.Core;

namespace _Farm.Core.Initialization.DI
{
    public class ProjectInstaller : FarmMono, IInstaller
    {
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            // Регистрация сервисов
            containerBuilder.AddSingleton(typeof(LocalizationService), typeof(ILocalizationService), typeof(IOrderedInitializable));
            containerBuilder.AddSingleton(container => container.Resolve<LocalizationService>(), typeof(IService));
            
            containerBuilder.AddSingleton(typeof(SaveLoadService), typeof(ISaveLoadService), typeof(IOrderedInitializable));
            containerBuilder.AddSingleton(container => container.Resolve<ISaveLoadService>(), typeof(IService));
            
            containerBuilder.AddSingleton(typeof(SceneLoader), typeof(ISceneLoader));
            containerBuilder.AddSingleton(container => container.Resolve<ISceneLoader>(), typeof(IService));
            
            // Регистрация инициализатора
            containerBuilder.AddSingleton(typeof(ProjectServiceInitializer), typeof(IInitializer));
        }
    }
}
```

### 5. Вызов инициализации
Используйте инициализатор в состоянии игры, например, в `InitializationState`:

```csharp
using System.Threading;
using Common.Services.GameState.States;
using Common.Services.Initialization;
using Common.Services.SceneLoader;
using Common.Utils.Logging;
using Cysharp.Threading.Tasks;

namespace _Farm.Core.States
{
    public class InitializationState : BaseGameState
    {
        private readonly IInitializer _projectServiceInitializer;
        private readonly ISceneLoader _sceneLoader;
        private readonly SceneLoadingConfig _sceneLoadingConfig;

        public InitializationState(
            IInitializer projectServiceInitializer,
            ISceneLoader sceneLoader,
            SceneLoadingConfig sceneLoadingConfig)
        {
            _projectServiceInitializer = projectServiceInitializer;
            _sceneLoader = sceneLoader;
            _sceneLoadingConfig = sceneLoadingConfig;
        }

        public override GameStateEnum StateEnum => GameStateEnum.Initializing;

        public override async UniTask OnEnter(CancellationToken token, BaseStateParam stateParam = null)
        {
            await base.OnEnter(token);
#if UNITY_EDITOR
            await _sceneLoader.LoadSceneAsync(_sceneLoadingConfig.BootSceneName, token);
#endif
            Logger.LogDevelopment(this, "Начало инициализации упорядоченных сервисов");
            await _projectServiceInitializer.InitializeAllAsync(token);
            await _sceneLoader.LoadSceneAsync(_sceneLoadingConfig.StartSceneName, token);
        }
    }
}
```

### 6. Поддержка отмены
Передайте `CancellationToken` для прерывания инициализации:

```csharp
var cts = new CancellationTokenSource();
await _projectServiceInitializer.InitializeAllAsync(cts.Token);
```

Прерывание операции:

```csharp
cts.Cancel();
```

## Важные замечания
- **Дублирующиеся ключи**: Система логирует предупреждения о дублирующихся ключах в `InitializationOrderSettings`. Используйте уникальные ключи.
- **Группировка по порядку**: Сервисы с одинаковым приоритетом инициализируются параллельно.
- **Обработка ошибок**: Исключения при инициализации сервиса логируются и пробрасываются, останавливая процесс.
- **Unity Editor**: Дублирующиеся ключи обнаруживаются при валидации в редакторе Unity.
- **Производительность**: Сервисы материализуются один раз при инициализации для оптимизации.

## Пример рабочего процесса
1. Создайте актив `InitializationOrderSettings` с приоритетами сервисов.
2. Реализуйте сервисы с интерфейсом `IOrderedInitializable`.
3. Настройте DI-контейнер в `ProjectInstaller` для регистрации сервисов и инициализатора.
4. Используйте `ProjectServiceInitializer` в `InitializationState` для запуска упорядоченной инициализации.