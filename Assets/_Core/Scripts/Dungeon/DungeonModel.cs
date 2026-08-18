using R3;

namespace Dungeon
{
    public class DungeonModel : IDungeonModel
    {
        private readonly DungeonGenerator _generator = new();
        private readonly DungeonGeneratorConfig _config;

        public DungeonData Data { get; private set; }
        public Subject<DungeonData> OnGenerated { get; } = new();

        public DungeonModel(DungeonGeneratorConfig config)
        {
            _config = config;
        }

        public void Generate()
        {
            Data?.Dispose();
            Data = _generator.Generate(_config);
            OnGenerated.OnNext(Data);
        }

        public void CloseRoom(int roomIndex) => Data?.CloseRoomCorridors(roomIndex);
        public void OpenRoom(int roomIndex) => Data?.OpenRoomCorridors(roomIndex);
        public void AddEnemyToRoom(int roomIndex, Enemy.IEnemy enemy) => Data?.AddEnemyToRoom(roomIndex, enemy);
        public void RemoveEnemyFromRoom(int roomIndex, Enemy.IEnemy enemy) => Data?.RemoveEnemyFromRoom(roomIndex, enemy);

        public void Dispose() => Data?.Dispose();
    }
}