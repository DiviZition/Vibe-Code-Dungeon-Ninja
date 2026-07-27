using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

public class DevTool : MonoBehaviour
{
    private DungeonGameDirector _dungeonDirector;

    [Inject]
    private void Construct(DungeonGameDirector dd)
    {
        _dungeonDirector = dd;
    }

    [Button]
    private void StartGame()
    {
        _dungeonDirector.StartGame().Forget();
    }

}
