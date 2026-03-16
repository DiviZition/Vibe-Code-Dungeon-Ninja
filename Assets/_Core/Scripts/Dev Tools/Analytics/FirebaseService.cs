using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using System;
using System.Collections.Concurrent;
using UnityEngine;

public static class FirebaseService
{
    private static ConcurrentQueue<Action> _eventsQueue;

    public static bool IsInitialized { get; private set; } = false;
    private static UniTask _tryFireEventsTask;

    private static async UniTask TryFireEvents()
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
        {
            if (_eventsQueue.TryDequeue(out var action) == true)
            {
                action?.Invoke();
            }   
        }
    }

    public static void InvokeEvent(string name, params Parameter[] parameter)
    {
        if (_eventsQueue == null)
            _eventsQueue = new ConcurrentQueue<Action>();

        _eventsQueue.Enqueue(() => DirectEventLog(name, parameter));

        _tryFireEventsTask = TryFireEvents();
    }

    private static void DirectEventLog(string name, params Parameter[] parameter)
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

    private static void DisposeOnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += DisposeOnExit;
        void DisposeOnExit(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                UnityEditor.EditorApplication.playModeStateChanged -= DisposeOnExit;
                Dispose();
            }
        }
#endif
        Application.quitting += Dispose;
    }

    public static void Dispose()
    {
        Application.quitting -= Dispose;
        _eventsQueue.Clear();
        IsInitialized = false;
    }
}