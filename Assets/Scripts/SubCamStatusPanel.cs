using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubCamStatusPanel : MonoBehaviour
{
    public Camera waypointCamera, enemyCamera, heroCamera;
    public Text waypointCameraText, enemyCameraText, heroCameraText;
    void Start()
    {

    }
    void Update()
    {
        if (waypointCamera.gameObject.activeSelf)
        {
            waypointCameraText.text = "Waypoint Camera: (Active)";
        }
        else
        {
            waypointCameraText.text = "Waypoint Camera: (Inactive)";
        }
        if (enemyCamera.gameObject.activeSelf)
        {
            enemyCameraText.text = "Enemy Camera: (Active)";
        }
        else
        {
            enemyCameraText.text = "Enemy Camera: (Inactive)";
        }
        if (heroCamera.gameObject.activeSelf)
        {
            heroCameraText.text = "Hero Camera: (Active)";
        }
        else
        {
            heroCameraText.text = "Hero Camera: (Inactive)";
        }
    }
}