# Roguelite Satyr

> A 2D roguelite action game developed in Unity as a bachelor's thesis project.

---

## Overview

**Roguelite Satyr** is a 2D top-down roguelite game built with Unity. The player controls a Satyr warrior navigating through procedurally generated dungeons, fighting enemies with melee and ranged weapons, and progressing toward a final boss encounter. Each run is unique thanks to the random level generation system.

This project was developed as a **bachelor's diploma thesis** with a focus on game architecture, procedural generation, and AI-driven enemy behavior.

---

## Gameplay Features

- **Procedural Level Generation** — Every playthrough generates a unique dungeon layout with interconnected rooms, corridors, and a boss room
- **Combat System** — Melee (Sword) and ranged (Bow) weapon mechanics with hold/release attack support
- **Enemy AI** — Enemies roam, detect the player, chase, and attack using state-based AI logic
- **Dash Mechanic** — Player can dash to dodge enemy attacks
- **Knockback System** — Both the player and enemies experience knockback on taking damage
- **Health System** — Player health with HUD, hit flash effects, and death handling
- **Room-based Mob Spawning** — Enemies spawn when the player enters a room for the first time
- **Boss Room** — A dedicated final boss encounter room generated at the end of the dungeon
- **Audio & Graphics Settings** — Volume controls (Master, Music, SFX), fullscreen toggle, settings persistence via `PlayerPrefs`
- **Main Menu & Pause Menu** — Full UI flow including main menu, options, pause, and game over screens

---

## Tech Stack

| Technology | Details |
|---|---|
| **Engine** | Unity (2D) |
| **Language** | C# |
| **Rendering** | ShaderLab / HLSL (custom shaders) |
| **Input** | Unity Input System (`PlayerInputActions`) |
| **Navigation** | NavMesh for AI pathfinding |
| **Architecture** | Singleton pattern, ScriptableObjects, Event-driven design |

---

## Architecture Overview

The project follows a component-based architecture with clear separation of concerns:

### Core Systems

| Class | Responsibility |
|---|---|
| `GameManager` | Global game state machine (`Playing`, `Paused`, `GameOver`, `MainMenu`) |
| `GameInput` | Input abstraction layer supporting both keyboard/mouse and gamepad |
| `Player` | Player controller — movement, dash, damage, death |
| `ActiveWeapon` | Manages the currently equipped weapon and attack direction |

### Combat

| Class | Responsibility |
|---|---|
| `Weapon` *(abstract)* | Base class for all weapons |
| `Sword` | Melee weapon with trigger-based hit detection |
| `Bow` | Ranged weapon with draw/release mechanics |
| `KnockBack` | Applies and resolves knockback impulse on entities |

### Enemy System

| Class | Responsibility |
|---|---|
| `EnemyEntity` | Enemy health, damage events, and death |
| `EnemyAI` | State-based AI: Idle → Roaming → Chasing → Attacking → Death |
| `EnemySO` | ScriptableObject data container for enemy parameters |

### Level Generation

| Class | Responsibility |
|---|---|
| `LevelGenerator` | Procedural dungeon builder — places rooms, connects doors, spawns boss room |
| `RoomInstance` | Runtime room state (cleared, visited, doors) |
| `DoorPoint` | Door markers with direction and connection references |
| `MobSpawner` | Spawns enemies in rooms when the player enters |
| `RoomTrigger` | Trigger collider that notifies `MobSpawner` on player entry |

### UI

| Class | Responsibility |
|---|---|
| `MainMenu` | Entry screen with play, options, and quit actions |
| `OptionsMenu` | Audio and display settings with persistence |
| `HealthBar` | Player health HUD |

---

## UML Diagram

A full class diagram is available in [`UML_GameProgram.md`](./UML_GameProgram.md).

Key relationships:
- `GameInput` fires events → `Player` reacts → `ActiveWeapon` delegates to `Weapon`
- `Sword` detects collision → `EnemyEntity.TakeDamage()` → `KnockBack` + death events
- `LevelGenerator` builds the map → `MobSpawner` reads rooms → `RoomTrigger` fires on player entry
- `Player` notifies `GameManager` on death → Game Over state

---

## Controls

| Action | Keyboard / Mouse | Gamepad |
|---|---|---|
| Move | `WASD` | Left Stick |
| Aim | Mouse cursor | Right Stick |
| Attack | Left Mouse Button | Right Trigger |
| Dash | `Space` | South Button (A/X) |
| Pause | `Escape` | Start |

---

## Project Structure

```
Roguelite-Satyr/
├── Assets/
│   ├── Scripts/         # All C# game logic
│   ├── Scenes/          # Unity scenes
│   ├── Prefabs/         # Reusable game objects
│   ├── Art/             # Sprites and visual assets
│   ├── Audio/           # Sound effects and music
│   └── ScriptableObjects/ # Enemy and item data (SO)
├── Packages/            # Unity package dependencies
├── ProjectSettings/     # Unity project configuration
├── UML_GameProgram.md   # Full UML class diagram
└── README.md
```

---

## Author

**Arkan-AAA**
Bachelor's Thesis Project — 2D Game Development in Unity

---

## License

This project was created for academic purposes as a diploma thesis. All rights reserved by the author.
