using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private readonly List<IPauseable> pauseables = new();

    private void Awake()
    {
        if (G.PauseManager != null)
            Destroy(gameObject);
        else
        {
            G.PauseManager = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void Register(IPauseable pauseable) => pauseables.Add(pauseable);
    public void Unregister(IPauseable pauseable) => pauseables.Remove(pauseable);

    public void Pause()
    {
        foreach (var p in pauseables)
            p.OnPause();
    }

    public void Resume()
    {
        foreach (var p in pauseables)
            p.OnResume();
    }
}