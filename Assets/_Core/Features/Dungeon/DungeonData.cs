using UnityEngine;
using System.Collections.Generic;
using Enemy;
using R3;
using System;

namespace Dungeon
{
    public enum RoomType
    {
        Normal,
        Boss,
    }

    public class RoomData : IDisposable
    {
        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public RoomType Type { get; private set; }

        public List<CorridorData> ConnectedCorridors { get; private set; }
        public List<EnemyBase> EnemiesInside { get; private set; }

        public Subject<RoomData> OnAllEnemiesCleared { get; private set; }
        public Subject<RoomData> OnNewEnemiesAppeared { get; private set; }

        public RoomData(Vector2Int grid, int width, int height, RoomType roomType = RoomType.Normal)
        {
            OnAllEnemiesCleared = new Subject<RoomData>();
            OnNewEnemiesAppeared = new Subject<RoomData>();

            ConnectedCorridors = new List<CorridorData>(2);
            EnemiesInside = new List<EnemyBase>(4);

            GridX = grid.x;
            GridY = grid.y;
            Width = width;
            Height = height;

            Type = roomType;
        }

        public void SetRoomType(RoomType newRoomType) => Type = newRoomType;

        public void AddEnemy(EnemyBase enemyToAdd)
        {
            if (EnemiesInside.Count <= 0)
                OnNewEnemiesAppeared.OnNext(this);

            EnemiesInside.Add(enemyToAdd);
        }

        public void RemoveEnemy(EnemyBase enemyToRemove)
        {
            if (EnemiesInside.Contains(enemyToRemove) == true)
                EnemiesInside.Remove(enemyToRemove);

            if (EnemiesInside.Count <= 0)
                OnAllEnemiesCleared.OnNext(this);
        }

        public void Dispose()
        {
            OnAllEnemiesCleared?.Dispose();
            OnNewEnemiesAppeared?.Dispose();
        }
    }

    public class CorridorData : IDisposable
    {
        public Vector2 PointFrom { get; private set; }
        public Vector2 PointTo { get; private set; }

        public int Width { get; private set; }
        public int Length { get; private set; }

        public int FromRoomIndex { get; private set; }
        public int ToRoomIndex { get; private set; }

        public bool IsOpened { get; private set; }

        public List<Vector3Int> DoorPositions { get; private set; }
        public Subject<CorridorData> OnOpened {  get; private set; }
        public Subject<CorridorData> OnClosed {  get; private set; }

        public CorridorData(Vector2 pointFrom, Vector2 pointTo, int width, int length, int fromRoomIndex, int toRoomIndex)
        {
            DoorPositions = new List<Vector3Int>();
            OnOpened = new Subject<CorridorData>();
            OnClosed = new Subject<CorridorData>();

            PointFrom = pointFrom;
            PointTo = pointTo;
            Width = width;
            Length = length;
            FromRoomIndex = fromRoomIndex;
            ToRoomIndex = toRoomIndex;
        }

        public void SetIsOpened(bool isOpened)
        {
            IsOpened = isOpened;

            if (isOpened)
                OnOpened.OnNext(this);
            else 
                OnClosed.OnNext(this);
        }

        public void Dispose()
        {
            OnOpened?.Dispose();
            OnClosed?.Dispose();
        }
    }

    public class DungeonData : IDisposable
    {
        public List<RoomData> Rooms { get; private set; }
        public List<CorridorData> Corridors { get; private set; }

        public int ZoneSize { get; private set; }
        public int ZonesCount { get; private set; }

        public int Seed { get; private set; }

        private IDisposable _roomsOpenCloseEvents;

        public DungeonData(List<RoomData> rooms, List<CorridorData> corridors, int zoneSize, int zoneCount, int seed)
        {
            ZoneSize = zoneSize;
            ZonesCount = zoneCount;
            Seed = seed;

            ReassignRoomsAndCorridors(rooms, corridors);
        }

        public void ReassignRoomsAndCorridors(List<RoomData> rooms, List<CorridorData> corridors)
        {
            Rooms = rooms;
            Corridors = corridors;

            foreach (RoomData room in Rooms)
            {
                var openEvent = room.OnAllEnemiesCleared.Subscribe(OpenRoomCorridors);
                var closeEvent = room.OnNewEnemiesAppeared.Subscribe(CloseRoomCorridors);
                _roomsOpenCloseEvents = Disposable.Combine(openEvent, closeEvent);
            }
        }

        public void CloseRoomCorridors(int roomIndex) => CloseRoomCorridors(Rooms[roomIndex]);
        public void CloseRoomCorridors(RoomData roomData) => SetRoomIsOpened(roomData, newIsOpened: false);
        public void OpenRoomCorridors(int roomIndex) => OpenRoomCorridors(Rooms[roomIndex]);
        public void OpenRoomCorridors(RoomData roomData) => SetRoomIsOpened(roomData, newIsOpened: true);

        private void SetRoomIsOpened(RoomData room, bool newIsOpened)
        {
            foreach (CorridorData corridor in room.ConnectedCorridors)
            {
                corridor.SetIsOpened(newIsOpened);
            }
        }

        public void Dispose()
        {
            _roomsOpenCloseEvents?.Dispose();

            foreach (RoomData room in Rooms)
                room.Dispose();
            
            foreach (CorridorData corridor in Corridors)
                corridor.Dispose();
        }
    }
}
