using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyBehavior : MonoBehaviour
{
    public Sprite initialTexture, stunnedTexture, eggTexture;
    public enum EnemyState
    {
        Patrol,
        RotateCCW,
        RotateCW,
        Chase,
        Enlarge,
        Shrink,
        Stunned,
        Egg,
        Disabled
    }
    public EnemyState myState = EnemyState.Patrol; // Current enemy state
    private float stateTimer; // For keeping track of a state's starting time
    private GameObject currentTarget; // For chasing the hero
    // Variables to assist with lerp motion upon collision from an egg:
    public bool isPushed = false;
    private Vector3 startingPosition;
    private Vector3 restingPosition;
    // Variables to help with scaling the object's size:
    private Vector3 originalScale;
    private float currentScaleRatio = 1;
    private float maximumEnlargedRatio = 2;
    // All instances of Enemy shares this one WayPoint and EnemySystem
    static private WayPointSystem sWayPoints = null;
    static private EnemySpawnSystem sEnemySystem = null;
    static public void InitializeEnemySystem(EnemySpawnSystem s, WayPointSystem w) { sEnemySystem = s; sWayPoints = w; }
    private const float kSpeed = 5f;
    private int mWayPointIndex = 0;
    private const float kTurnRate = 0.03f / 60f;
    // Use this for initialization
    void Start()
    {
        originalScale = transform.localScale;
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

    bool isTouchingTarget()
    {
        Collider2D localCollider = gameObject.GetComponent<Collider2D>();
        Collider2D targetCollider = currentTarget.GetComponent<Collider2D>();

        return localCollider.IsTouching(targetCollider);
    }

    // For chasing the hero:
    void UpdateChase()
    {
        if (isTouchingTarget())
        {
            if (currentTarget.name == "Hero")
            {
                HeroBehavior player = currentTarget.GetComponent<HeroBehavior>();
                player.TouchedEnemy();
            }

            ThisEnemyIsHit();
        }
        else
        {
            if (targetIsWithin(40))
            {
                // Lock onto the target instantly and chase:
                PointAtPosition(currentTarget.transform.position, 100);
                transform.position += (kSpeed * Time.smoothDeltaTime) * transform.up;
            }
            else
            {
                currentScaleRatio = 1;
                myState = EnemyState.Enlarge;
                resetTimer();
            }
        }
    }

    void UpdateScale(float rate)
    {
        if (timeHasElapsed(1)) // If at least 1 second has elapsed:
        {
            if (rate > 0) // is enlarging
            {
                currentScaleRatio = maximumEnlargedRatio;
                myState = EnemyState.Shrink; // switch to shrinking
            }
            else // is shrinking
            {
                currentScaleRatio = 1; // Original size
                GetComponent<Renderer>().material.color = Color.white;
                myState = EnemyState.Patrol; // return to normal operation
            }

            resetTimer();
        }
        else
        {
            currentScaleRatio += rate * Time.deltaTime;
        }

        transform.localScale = originalScale * currentScaleRatio;
    }

    void pushBy(float numberOfUnits, GameObject orientation)
    {
        isPushed = true;
        startingPosition = transform.position;
        restingPosition = orientation.transform.up * numberOfUnits + startingPosition;
        resetTimer();
    }

    void UpdateLerpPosition()
    {
        if (isPushed)
        {
            float elapsedTime = Time.time - stateTimer;

            if (elapsedTime < 2)
            {
                // My lerp ratio is a function of elapsed time in the form of a parabola,
                // such that the object slows down to a halt upon reaching 2 seconds:
                float lerpRatio = elapsedTime * (4f - elapsedTime) / 4f;
                transform.position = Vector3.Lerp(startingPosition, restingPosition, lerpRatio);
            }
            else
            {
                transform.position = restingPosition;
                isPushed = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (myState)
        {
            case EnemyState.Patrol:
                {
                    sWayPoints.CheckNextWayPoint(transform.position, ref mWayPointIndex);
                    PointAtPosition(sWayPoints.WayPoint(mWayPointIndex), kTurnRate);
                    transform.position += (kSpeed * Time.smoothDeltaTime) * transform.up;
                    break;
                }
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
                    UpdateScale(maximumEnlargedRatio - 1);
                    break;
                }
            case EnemyState.Shrink:
                {
                    UpdateScale(1 - maximumEnlargedRatio);
                    break;
                }
            case EnemyState.Stunned:
                {
                    transform.Rotate(0, 0, 90f * Time.deltaTime);
                    UpdateLerpPosition();
                    break;
                }
            case EnemyState.Egg:
                {
                    UpdateLerpPosition();
                    break;
                }
                // Disabled state does nothing, therefore a default case is not necessary.
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
            if (myState == EnemyState.Egg)
            {
                ThisEnemyIsHit();
            }
            else if (myState == EnemyState.Stunned)
            {
                pushBy(8, g);
                gameObject.GetComponent<SpriteRenderer>().sprite = eggTexture;
                myState = EnemyState.Egg;
            }
            else
            {
                pushBy(4, g);
                currentScaleRatio = 1;
                transform.localScale = originalScale;
                GetComponent<Renderer>().material.color = Color.white;
                gameObject.GetComponent<SpriteRenderer>().sprite = stunnedTexture;
                myState = EnemyState.Stunned;
            }
        }
    }

    private void ThisEnemyIsHit()
    {
        // Disabled state prevents the enemies-destroyed count from
        // being incremented multiple times for the same enemy.
        if (myState != EnemyState.Disabled)
        {
            myState = EnemyState.Disabled;
            sEnemySystem.OneEnemyDestroyed();
            Destroy(gameObject);
        }
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