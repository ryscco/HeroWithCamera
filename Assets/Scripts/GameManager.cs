using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager sTheGlobalBehavior = null;
    public Text mGameStateEcho = null;  // Defined in UnityEngine.UI
    public Text WaypointText = null;
    public Text HeroText = null;
    public Text EnemyText = null;
    public HeroBehavior mHero = null;
    public WayPointSystem mWayPoints = null;
    private EnemySpawnSystem mEnemySystem = null;
    private CameraSupport mMainCamera;
    public Camera mWaypointCamera, mEnemyCamera, mHeroCamera;
    GameObject[] enemies;
    void Start()
    {
        GameManager.sTheGlobalBehavior = this;  // Singleton pattern

        // This must occur before EnemySystem's Start();
        Debug.Assert(mWayPoints != null);
        Debug.Assert(mHero != null);

        mMainCamera = Camera.main.GetComponent<CameraSupport>();
        Debug.Assert(mMainCamera != null);

        Bounds b = mMainCamera.GetWorldBound();

        mEnemySystem = new EnemySpawnSystem(b.min, b.max);
        // Make sure all enemy sees the same EnemySystem and WayPointSystem
        EnemyBehavior.InitializeEnemySystem(mEnemySystem, mWayPoints);
        mEnemySystem.GenerateEnemy();  // Can only create enemies when WayPoint is initialized in EnemyBehavior
        enemies = GameObject.FindGameObjectsWithTag("enemy");
    }
    void Update()
    {
        EchoGameState(); // always do this
        if (Input.GetKey(KeyCode.Q))
            Application.Quit();

        if (Input.GetKeyDown(KeyCode.P))
        {
            toggleCamera(mWaypointCamera);
            toggleCamera(mEnemyCamera);
            toggleCamera(mHeroCamera);
        }
        // Check if an enemy is chasing, if so, initiate Enemy Camera
        GameObject e = checkForChase();
        if (e != null) {
            mEnemyCamera.gameObject.SetActive(true);
            mEnemyCamera.GetComponent<EnemyCamera>().enemyChase(e);
        }
    }
    #region Bound Support
    public CameraSupport.WorldBoundStatus CollideWorldBound(Bounds b) { return mMainCamera.CollideWorldBound(b); }
    #endregion 
    private void EchoGameState()
    {
        WaypointText.text = mWayPoints.GetWayPointState();
        HeroText.text = mHero.GetHeroState();
        EnemyText.text = mEnemySystem.GetEnemyState();
    }
    void toggleCamera(Camera cam) // Toggle culling mask to "inactivate" a camera's rendering
    {
        cam.gameObject.SetActive(!(cam.gameObject.activeSelf));
    }
    public GameObject checkForChase() {
        enemies = GameObject.FindGameObjectsWithTag("enemy");
        foreach(GameObject e in enemies) {
            if (e.GetComponent<EnemyBehavior>().myState == EnemyBehavior.EnemyState.Chase) {
                return e;
            }
        }
        return null;
    }
}