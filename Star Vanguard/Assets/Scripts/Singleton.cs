using System;
using UnityEngine;

/// <summary>
/// Marks a singleton that belongs to its scene (UI, wave director, parallax, player).
/// The generic base cannot be asked "are you scene bound?" from static code, so the fact lives
/// on the type instead of on a property.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class SceneBoundSingletonAttribute : Attribute
{
}

public abstract class Singleton<T> : MonoBehaviour where T: MonoBehaviour
{
    private static T _instance;
    private static bool _isQuitting = false;

    /// <summary>Scene-bound singletons can never be auto-created (see Instance).</summary>
    private static readonly bool s_SceneBound =
        typeof(T).IsDefined(typeof(SceneBoundSingletonAttribute), true);

    /// <summary>
    /// Unity forbids [RuntimeInitializeOnLoadMethod] inside a generic class, so the reset is
    /// registered with the non-generic <see cref="SingletonRegistry"/> instead - one hook per
    /// closed Singleton&lt;T&gt;, all fired before each Play Mode session.
    /// </summary>
    static Singleton()
    {
        SingletonRegistry.Register(ResetStaticData);
    }

    private static void ResetStaticData()
    {
        _instance = null;
        _isQuitting = false;
    }

    public static bool HasInstance => _instance != null && !_isQuitting;

    /// <summary>
    /// Scene-owned singletons (UI, wave director, parallax, player) must NOT survive a
    /// scene load: their serialized references point at objects of the old scene, which
    /// Unity destroys, and any later access throws MissingReferenceException.
    /// Systems that are pure logic (GameManager, AudioManager, PoolManager, ...) keep
    /// persisting across scenes.
    /// </summary>
    protected virtual bool PersistAcrossScenes => true;


    protected virtual void Awake()
    {
        if (_isQuitting)
        {
            Destroy(gameObject);
            return;
        }

        if (_instance != null && _instance != this)
        {
            if (_instance.gameObject.scene == gameObject.scene)
            {
                // Two copies inside one scene is a real authoring mistake.
                Debug.LogWarning($"Duplicate {typeof(T).Name} detected in scene '{gameObject.scene.name}'. " +
                                 "Destroying this instance.");
            }

            Destroy(gameObject);
            return;
        }
        
        _instance = this as T;
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        if (PersistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public static T Instance
    {
        get
        {
            if (_isQuitting || !Application.isPlaying)
            {
                return null;
            }
            
            if (_instance != null)
            {
                return _instance;
            }
            
            // Check if an instance already exists in the scene
            var existing = FindAnyObjectByType<T>();
            if (existing != null)
            {
                _instance = existing;
                return _instance;
            }

            // Do not lazily spawn new GameObjects if application is shutting down
            if (_isQuitting || !Application.isPlaying)
            {
                return null;
            }

            // Scene-bound singletons (UI, wave director, parallax, player) must never be invented:
            // a manager without its scene has no references and is useless. Inventing one while a
            // scene unloads is what produced "Some objects were not cleaned up when closing the
            // scene" - the orphan then won the next scene's duplicate check and killed the real
            // manager there.
            if (s_SceneBound)
            {
                return null;
            }

            // Create new instance if none exists
            var singletonObject = new GameObject(typeof(T).Name);
            T created = singletonObject.AddComponent<T>();
            _instance = created;

            Singleton<T> owner = created as Singleton<T>;
            if (owner == null || owner.PersistAcrossScenes)
            {
                DontDestroyOnLoad(singletonObject);
            }

            return _instance;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}
