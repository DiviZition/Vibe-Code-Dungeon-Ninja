using Firebase;
using Firebase.Extensions;
using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class DevTool : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;


    [Button]
    private void DoAction()
    {
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(status => ObDependencyStatusReceived(status));
    }

    private void ObDependencyStatusReceived(Task<DependencyStatus> statusTask)
    {
        try
        {
            if (statusTask.IsCompletedSuccessfully == false)
                throw new Exception($"Dependancy resolvation failed: {statusTask.Exception}");
            else if (statusTask.Result != DependencyStatus.Available)
                throw new Exception($"Dependancies unavailable. Status: {statusTask.Result}");
            else
                Debug.Log($"Dependancy resolved: ✅");
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }
}
