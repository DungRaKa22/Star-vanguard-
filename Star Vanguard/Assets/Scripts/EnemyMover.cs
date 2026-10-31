using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyMover : MonoBehaviour
{
    public eEnemyMoveType m_MoveType = eEnemyMoveType.StraightDown;
    public float Speed { get; set; }
    
    // Sine wave
    public float SineAmplitude { get; set; } = 1.2f;
    public float SineFrequency { get; set; } = 2f;
    private float m_SinePhase;
    
    // Dive
    private float m_SpawnTime;
    public float DiveDelay { get; set; } = 0.6f;
    public float DiveSpeed { get; set; } = 7f;
    private Transform m_Target;
    private Vector2 m_DiveDirection;
    private bool m_Dived;

    // Semicircle
    private Vector2 m_SemicircleCenter;
    private float m_SemicircleRadius = 3f;
    private bool m_SemicircleClockwise = true;
    private float m_SemicircleCurrentAngle;
    private float m_SemicircleAngularSpeed;
    
    // Formation
    private Vector3 m_FormationOffset; // Offset from formation center
    private static Vector3 s_FormationCenter; // Shared formation center position
    private static Vector3 s_FormationVelocity; // Shared formation velocity

    // Cubic Bezier (REA extracted formula)
    private Vector2 m_BezierP0, m_BezierP1, m_BezierP2, m_BezierP3;
    private float m_BezierDuration = 4.0f;
    private float m_BezierStartTime;
    
    private Camera m_Camera;
    
    private void Awake()
    {
        m_Camera = Camera.main;
    }

    private void OnEnable()
    {
        m_SpawnTime = Time.time;
        m_SinePhase = Random.value * Mathf.PI * 2f; // random phase
    }
    
    public void InitDive(Transform i_Target)
    {
        m_Target = i_Target;
        m_Dived = false;
    }
    
    public void InitSemicircle(Vector2 i_Center, float i_Radius, bool i_Clockwise, float i_StartAngle)
    {
        m_SemicircleCenter = i_Center;
        m_SemicircleRadius = i_Radius;
        m_SemicircleClockwise = i_Clockwise;
        m_SemicircleCurrentAngle = i_StartAngle;
        m_SemicircleAngularSpeed = Speed / i_Radius; // Convert linear speed to angular speed
    }
    
    public void InitFormation(Vector3 i_Offset)
    {
        m_FormationOffset = i_Offset;
    }
    
    public static void SetFormationCenter(Vector3 i_Center)
    {
        s_FormationCenter = i_Center;
    }

    public void InitCubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float duration)
    {
        m_BezierP0 = p0;
        m_BezierP1 = p1;
        m_BezierP2 = p2;
        m_BezierP3 = p3;
        m_BezierDuration = Mathf.Max(0.1f, duration);
        m_BezierStartTime = Time.time;
        m_MoveType = eEnemyMoveType.CubicBezier;
        transform.position = new Vector3(p0.x, p0.y, transform.position.z);
    }

    private void Update()
    {
        // Enemies come from a pool that may have been created in a previous scene
        if (m_Camera == null) EnsureCamera();

        switch(m_MoveType)
        {
            case(eEnemyMoveType.StraightDown):
                handleStraightDownWave();
                break;
            case(eEnemyMoveType.SineHorizontal):
                handleSineHorizontalWave();
                break;
            case(eEnemyMoveType.DiveAtPlayer):
                handleDiveAtPlayerWave();
                break;
            case(eEnemyMoveType.SemicircleArc):
                handleSemicircleArcWave();
                break;
            case(eEnemyMoveType.Formation):
                handleFormationWave();
                break;
            case(eEnemyMoveType.CubicBezier):
                handleCubicBezierWave();
                break;
            default:
                Debug.LogError("EnemyMover: Unknown move type " + m_MoveType);
                break;
        }

        handleOffScreen();
    }

    private void handleDiveAtPlayerWave()
    {
        if (!m_Dived && Time.time - m_SpawnTime >= DiveDelay && m_Target != null)
        {
            m_DiveDirection = (m_Target.position - transform.position).normalized;
            m_Dived = true;
        }
        
        transform.Translate(Time.deltaTime * 
                            (m_Dived ? DiveSpeed : Speed) *
                            (m_Dived ? m_DiveDirection : Vector2.down )
                            , Space.World);
    }

    private void handleSineHorizontalWave()
    {
        transform.Translate(Time.deltaTime * Speed * Vector2.down, Space.World);
        float dx = Mathf.Sin((Time.time + m_SinePhase) * SineFrequency) * SineAmplitude * Time.deltaTime;
        transform.Translate(new Vector2(dx, 0f), Space.World);
    }

    private void handleStraightDownWave()
    {
        transform.Translate(Time.deltaTime * Speed * Vector2.down, Space.World); 
    }

    private void handleSemicircleArcWave()
    {
        // Update the current angle based on angular speed
        float angleDirection = m_SemicircleClockwise ? -1f : 1f;
        m_SemicircleCurrentAngle += m_SemicircleAngularSpeed * angleDirection * Time.deltaTime;
        
        // Calculate the new position based on the current angle
        float x = m_SemicircleCenter.x + m_SemicircleRadius * Mathf.Cos(m_SemicircleCurrentAngle);
        float y = m_SemicircleCenter.y + m_SemicircleRadius * Mathf.Sin(m_SemicircleCurrentAngle);
        
        transform.position = new Vector3(x, y, transform.position.z);
    }
    
    private void handleFormationWave()
    {
        // Move to formation center + individual offset
        Vector3 targetPosition = s_FormationCenter + m_FormationOffset;
        transform.position = targetPosition;
    }

    private void handleCubicBezierWave()
    {
        float t = Mathf.Clamp01((Time.time - m_BezierStartTime) / m_BezierDuration);
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        // B(t) = (1-t)^3 * P0 + 3(1-t)^2 * t * P1 + 3(1-t) * t^2 * P2 + t^3 * P3
        Vector2 pos = uuu * m_BezierP0 + 3f * uu * t * m_BezierP1 + 3f * u * tt * m_BezierP2 + ttt * m_BezierP3;
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

        // Heading rotation along tangent B'(t)
        Vector2 tangent = 3f * uu * (m_BezierP1 - m_BezierP0) + 6f * u * t * (m_BezierP2 - m_BezierP1) + 3f * tt * (m_BezierP3 - m_BezierP2);
        if (tangent.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // When curve completes, switch to straight descent
        if (t >= 1.0f)
        {
            m_MoveType = eEnemyMoveType.StraightDown;
            transform.rotation = Quaternion.identity;
        }
    }

    private void handleOffScreen()
    {
        if (m_Camera == null) return; // no camera to measure the viewport against yet

        // Return to pool if out of screen
        Vector3 viewPortPosition = m_Camera.WorldToViewportPoint(transform.position);
        if (viewPortPosition.y < -0.15f || viewPortPosition.x < -1f || viewPortPosition.x > 2f)
        {
            // Return enemy to pool instead of destroying
            var enemyController = GetComponent<EnemyController>();
            if (enemyController != null)
            {
                PoolManager.Instance?.ReturnEnemy(enemyController);
            }
        }
    }
    private void EnsureCamera()
    {
        if (m_Camera == null) m_Camera = Camera.main;
    }
}