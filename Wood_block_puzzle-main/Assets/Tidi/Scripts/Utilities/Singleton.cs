using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton class
/// </summary>
/// <typeparam name="T">Type of the singleton</typeparam>
public abstract class Singleton<T> : BaseBehaviour where T : Singleton<T>
{
    // Check to see if we're about to be destroyed.
    private static bool m_ShuttingDown = false;
    private static object m_Lock = new object();
    private static T s_Instance;

    /// <summary>
    /// The static reference to the instance
    /// </summary>
    public static T Instance
    {
        get
        {
            // if (m_ShuttingDown)
            // {
            //     Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
            //         "' already destroyed. Returning null.");
            //     return null;
            // }
 
            lock (m_Lock)
            {
                if (s_Instance == null)
                {
                    // Search for existing instance.
                    s_Instance = (T)FindObjectOfType(typeof(T));
 
                    // Create new instance if one doesn't already exist.
                    if (s_Instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        s_Instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";
 
                        // Make instance persistent.
                        DontDestroyOnLoad(singletonObject);
                    }
                }
 
                return s_Instance;
            }
        }
        protected set
        {
            s_Instance = value;
        }
    }

    /// <summary>
    /// Gets whether an instance of this singleton exists
    /// </summary>
    public static bool s_InstanceExists { get { return s_Instance != null; } }

    /// <summary>
    /// Awake method to associate singleton with instance
    /// </summary>
    protected virtual void Awake()
    {
        if (s_Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            s_Instance = (T)this;
        }
    }

    /// <summary>
    /// OnDestroy method to clear singleton association
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (s_Instance == this)
        {
            s_Instance = null;
        }

        m_ShuttingDown = true;
    }

    protected virtual void OnApplicationQuit()
    {
        m_ShuttingDown = true;
    }
}

