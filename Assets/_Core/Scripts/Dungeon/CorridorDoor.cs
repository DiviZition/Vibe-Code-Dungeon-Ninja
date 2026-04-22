using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dungeon
{
    public enum DoorState { Closed, Open }

    public class CorridorDoor : MonoBehaviour
    {
        [SerializeField] private int _corridorIndex;
        [SerializeField] private Tilemap _wallTilemap;
        [SerializeField] private Vector3Int[] _doorPositions;

        public DoorState State { get; private set; } = DoorState.Closed;
        public int CorridorIndex => _corridorIndex;

        public void Initialize(int corridorIndex, Tilemap wallTilemap, Vector3Int[] doorPositions)
        {
            _corridorIndex = corridorIndex;
            _wallTilemap = wallTilemap;
            _doorPositions = doorPositions;
            State = DoorState.Closed;
        }

        public void Open()
        {
            if (State == DoorState.Open)
                return;

            if (_wallTilemap != null && _doorPositions != null)
            {
                foreach (var pos in _doorPositions)
                {
                    _wallTilemap.SetTile(pos, null);
                }
            }

            State = DoorState.Open;
        }
    }
}