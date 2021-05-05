using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeroBehavior : MonoBehaviour
{
    public Camera heroCam;
    public EggSpawnSystem mEggSystem = null;
    public EnemyBehavior touchedChaser = null;
    private const float kHeroRotateSpeed = 90f / 2f; // 90-degrees in 2 seconds
    private const float kHeroSpeed = 20f;  // 20-units in a second
    private float mHeroSpeed = kHeroSpeed;
    private bool mMouseDrive = true;
    //  Hero state
    private int mHeroTouchedEnemy = 0;

    public void TouchedEnemy()
    {
        mHeroTouchedEnemy++;
    }

    public string GetHeroState()
    {
        return "HERO: Drive(" + (mMouseDrive ? "Mouse" : "Key") + (
               ") Hit(" + mHeroTouchedEnemy) + ") "
               + mEggSystem.EggSystemStatus();
    }
    private void Awake()
    {
        // Actually since Hero spwans eggs, this can be done in the Start() function, but, 
        // just to show this can also be done here.
        Debug.Assert(mEggSystem != null);
        EggBehavior.InitializeEggSystem(mEggSystem);
    }
    void Update()
    {
        UpdateMotion();
        ProcessEggSpwan();
    }
    private int EggsOnScreen() { return mEggSystem.GetEggCount(); }
    private void UpdateMotion()
    {
        if (Input.GetKeyDown(KeyCode.M))
            mMouseDrive = !mMouseDrive;

        // Only support rotation
        transform.Rotate(Vector3.forward, -1f * Input.GetAxis("Horizontal") *
                                    (kHeroRotateSpeed * Time.smoothDeltaTime));
        if (mMouseDrive)
        {
            Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            p.z = 0f;
            transform.position = p;
        }
        else
        {
            mHeroSpeed += Input.GetAxis("Vertical");
            transform.position += transform.up * (mHeroSpeed * Time.smoothDeltaTime);
        }
    }

    private void ProcessEggSpwan()
    {
        if (mEggSystem.CanSpawn())
        {
            if (Input.GetKey("space"))
                mEggSystem.SpawnAnEgg(transform.position, transform.up);
        }
    }
}