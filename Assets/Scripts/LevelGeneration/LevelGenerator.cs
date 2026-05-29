using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour {
    [Header("Настройки уровня")]
    public int targetRoomCount = 10;
    public int floorIndex = 0;
    public int seed = 0;
    public bool useRandomSeed = true;

    [Header("Комнаты")]
    public RoomData startRoomData;
    public RoomData bossRoomData;
    public List<RoomData> combatRooms;
    public List<RoomData> specialRooms;

    [Header("Размер комнат (расстояние между центрами)")]
    public Vector2 roomSpacing = new Vector2(26f, 26f); // Стандартное расстояние между центрами комнат

    [Header("Коридоры")]
    public GameObject corridorHorizontalPrefab;
    public GameObject corridorVerticalPrefab;

    [Header("Заглушки")]
    public GameObject doorBlockerPrefab;

    private Dictionary<Vector2Int, RoomInstance> _grid = new();
    private List<RoomInstance> _allRooms = new();

    private System.Random _rng;

    void Start() {
        Generate();
    }

    [ContextMenu("Generate")]
    public void Generate() {
        Clear();

        int actualSeed = useRandomSeed ? Random.Range(0, int.MaxValue) : seed;
        _rng = new System.Random(actualSeed);

        Debug.Log($"[Generator] Seed: {actualSeed}");

        SpawnRoom(startRoomData, Vector2Int.zero);

        Queue<RoomInstance> frontier = new();
        frontier.Enqueue(_allRooms[0]);

        int attempts = 0;

        while (_allRooms.Count < targetRoomCount - 1 && frontier.Count > 0 && attempts < 500) {
            attempts++;
            RoomInstance current = frontier.Dequeue();

            foreach (DoorPoint door in current.GetFreeDoors().ToList()) {
                if (_allRooms.Count >= targetRoomCount - 1)
                    break;

                Vector2Int nextPos = current.gridPosition + DirToGrid(door.direction);

                if (_grid.ContainsKey(nextPos))
                    continue;

                RoomData candidate = PickRoom(door.direction);

                if (candidate == null) {
                    Debug.LogWarning($"Нет комнаты для направления {door.direction}");
                    continue;
                }

                RoomInstance newRoom = SpawnRoom(candidate, nextPos);

                if (newRoom == null)
                    continue;

                DoorPoint otherDoor = newRoom.GetDoor(door.Opposite());

                if (otherDoor == null) {
                    Debug.LogWarning($"У новой комнаты нет двери {door.Opposite()}");
                    continue;
                }

                ConnectDoors(door, otherDoor);
                frontier.Enqueue(newRoom);
            }
        }

        SpawnBossRoom();
        BlockFreeDoors();

        Debug.Log($"[Generator] Сгенерировано {_allRooms.Count} комнат");
    }

    // =========================================================
    // РАСЧЁТ ПОЗИЦИИ КОМНАТЫ С УЧЁТОМ ЕЁ РАЗМЕРА
    // =========================================================
    Vector3 CalculateRoomPosition(RoomData data, Vector2Int gridPos) {
        Vector2 roomSize = GetRoomSize(data);
        Vector2 standardSize = new Vector2(24f, 24f); // стандартный размер комнаты в тайлах

        // Разница между размером этой комнаты и стандартной
        Vector2 sizeDifference = (roomSize - standardSize) / 2f;

        // Базовая позиция по сетке
        float baseX = gridPos.x * roomSpacing.x;
        float baseY = gridPos.y * roomSpacing.y;

        // Корректируем позицию с учётом размера комнаты
        float offsetX = baseX + (gridPos.x > 0 ? sizeDifference.x : gridPos.x < 0 ? -sizeDifference.x : 0);
        float offsetY = baseY + (gridPos.y > 0 ? sizeDifference.y : gridPos.y < 0 ? -sizeDifference.y : 0);

        return new Vector3(offsetX, offsetY, 0f);
    }

    Vector2 GetRoomSize(RoomData data) {
        if (data != null && data.roomSize != Vector2.zero) {
            return data.roomSize;
        }
        return new Vector2(24f, 24f);
    }

    // =========================================================

    RoomInstance SpawnRoom(RoomData data, Vector2Int gridPos) {
        if (data == null || data.prefab == null)
            return null;

        Vector3 worldPos = CalculateRoomPosition(data, gridPos);

        GameObject go = Instantiate(data.prefab, worldPos, Quaternion.identity, transform);
        go.name = $"Room_{data.roomType}_{gridPos}";

        RoomInstance instance = go.GetComponent<RoomInstance>();
        if (instance == null)
            instance = go.AddComponent<RoomInstance>();

        instance.data = data;
        instance.gridPosition = gridPos;

        _grid.Add(gridPos, instance);
        _allRooms.Add(instance);

        Debug.Log($"Spawned {data.roomType} at {gridPos} -> world {worldPos}");

        return instance;
    }

    // =========================================================

    RoomData PickRoom(DoorDirection incomingDirection) {
        DoorDirection neededDoor = OppositeDir(incomingDirection);

        bool wantSpecial = _rng.NextDouble() < 0.25 && specialRooms.Count > 0;

        List<RoomData> source = wantSpecial ? specialRooms : combatRooms;
        List<RoomData> pool = new();

        foreach (RoomData rd in source) {
            if (rd == null || rd.prefab == null)
                continue;

            if (rd.minFloor > floorIndex)
                continue;

            DoorPoint[] doors = rd.prefab.GetComponentsInChildren<DoorPoint>(true);
            bool hasNeededDoor = doors.Any(d => d.direction == neededDoor);

            if (hasNeededDoor)
                pool.Add(rd);
        }

        Debug.Log($"Need: {neededDoor} | Pool: {pool.Count}");

        if (pool.Count == 0)
            return null;

        return WeightedRandom(pool);
    }

    // =========================================================

    RoomData WeightedRandom(List<RoomData> pool) {
        float total = pool.Sum(r => r.spawnWeight);
        float roll = (float)(_rng.NextDouble() * total);

        foreach (RoomData room in pool) {
            roll -= room.spawnWeight;
            if (roll <= 0f)
                return room;
        }

        return pool[^1];
    }

    // =========================================================

    void ConnectDoors(DoorPoint a, DoorPoint b) {
        a.isConnected = true;
        b.isConnected = true;

        a.connectedTo = b;
        b.connectedTo = a;

        SpawnCorridor(a, b);
        OpenDoor(a);
        OpenDoor(b);
    }

    // =========================================================

    void SpawnCorridor(DoorPoint a, DoorPoint b) {
        bool horizontal = a.direction == DoorDirection.Left || a.direction == DoorDirection.Right;

        GameObject prefab = horizontal ? corridorHorizontalPrefab : corridorVerticalPrefab;

        if (prefab == null)
            return;

        Vector3 midpoint = (a.transform.position + b.transform.position) / 2f;
        GameObject corridor = Instantiate(prefab, midpoint, Quaternion.identity, transform);
        corridor.name = $"Corridor_{a.direction}";
    }

    // =========================================================

    void OpenDoor(DoorPoint door) {
        Transform roomRoot = door.transform.parent;
        Transform passageBlockers = roomRoot.Find("PassageBlockers");

        if (passageBlockers == null) {
            Debug.LogWarning("Нет PassageBlockers в комнате");
            return;
        }

        string passageName = door.direction switch {
            DoorDirection.Up => "Passage_Up",
            DoorDirection.Down => "Passage_Down",
            DoorDirection.Left => "Passage_Left",
            DoorDirection.Right => "Passage_Right",
            _ => ""
        };

        Transform passage = passageBlockers.Find(passageName);
        if (passage != null) {
            passage.gameObject.SetActive(false);
            Debug.Log($"Открыт проход: {passageName}");
        }
    }

    // =========================================================

    void SpawnBossRoom() {
        RoomInstance target = _allRooms
            .Where(r =>
                r.data.roomType != RoomType.Start &&
                r.data.roomType != RoomType.Boss &&
                r.GetFreeDoors().Count > 0
            )
            .OrderByDescending(r => r.gridPosition.sqrMagnitude)
            .FirstOrDefault();

        if (target == null) {
            Debug.LogWarning("No target room for boss!");
            return;
        }

        DoorPoint freeDoor = target.GetFreeDoors().FirstOrDefault();
        if (freeDoor == null) return;

        Vector2Int bossPos = target.gridPosition + DirToGrid(freeDoor.direction);

        if (_grid.ContainsKey(bossPos)) return;

        RoomInstance bossRoom = SpawnRoom(bossRoomData, bossPos);

        if (bossRoom == null) return;

        DoorPoint bossDoor = bossRoom.GetDoor(freeDoor.Opposite());
        if (bossDoor == null) return;

        ConnectDoors(freeDoor, bossDoor);
    }

    // =========================================================

    void BlockFreeDoors() {
        if (doorBlockerPrefab == null)
            return;

        foreach (RoomInstance room in _allRooms) {
            foreach (DoorPoint door in room.GetFreeDoors()) {
                Instantiate(doorBlockerPrefab, door.transform.position, Quaternion.identity, room.transform);
                door.isConnected = true;
            }
        }
    }

    // =========================================================

    void Clear() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        _grid.Clear();
        _allRooms.Clear();
    }

    // =========================================================

    static Vector2Int DirToGrid(DoorDirection dir) {
        return dir switch {
            DoorDirection.Up => Vector2Int.up,
            DoorDirection.Down => Vector2Int.down,
            DoorDirection.Left => Vector2Int.left,
            DoorDirection.Right => Vector2Int.right,
            _ => Vector2Int.zero
        };
    }

    static DoorDirection OppositeDir(DoorDirection dir) {
        return dir switch {
            DoorDirection.Up => DoorDirection.Down,
            DoorDirection.Down => DoorDirection.Up,
            DoorDirection.Left => DoorDirection.Right,
            DoorDirection.Right => DoorDirection.Left,
            _ => DoorDirection.Up
        };
    }
}