using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[System.Serializable]
public class MobData {
    public GameObject mobPrefab;
    public string mobName;
    public int spawnWeight = 10;
    public bool isBoss = false;
}

[System.Serializable]
public class RoomSpawnConfig {
    public RoomType roomType;
    public int minMobs = 1;
    public int maxMobs = 5;
    public float spawnChance = 1f;
}

public class MobSpawner : MonoBehaviour {
    [Header("Настройки")]
    public LevelGenerator levelGenerator;
    public List<MobData> allMobs = new List<MobData>();
    public List<RoomSpawnConfig> roomConfigs = new List<RoomSpawnConfig>();

    [Header("Количество мобов")]
    public int defaultMinMobs = 1;
    public int defaultMaxMobs = 5;
    public float defaultSpawnChance = 0.8f;

    [Header("Размер пола (в Unity единицах)")]
    public float floorWidth = 14f;
    public float floorHeight = 18f;

    [Header("Отступ от стен")]
    public float wallOffset = 1.5f;

    [Header("Босс")]
    public MobData bossMob;

    [Header("Отладка")]
    public bool showDebugLogs = true;

    private System.Random _rng;
    private Transform _player;
    private Dictionary<RoomInstance, List<GameObject>> _spawnedMobs = new();
    private HashSet<RoomInstance> _triggeredRooms = new();
    private bool _navMeshReady = false;

    void Start() {
        if (levelGenerator == null) {
            levelGenerator = FindAnyObjectByType<LevelGenerator>();
        }

        _rng = new System.Random();
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Проверяем NavMesh каждые 0.5 секунды
        InvokeRepeating(nameof(CheckNavMeshReady), 0.5f, 0.5f);

        // Добавляем триггеры сразу
        Invoke(nameof(AddRoomTriggers), 0.1f);
    }

    void CheckNavMeshReady() {
        if (NavMesh.SamplePosition(Vector3.zero, out _, 100f, NavMesh.AllAreas)) {
            if (!_navMeshReady) {
                _navMeshReady = true;
                if (showDebugLogs) Debug.Log("NavMesh is now ready!");
            }
        }
    }

    void AddRoomTriggers() {
        var rooms = GetAllRooms();
        if (rooms == null || rooms.Count == 0) {
            Invoke(nameof(AddRoomTriggers), 0.2f);
            return;
        }

        foreach (var room in rooms) {
            if (room == null) continue;
            if (room.data.roomType == RoomType.Start) continue;
            if (room.GetComponentInChildren<RoomTrigger>() != null) continue;

            AddRoomTrigger(room);
        }

        if (showDebugLogs) Debug.Log($"Added triggers to {_triggeredRooms.Count} rooms");
    }

    void AddRoomTrigger(RoomInstance room) {
        GameObject triggerObj = new GameObject("RoomTrigger");
        triggerObj.transform.SetParent(room.transform);
        triggerObj.transform.localPosition = Vector3.zero;

        BoxCollider2D collider = triggerObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(floorWidth, floorHeight);

        var trigger = triggerObj.AddComponent<RoomTrigger>();
        trigger.Initialize(room, this);
    }

    public void OnPlayerEnteredRoom(RoomInstance room) {
        if (_triggeredRooms.Contains(room)) return;
        _triggeredRooms.Add(room);

        if (showDebugLogs) Debug.Log($"Player entered {room.data.roomType} room");

        // Спавним мобов БЕЗ задержки
        SpawnMobsInRoom(room);
    }

    void SpawnMobsInRoom(RoomInstance room) {
        if (room.data.roomType == RoomType.Boss) {
            SpawnBoss(room);
            return;
        }

        if (room.data.roomType == RoomType.Start) return;

        RoomSpawnConfig config = roomConfigs.Find(c => c.roomType == room.data.roomType);
        int mobCount;
        float chance;

        if (config != null) {
            mobCount = Random.Range(config.minMobs, config.maxMobs + 1);
            chance = config.spawnChance;
        }
        else {
            mobCount = Random.Range(defaultMinMobs, defaultMaxMobs + 1);
            chance = defaultSpawnChance;
        }

        if (Random.value > chance || mobCount <= 0) return;

        List<MobData> availableMobs = allMobs.Where(m => !m.isBoss).ToList();
        if (availableMobs.Count == 0) return;

        List<Vector3> spawnPoints = GetValidSpawnPointsInRoom(room, mobCount);

        foreach (Vector3 pos in spawnPoints) {
            MobData mob = WeightedRandom(availableMobs);
            if (mob?.mobPrefab == null) continue;

            GameObject mobObj = Instantiate(mob.mobPrefab, pos, Quaternion.identity, room.transform);

            // Настройка NavMeshAgent
            var agent = mobObj.GetComponent<NavMeshAgent>();
            if (agent != null) {
                agent.updateRotation = false;
                agent.updateUpAxis = false;
                agent.Warp(pos);
                if (!agent.isOnNavMesh) {
                    if (showDebugLogs) Debug.LogWarning($"Mob {mob.mobName} not on NavMesh at {pos}");
                }
            }

            if (!_spawnedMobs.ContainsKey(room)) _spawnedMobs[room] = new List<GameObject>();
            _spawnedMobs[room].Add(mobObj);

            if (showDebugLogs) Debug.Log($"Spawned {mob.mobName} at {pos}");
        }
    }

    void SpawnBoss(RoomInstance room) {
        if (bossMob?.mobPrefab == null) return;

        Vector3 spawnPos = room.transform.position;
        GameObject boss = Instantiate(bossMob.mobPrefab, spawnPos, Quaternion.identity, room.transform);

        var agent = boss.GetComponent<NavMeshAgent>();
        if (agent != null) {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.Warp(spawnPos);
        }

        _spawnedMobs[room] = new List<GameObject> { boss };
        if (showDebugLogs) Debug.Log($"Spawned boss at {spawnPos}");
    }

    List<Vector3> GetValidSpawnPointsInRoom(RoomInstance room, int count) {
        List<Vector3> points = new List<Vector3>();
        Vector3 roomPos = room.transform.position;

        float halfWidth = floorWidth / 2f;
        float halfHeight = floorHeight / 2f;

        // Границы спавна (внутри комнаты, не в стенах)
        float left = roomPos.x - halfWidth + wallOffset;
        float right = roomPos.x + halfWidth - wallOffset;
        float bottom = roomPos.y - halfHeight + wallOffset;
        float top = roomPos.y + halfHeight - wallOffset;

        // Углы
        Vector3[] corners = {
            new Vector3(left, top, 0),
            new Vector3(right, top, 0),
            new Vector3(left, bottom, 0),
            new Vector3(right, bottom, 0)
        };

        points.AddRange(corners);

        // Случайные точки
        for (int i = 0; i < count; i++) {
            float x = Random.Range(left, right);
            float y = Random.Range(bottom, top);
            points.Add(new Vector3(x, y, 0));
        }

        return points.Distinct().Take(count).ToList();
    }

    MobData WeightedRandom(List<MobData> mobs) {
        int total = mobs.Sum(m => m.spawnWeight);
        int roll = _rng.Next(total);

        foreach (MobData mob in mobs) {
            roll -= mob.spawnWeight;
            if (roll < 0) return mob;
        }
        return mobs.FirstOrDefault();
    }

    List<RoomInstance> GetAllRooms() {
        var field = levelGenerator?.GetType().GetField("_allRooms",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var rooms = field?.GetValue(levelGenerator) as System.Collections.IList;

        List<RoomInstance> result = new List<RoomInstance>();
        if (rooms != null) {
            foreach (var room in rooms) {
                result.Add(room as RoomInstance);
            }
        }
        return result;
    }
}