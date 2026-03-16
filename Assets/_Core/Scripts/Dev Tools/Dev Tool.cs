using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class DevTool : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;
    private UniTask _tryFireEventsTask;

    [Button]
    private void DoAction()
    {
        FirebaseService.InvokeEvent("login");
    }

    private void Start()
    {
        FirebaseService.InvokeEvent("login");
    }
}
