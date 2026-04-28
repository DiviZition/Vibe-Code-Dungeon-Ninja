using System.Collections;
using Dungeon;
using NUnit.Framework;
using Unity.Properties;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PlaymodeTests
{
    [Test]
    public void VerifyApplicationRunning()
    {
        Assert.That(Application.isPlaying, Is.True);
    }

    [Test]
    public void VerifyDungeonInScene()
    {
        var go = GameObject.FindAnyObjectByType<DungeonManager>();
        Assert.That(go, Is.Not.Null, "Dungeon manager not found in {0}", SceneManager.GetActiveScene().path);
    }

    [UnityTest]
    public IEnumerator Accept_ShouldExecuteVisit_WhenCalledWithVisitor()
    {
        //Given
        var obj = new GameObject();
        //var Dunheon = obj.AddComponent<>();
        yield return null;
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