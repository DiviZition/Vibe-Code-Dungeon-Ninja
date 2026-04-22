using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class DevTool : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;
    private UniTask _tryFireEventsTask;
    private FirebaseService _fireBase;

    [Inject]
    private void Construct(FirebaseService fireBase)
    {
        _fireBase = fireBase;
    }

    [Button]
    private void DoAction()
    {
        _fireBase.FireEvent("player_died");
    }

    private void Start()
    {
        DoAction();
    }
}
