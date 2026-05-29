# UML-диаграмма проекта RogueLite

## 1. Общая схема связей

```mermaid
classDiagram
    direction TB

    class GameManager {
        +CurrentState: GameState
        +IsPaused: bool
        +IsGameOver: bool
        +OnStateChanged: Action<GameState>
        +Awake()
        +Start()
        +Update()
        +SetState(newState)
        +TogglePause()
        +PauseGame()
        +ResumeGame()
        +GameOver()
        +RestartLevel()
        +LoadMainMenu()
        +QuitGame()
        -ApplyState()
        -SetPanel(panel, visible)
    }

    class GameInput {
        +Instance: GameInput
        +OnPlayerAttack: EventHandler
        +OnPlayerAttackHeld: Action
        +OnPlayerAttackReleased: Action
        +OnPlayerDash: Action
        +Awake()
        +PlayerAttack_started(ctx)
        +GetMovementVector()
        +GetMousePosition()
        +GetGamepadLookVector()
        +IsGamepadActive()
        +GetGamepadMoveVector()
        +IsGamepadMoving()
        +DisableInput()
        +EnableInput()
    }

    class Player {
        +Instance: Player
        +MaxHealth: int
        +OnFlashBlink: event
        +OnHealthChanged: event
        +Awake()
        +Start()
        +FixedUpdate()
        +TakeDamage(damageSource, damage)
        -DetectDeath()
        -DestroyAfterDeath()
        +OnDestroy()
        -DamageCooldown()
        -HandleMovement()
        -Dash()
        +IsRunning()
        +GetPlayerScreenPosition()
    }

    class ActiveWeapon {
        +Instance: ActiveWeapon
        +GetActiveWeapon()
        +Attack()
        +AttackHeld()
        +AttackReleased()
        -Update()
        -FollowLookDirection()
    }

    abstract class Weapon {
        <<abstract>>
        +Attack()
        +AttackHeld()
        +AttackReleased()
    }

    class Sword {
        +OnSwordSwing: EventHandler
        +Awake()
        +Attack()
        +OnTriggerEnter2D(collision)
        +AttackColiderTurnOff()
    }

    class Bow {
        +OnBowDraw: EventHandler
        +OnBowRelease: EventHandler
        +Attack()
        +AttackHeld()
        +AttackReleased()
    }

    class EnemyEntity {
        +CurrentHealth: int
        +OnHit: event
        +OnDeath: event
        +Start()
        +TakeDamage(damageSource, damage)
    }

    class EnemyAI {
        +OnFlashBlink: event
        +Awake()
        +Start()
        +Update()
        +OnHit()
        +OnDestroy()
        +OnDeath()
        +DealDamage()
        +DestroyEnemy()
        -Roaming()
        -GetRoamingPosition()
        -ChangeFacingDirection(sourcePosition, targetPosition)
    }

    class HealthBar {
        +Start()
        +OnDestroy()
        +UpdateHealthBar(current, max)
    }

    class MainMenu {
        +PlayGame()
        +OptionsMenu()
        +QuitGame()
        +PlayTransition()
    }

    class OptionsMenu {
        +Start()
        +SetMasterVolume(value)
        +SetMusicVolume(value)
        +SetSFXVolume(value)
        +SetMute(isMuted)
        +SetFullscreen(isFullscreen)
        +LoadSettings()
        +SaveSettings()
        +Back()
    }

    class LevelGenerator {
        +targetRoomCount
        +floorIndex
        +seed
        +useRandomSeed
        +startRoomData
        +bossRoomData
        +combatRooms
        +specialRooms
        +roomSpacing
        +corridorHorizontalPrefab
        +corridorVerticalPrefab
        +doorBlockerPrefab
        +navMeshSurface
        +bakeNavMeshOnGenerate
        +navMeshBakeDelay
        +Start()
        +Generate()
        +BakeNavMesh()
        +SpawnRoom(data, gridPos)
        +PickRoom(incomingDirection)
        +WeightedRandom(pool)
        +ConnectDoors(a, b)
        +SpawnCorridor(a, b)
        +OpenDoor(door)
        +SpawnBossRoom()
        +BlockFreeDoors()
        +Clear()
        +DirToGrid(dir)
        +OppositeDir(dir)
    }

    class RoomInstance {
        +data
        +gridPosition
        +doors
        +isCleared
        +isVisited
        +RoomSize
        +Awake()
        +GetFreeDoors()
        +GetDoor(dir)
        +Enter()
        +Clear()
    }

    class DoorPoint {
        +direction
        +isConnected
        +connectedTo
        +Opposite()
        +OnDrawGizmos()
    }

    class MobSpawner {
        +levelGenerator
        +allMobs
        +roomConfigs
        +defaultMinMobs
        +defaultMaxMobs
        +defaultSpawnChance
        +floorWidth
        +floorHeight
        +wallOffset
        +bossMob
        +showDebugLogs
        +Start()
        +CheckNavMeshReady()
        +AddRoomTriggers()
        +AddRoomTrigger(room)
        +OnPlayerEnteredRoom(room)
        +SpawnMobsInRoom(room)
        +SpawnBoss(room)
        +GetValidSpawnPointsInRoom(room, count)
        +WeightedRandom(mobs)
        +GetAllRooms()
    }

    class RoomTrigger {
        +Initialize(room, spawner)
        +OnTriggerEnter2D(other)
    }

    class KnockBack {
        +IsGettingKnockedBack
        +Awake()
        +Update()
        +GetKnockedBack(damageSource)
        +StopKnockBackMovement()
    }

    class PlayerVisual {
        +Start()
        +TriggerHit()
        +SetDead()
        +SetDashing(isDashing)
    }

    class FlashBlink {
        +Start()
        +Blink()
    }

    class LookDirectionHelper {
        +GetLookX()
    }

    class Utils {
        +GetRandomDir()
    }

    class EnemySO {
        +enemyName
        +enemyHealth
        +enemyDamageAmount
    }

    class IDamageable {
        <<interface>>
        +TakeDamage(damageSource, damage)
    }

    GameManager --> GameInput : управляет доступом
    GameManager --> Player : вызывает GameOver()

    GameInput --> Player : генерирует события атаки/дэш
    Player --> ActiveWeapon : вызывает Attack()/AttackHeld()/AttackReleased()
    Player --> KnockBack : получает импульс
    Player --> PlayerVisual : визуальные эффекты
    Player --> GameManager : вызывает GameOver()
    Player --> HealthBar : отправляет OnHealthChanged

    ActiveWeapon --> Weapon : хранит currentWeapon
    Weapon <|-- Sword
    Weapon <|-- Bow

    Sword --> EnemyEntity : наносит урон
    EnemyEntity --> KnockBack : получает отбрасывание
    EnemyEntity --> EnemySO : берет параметры
    EnemyAI --> EnemyEntity : подписывается на OnHit/OnDeath
    EnemyAI --> Player : ищет и атакует
    EnemyAI --> KnockBack : учитывает отбрасывание
    EnemyAI --> Utils : выбирает случайное направление

    LevelGenerator --> RoomInstance : спавнит и хранит
    LevelGenerator --> DoorPoint : соединяет двери
    RoomInstance --> DoorPoint : содержит двери
    DoorPoint --> DoorPoint : connectedTo

    MobSpawner --> LevelGenerator : берет список комнат
    MobSpawner --> RoomInstance : работает с комнатами
    RoomTrigger --> MobSpawner : вызывает OnPlayerEnteredRoom(room)
    RoomTrigger --> RoomInstance : хранит room

    MainMenu --> GameInput : DisableInput()
    MainMenu --> SceneManager : LoadScene()
    OptionsMenu --> PlayerPrefs : сохраняет настройки
    LookDirectionHelper --> GameInput : читает ввод
    LookDirectionHelper --> Player : вычисляет позицию курсора относительно игрока
```

## 2. Краткое описание ключевых классов

### Управление игрой
- **GameManager** — главный контроллер состояния игры (`Playing`, `Paused`, `GameOver`, `MainMenu`). Управляет временем, панелями UI и вводом.
- **GameInput** — обертка над `PlayerInputActions`, отдает события атаки, дэш, движение и позицию мыши/геймпада.

### Игрок и бой
- **Player** — главный персонаж: движение, дэш, получение урона, смерть, уведомление UI о здоровье.
- **ActiveWeapon** — текущая активная оружейная сущность, проксирует атаки в `Weapon` и следит за направлением взгляда.
- **Weapon** — абстрактный базовый класс для оружия.
- **Sword** — реализация ближнего боя, наносит урон по `EnemyEntity` через `OnTriggerEnter2D`.
- **Bow** — заготовка дальнего оружия, поддерживает `AttackHeld` / `AttackReleased`.

### Враги
- **EnemyEntity** — здоровье врага, события `OnHit`/`OnDeath`, отбрасывание.
- **EnemyAI** — поведение ИИ: idle/roaming/chasing/attacking/death, поиск игрока, атака, анимации.

### Генерация уровня
- **LevelGenerator** — генерирует карту комнат, соединяет двери, ставит босса и проставляет блокираторы.
- **RoomInstance** — runtime-состояние комнаты: `data`, `gridPosition`, дверь, `isCleared`, `isVisited`.
- **DoorPoint** — маркер входа/выхода комнаты, хранит направление и связь с другой дверью.
- **MobSpawner** — спавнит врагов в комнатах при входе игрока.
- **RoomTrigger** — триггер, который сообщает `MobSpawner`, что игрок вошёл в комнату.

### UI
- **MainMenu** — вступительное меню, переход на сцену уровня, fade-эффект.
- **OptionsMenu** — настройки аудио и графики.
- **HealthBar** — HUD здоровья игрока.

### Вспомогательные классы
- **KnockBack** — отбрасывание персонажа/врага.
- **LookDirectionHelper** — определяет направление взгляда с учетом мыши и геймпада.
- **Utils** — вспомогательные функции.
- **EnemySO** — ScriptableObject с параметрами врага.

## 3. Короткий список основных методов

### GameManager
- `SetState()`, `TogglePause()`, `ResumeGame()`, `PauseGame()`, `GameOver()`, `RestartLevel()`, `LoadMainMenu()`, `QuitGame()`

### Player
- `TakeDamage()`, `IsRunning()`, `GetPlayerScreenPosition()`, `HandleMovement()`, `Dash()`

### ActiveWeapon
- `GetActiveWeapon()`, `Attack()`, `AttackHeld()`, `AttackReleased()`

### EnemyAI
- `Update()`, `DealDamage()`, `DestroyEnemy()`, `OnDeath()`, `OnHit()`

### LevelGenerator
- `Generate()`, `SpawnRoom()`, `PickRoom()`, `ConnectDoors()`, `SpawnBossRoom()`, `BlockFreeDoors()`, `Clear()`

### MobSpawner
- `OnPlayerEnteredRoom()`, `SpawnMobsInRoom()`, `SpawnBoss()`, `GetValidSpawnPointsInRoom()`

## 4. Как использовать диаграмму

- Если хотите, я могу дальше превратить этот файл в **PNG/PDF**,
- или сделать **упрощённую версию только для ядра игры**,
- или отдельно нарисовать **UML для сцены `Player`, `EnemyAI`, `LevelGenerator`**.
