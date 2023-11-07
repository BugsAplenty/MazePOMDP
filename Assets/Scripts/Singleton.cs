using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T BeginInstance;
    private static readonly object Lock = new();
    private static bool BeginApplicationIsQuitting;

    public static T Instance
    {
        get
        {
            if (BeginApplicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                return null;
            }

            lock (Lock)
            {
                if (BeginInstance == null)
                {
                    BeginInstance = (T) FindFirstObjectByType(typeof(T));
                    if (FindObjectsByType(typeof(T), FindObjectsSortMode.None).Length > 1)
                    {
                        Debug.LogError($"[Singleton] Something went wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                        return BeginInstance;
                    }

                    if (BeginInstance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        BeginInstance = singletonObject.AddComponent<T>();
                        singletonObject.name = $"{typeof(T).ToString()} (Singleton)";

                        DontDestroyOnLoad(singletonObject);

                        Debug.Log($"[Singleton] An instance of {typeof(T)} is needed in the scene, so '{singletonObject}' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        Debug.Log($"[Singleton] Using instance already created: {BeginInstance.gameObject.name}");
                    }
                }

                return BeginInstance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (BeginInstance == null)
        {
            BeginInstance = this as T;
        }
        else
        {
            if (this != BeginInstance)
            {
                Destroy(gameObject);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (BeginInstance == this)
        {
            BeginApplicationIsQuitting = true;
        }
    }
}
