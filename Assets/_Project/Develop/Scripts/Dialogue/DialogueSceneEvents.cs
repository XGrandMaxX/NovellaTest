using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueSceneEvents : MonoBehaviour
{
    [System.Serializable]
    public class NamedEvent
    {
        public string key;
        public UnityEvent unityEvent;
    }

    public List<NamedEvent> events;

    private Dictionary<string, UnityEvent> _eventMap;

    private void Awake()
    {
        _eventMap = new Dictionary<string, UnityEvent>();
        foreach (var e in events)
        {
            if (!_eventMap.ContainsKey(e.key))
                _eventMap.Add(e.key, e.unityEvent);
        }
    }

    public void Invoke(string key)
    {
        if (_eventMap != null && _eventMap.TryGetValue(key, out var evt))
        {
            evt.Invoke();
        }
        else
        {
            Debug.LogWarning($"DialogueSceneEvents: No event found with key '{key}'");
        }
    }
}
