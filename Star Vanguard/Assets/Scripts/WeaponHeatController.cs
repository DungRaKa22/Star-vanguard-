using System;
using UnityEngine;

/// <summary>
/// Controls weapon heat and overheat mechanics for Star Vanguard.
/// Implemented based on REA extracted specifications.
/// </summary>
public class WeaponHeatController : MonoBehaviour
{
    public static WeaponHeatController Instance { get; private set; }
    public static event Action<float> OnAnyHeatChanged;
    public static event Action<bool> OnAnyOverheatStateChanged;

    public event Action<float> OnHeatChanged;              // normalized [0..1]
    public event Action<bool> OnOverheatStateChanged;      // true when locked

    private void Awake()
    {
        Instance = this;
    }

    [Header("Heat Settings")]
    [SerializeField] private float m_BaseHeatPerShot = 0.045f;
    [SerializeField] private float m_HeatPerLevel = 0.005f;
    [SerializeField] private float m_CoolingRate = 0.35f;       // Recovers ~35% per second
    [SerializeField] private float m_CoolDownDelay = 0.25f;     // Delay before cooling begins

    private float m_CurrentHeat = 0.0f;
    private float m_LastShotTime = 0.0f;
    private bool m_IsOverheated = false;

    public float CurrentHeat => m_CurrentHeat;
    public bool IsOverheated => m_IsOverheated;

    private void Update()
    {
        // Cool down weapon if delay has passed
        if (Time.time >= m_LastShotTime + m_CoolDownDelay && m_CurrentHeat > 0f)
        {
            float coolingAmount = m_CoolingRate * Time.deltaTime;
            m_CurrentHeat = Mathf.Max(0f, m_CurrentHeat - coolingAmount);
            OnHeatChanged?.Invoke(m_CurrentHeat);
            OnAnyHeatChanged?.Invoke(m_CurrentHeat);

            // If overheated, recover once completely cooled down
            if (m_IsOverheated && m_CurrentHeat <= 0.001f)
            {
                m_CurrentHeat = 0f;
                m_IsOverheated = false;
                OnOverheatStateChanged?.Invoke(false);
                OnAnyOverheatStateChanged?.Invoke(false);
            }
        }
    }

    /// <summary>
    /// Checks if weapon can fire and applies heat if successful.
    /// </summary>
    /// <param name="weaponLevel">Current weapon tier</param>
    /// <returns>True if weapon can shoot, false if overheated</returns>
    public bool TryFire(int weaponLevel = 1)
    {
        if (m_IsOverheated)
        {
            return false;
        }

        float heatAdded = m_BaseHeatPerShot + (weaponLevel * m_HeatPerLevel);
        m_CurrentHeat = Mathf.Min(1.0f, m_CurrentHeat + heatAdded);
        m_LastShotTime = Time.time;
        OnHeatChanged?.Invoke(m_CurrentHeat);
        OnAnyHeatChanged?.Invoke(m_CurrentHeat);

        // Check if this shot triggered an overheat
        if (m_CurrentHeat >= 1.0f)
        {
            m_IsOverheated = true;
            OnOverheatStateChanged?.Invoke(true);
            OnAnyOverheatStateChanged?.Invoke(true);
        }

        return true;
    }

    /// <summary>
    /// Force reset heat (e.g., on player respawn or round transition)
    /// </summary>
    public void ResetHeat()
    {
        m_CurrentHeat = 0f;
        m_IsOverheated = false;
        OnHeatChanged?.Invoke(0f);
        OnAnyHeatChanged?.Invoke(0f);
        OnOverheatStateChanged?.Invoke(false);
        OnAnyOverheatStateChanged?.Invoke(false);
    }
}
