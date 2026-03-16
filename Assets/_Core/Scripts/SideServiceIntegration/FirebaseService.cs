using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using System;
using System.Collections.Concurrent;
using UnityEngine;

public class FirebaseService : IDisposable
{
    private ConcurrentQueue<Action> _eventsQueue;

    public bool IsInitialized { get; private set; } = false;
    private UniTask _tryFireEventsTask;

    private async UniTask TryFireEvents()
    {
        if (IsInitialized == false && _tryFireEventsTask.Status.IsCompleted())
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
            if (status != DependencyStatus.Available)
            {
                Debug.LogError($"Dependencies unavailable: {status}");
                return;
            }

            Debug.Log($"Dependancy resolved: ✅");
            IsInitialized = true;
        }

        while (_eventsQueue.Count > 0)
            if (_eventsQueue.TryDequeue(out var action) == true)
                action?.Invoke();
    }

    public void FireEvent(string name, params Parameter[] parameter)
    {
        if (_eventsQueue == null)
            _eventsQueue = new ConcurrentQueue<Action>();

        _eventsQueue.Enqueue(() => DirectEventLog(name, parameter));

        _tryFireEventsTask = TryFireEvents();
    }

    private void DirectEventLog(string name, params Parameter[] parameter)
    {
        Debug.Log($"Firebase event inoked: {name}");
        try
        {
            FirebaseAnalytics.LogEvent(name, parameter);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firebase event: {name} \nfailed with: {ex}");
        }
    }

    public void Dispose()
    {
        _eventsQueue.Clear();
    }
}