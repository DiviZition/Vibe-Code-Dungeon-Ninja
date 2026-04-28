using System.Collections;
using System.Linq;
using Dungeon;
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
        var obj = new GameObject("Dungeon Generator Test");
        var dg = obj.AddComponent<DungeonGenerator>();
        yield return null;

        dg.Generate();
        Assert.That(dg.Data, Is.Not.Null);
        Assert.That(dg.Data.Rooms, Is.Not.Null);
        Assert.That(dg.Data.Rooms.Count, Is.EqualTo(dg.ZonesCount));
        Assert.That(dg.Data.Rooms.SelectMany(r => r.ConnectedCorridors), Is.All.Matches<CorridorData>(c => c.IsOpened == false));
        
        dg.Data.OpenRoomCorridors(0);
        Assert.That(dg.Data.Rooms[0].ConnectedCorridors, Is.All.Matches<CorridorData>(c => c.IsOpened == true));
        Assert.That(dg.Data.Corridors, Has.Exactly(dg.Data.Rooms[0].ConnectedCorridors.Count).Matches<CorridorData>(c => c.IsOpened));

        for (int i = 0; i < dg.Data.Rooms.Count; i++)
            dg.Data.OpenRoomCorridors(i);

        Assert.That(dg.Data.Corridors, Is.All.Matches<CorridorData>(c => c.IsOpened == true));
    }

    public class TestVisitor : MonoBehaviour, IVisitor
    {
        public bool Visited { get; private set; }

        void IVisitor.Visited<T>(T visitor)
        {
            Visited = true;
        }
    }
}

internal interface IVisitor
{
    void Visited<T>(T visitor) where T : Component, IVisitable
    {

    }
}

internal interface IVisitable
{
}