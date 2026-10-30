using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Non-generic companion to <see cref="Singleton{T}"/>.
///
/// It owns the [RuntimeInitializeOnLoadMethod] that clears every singleton before Play Mode (Unity
/// refuses that attribute inside a generic class, which used to be a compile error).
/// With persistent managers isolated in BootstrapScene, scenes no longer ship redundant copies
/// and no handover or copy-dropping mechanism is needed.
/// </summary>
public static class SingletonRegistry
{
    private static readonly List<Action> s_Resetters = new List<Action>();

    public static void Register(Action i_Reset)
    {
        if (i_Reset != null && !s_Resetters.Contains(i_Reset))
        {
            s_Resetters.Add(i_Reset);
        }
    }

    /// <summary>
    /// Clears every singleton's cached instance before a new Play Mode session, so a stale
    /// reference from the previous session can never be reused when domain reload is disabled.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetAll()
    {
        for (int i = 0; i < s_Resetters.Count; i++)
        {
            s_Resetters[i]?.Invoke();
        }
    }
}
