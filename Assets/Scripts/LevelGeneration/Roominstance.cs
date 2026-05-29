using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Прикрепляется автоматически к каждому заспауненному prefab'у комнаты.
/// Хранит runtime-состояние: какие двери открыты, очищена ли комната.
/// </summary>
public class RoomInstance : MonoBehaviour {
    public RoomData data;
    public Vector2Int gridPosition;

    public List<DoorPoint> doors = new();
    public bool isCleared = false;
    public bool isVisited = false;

    public Vector2 RoomSize {
        get {
            if (data != null && data.roomSize != Vector2.zero)
                return data.roomSize;

            // Автоматическое определение из Tilemap
            var tilemap = GetComponentInChildren<Tilemap>();
            if (tilemap != null) {
                return tilemap.localBounds.size;
            }

            return new Vector2(24f, 24f); // Значение по умолчанию
        }
    }

    void Awake() {
        doors.AddRange(GetComponentsInChildren<DoorPoint>());
    }

    public List<DoorPoint> GetFreeDoors() {
        return doors.FindAll(d => !d.isConnected);
    }

    public DoorPoint GetDoor(DoorDirection dir) {
        return doors.Find(d => d.direction == dir);
    }

    /// <summary>
    /// Вызывается когда игрок входит в комнату.
    /// Переопредели в подклассе или подпишись на событие.
    /// </summary>
    public System.Action<RoomInstance> OnRoomEntered;
    public System.Action<RoomInstance> OnRoomCleared;

    public void Enter() {
        isVisited = true;
        OnRoomEntered?.Invoke(this);
    }

    public void Clear() {
        isCleared = true;
        OnRoomCleared?.Invoke(this);
    }


}