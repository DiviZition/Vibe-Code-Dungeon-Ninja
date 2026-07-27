using System.Collections;
using System.Linq;
using Dungeon;
using Enemy;
using NUnit.Framework;
using Unity.Properties;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class DungeonGenerationTest
{
    [UnityTest]
    public IEnumerator DungeonDataGeneration_CorridorConnection_And_Oppening_Test()
    {
        DungeonGeneratorConfig config = new DungeonGeneratorConfig();
        DungeonGenerator dg = new DungeonGenerator();
        config.ZonesCount = 10;
        yield return null;

        var dungeonData = dg.Generate(config);
        var rooms = dungeonData.Rooms;
        var firstRoom = rooms[0];

        Assert.That(dungeonData, Is.Not.Null);
        Assert.That(rooms, Is.Not.Null);
        Assert.That(rooms.Count, Is.EqualTo(dg.ZonesCount));
        Assert.That(rooms.SelectMany(r => r.ConnectedCorridors), Is.All.Matches<CorridorData>(c => c.IsOpened));

        // Single room doors Close
        var enemy = new TestEnemy();
        firstRoom.AddEnemy(enemy);
        Assert.That(firstRoom.ConnectedCorridors, Is.All.Matches<CorridorData>(c => c.IsOpened == false));
        Assert.That(dungeonData.Corridors, Has.Exactly(firstRoom.ConnectedCorridors.Count).Matches<CorridorData>(c => !c.IsOpened));

        // All doors Close test
        for (int i = 1; i < rooms.Count; i++)
            rooms[i].AddEnemy(enemy);

        Assert.That(dungeonData.Corridors, Is.All.Matches<CorridorData>(c => c.IsOpened == false));

        // Single room doors Open
        for (int i = 0; i < firstRoom.EnemiesInside.Count; i++)
            firstRoom.RemoveEnemy(firstRoom.EnemiesInside[i]);

        Assert.That(firstRoom.ConnectedCorridors, Is.All.Matches<CorridorData>(c => c.IsOpened));
        Assert.That(dungeonData.Corridors, Has.Exactly(firstRoom.ConnectedCorridors.Count).Matches<CorridorData>(c => c.IsOpened));
    }
}

public class TestEnemy : IEnemy
{

}