using UnityEngine;
using System.Collections;

public partial class EnemyBehavior : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        RotateCCW,
        RotateCW,
        Chase,
        Enlarge,
        Shrink,
        Stunned,
        Egg
    }

    public EnemyState myState = EnemyState.Patrol;
    private float stateTimer; // For keeping track of a state's starting time
    private GameObject currentTarget; // For chasing

    Vector3 originalScale;
    float currentScaleRatio = 1;

    // All instances of Enemy shares this one WayPoint and EnemySystem
    static private WayPointSystem sWayPoints = null;
    static private EnemySpawnSystem sEnemySystem = null;
    static public void InitializeEnemySystem(EnemySpawnSystem s, WayPointSystem w) { sEnemySystem = s; sWayPoints = w; }

    private const float kSpeed = 5f;
    private int mWayPointIndex = 0;

    private const float kTurnRate = 0.03f / 60f;
    private int mNumHit = 0;
    private const int kHitsToDestroy = 4;
    private const float kEnemyEnergyLost = 0.8f;

    // Use this for initialization
    void Start()
    {
        mWayPointIndex = sWayPoints.GetInitWayIndex();
    }

    private void enterRotation()
    {
        myState = EnemyState.RotateCCW;
        gameObject.GetComponent<Renderer>().material.color = Color.red;
        resetTimer();
    }

    // For handling the CCW and CW rotation states:
    void UpdateRotation(float rate)
    {
        transform.Rotate(0, 0, rate * Time.deltaTime);

        if (timeHasElapsed(1)) // If at least 1 second has elapsed:
        {
            if (rate > 0) // is CCW
            {
                myState = EnemyState.RotateCW; // switch to CW rotation
            }
            else // is CW
            {
                myState = EnemyState.Chase; // chase the hero
            }

            resetTimer();
        }
    }

    // Returns true if the currentTarget is within the specified distance:
    bool targetIsWithin(float radius)
    {
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        return distance <= radius;
    }

    // For chasing the hero:
    void UpdateChase()
    {
        if (targetIsWithin(40))
        {
            // Lock onto the target instantly and chase:
            PointAtPosition(currentTarget.transform.position, 100);
            transform.position += (kSpeed * Time.smoothDeltaTime) * transform.up;
        }
        else
        {
            enterScalingStates(); // Begin enlarging then shrinking
        }
    }

    void enterScalingStates()
    {
        currentScaleRatio = 1;
        originalScale = transform.localScale;
        myState = EnemyState.Enlarge;
        resetTimer();
    }

    void UpdateScale(float rate)
    {
        currentScaleRatio += rate * Time.deltaTime;
        transform.localScale = originalScale * currentScaleRatio;

        if (timeHasElapsed(1)) // If at least 1 second has elapsed:
        {
            if (rate > 0) // is enlarging
            {
                transform.localScale = originalScale * (rate + 1);
                myState = EnemyState.Shrink; // switch to shrinking
            }
            else // is shrinking
            {
                transform.localScale = originalScale;
                GetComponent<Renderer>().material.color = Color.white;
                myState = EnemyState.Patrol; // return to normal operation
            }

            resetTimer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (myState)
        {
            case EnemyState.RotateCCW:
                {
                    UpdateRotation(90f);
                    break;
                }
            case EnemyState.RotateCW:
                {
                    UpdateRotation(-90f);
                    break;
                }
            case EnemyState.Chase:
                {
                    UpdateChase();
                    break;
                }
            case EnemyState.Enlarge:
                {
                    UpdateScale(1);
                    break;
                }
            case EnemyState.Shrink:
                {
                    UpdateScale(-1);
                    break;
                }
            case EnemyState.Stunned:
                {
                    break;
                }
            case EnemyState.Egg:
                {
                    break;
                }
            default: // Patrol state:
                {
                    sWayPoints.CheckNextWayPoint(transform.position, ref mWayPointIndex);
                    PointAtPosition(sWayPoints.WayPoint(mWayPointIndex), kTurnRate);
                    transform.position += (kSpeed * Time.smoothDeltaTime) * transform.up;
                    break;
                }
        }
    }

    private void PointAtPosition(Vector3 p, float r)
    {
        Vector3 v = p - transform.position;
        transform.up = Vector3.LerpUnclamped(transform.up, v, r);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log("Enemy OnTriggerEnter");
        TriggerCheck(collision.gameObject);
    }

    private void TriggerCheck(GameObject g)
    {
        if (g.name == "Hero")
        {
            if (myState == EnemyState.Patrol)
            {
                currentTarget = g;
                enterRotation();
            }
        }
        else if (g.name == "Egg(Clone)")
        {
            mNumHit++;
            if (mNumHit < kHitsToDestroy)
            {
                Color c = GetComponent<Renderer>().material.color;
                c.a = c.a * kEnemyEnergyLost;
                GetComponent<Renderer>().material.color = c;
            }
            else
            {
                ThisEnemyIsHit();
            }
        }
    }

    private void ThisEnemyIsHit()
    {
        sEnemySystem.OneEnemyDestroyed();
        Destroy(gameObject);
    }

    private void resetTimer()
    {
        stateTimer = Time.time;
    }

    private bool timeHasElapsed(float targetTime)
    {
        float elapsedTime = Time.time - stateTimer;
        return elapsedTime >= targetTime;
    }
}
