using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCamera : MonoBehaviour
{
    public GameManager theGameManager;
    public Camera selfCam;
    GameObject enemyToFollow;
    Vector3 heroPos, enemyPos;
    void Start()
    {
        gameObject.SetActive(false);
    }
    void Update()
    {
        heroPos = GameObject.Find("Hero").transform.position;
        if (enemyToFollow && enemyToFollow.GetComponent<EnemyBehavior>().myState == EnemyBehavior.EnemyState.Chase)
        {
            enemyPos = enemyToFollow.transform.position;
            selfCam.orthographicSize = (heroPos - enemyPos).magnitude;
            transform.position = heroPos + ((enemyPos - heroPos) * 0.5f);
        }
        else
        {
            enemyToFollow = findNewChase();
            if (enemyToFollow == null) stopChase();
        }
    }
    public void enemyChase(GameObject e)
    {
        enemyToFollow = e;
    }
    public void stopChase()
    {
        enemyToFollow = null;
        transform.position = Vector3.zero;
        gameObject.SetActive(false);
    }
    GameObject findNewChase()
    {
        return theGameManager.GetComponent<GameManager>().checkForChase();
    }
}