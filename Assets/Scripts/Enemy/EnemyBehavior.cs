using UnityEngine;
using System.Collections;

public partial class EnemyBehavior : MonoBehaviour
{
    enum EnemyState
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

    private EnemyState myState = EnemyState.Patrol;
    private float stateTimer; // For keeping track of a state's starting time
    private GameObject currentTarget; // For chasing

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

        if (timeHasElapsed(1f))
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

    // For chasing the hero:
    void UpdateChase()
    {

        PointAtPosition(currentTarget.transform.position, kTurnRate);
        transform.position += (kSpeed * Time.smoothDeltaTime) * transform.up;
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
                    break;
                }
            case EnemyState.Shrink:
                {
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
            //ThisEnemyIsHit();

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
