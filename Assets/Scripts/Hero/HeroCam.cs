using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroCam : MonoBehaviour
{
    public Camera selfCam;
    public Camera mainCam;
    public GameObject theHero;
    Vector3 pos;
    void OnEnable()
    {
        pos = theHero.transform.position;
        selfCam.transform.position = pos;
    }
    void Update()
    {
        pos = theHero.transform.position;
        StartCoroutine(doLerp(pos, 0.5f));
    }
    IEnumerator doLerp(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = selfCam.transform.position;

        while (time < duration)
        {
            selfCam.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        selfCam.transform.position = targetPosition;
    }
    IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = theHero.transform.position;

        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            float x = originalPos.x - Random.Range(-1f, 1f) * magnitude;
            float y = originalPos.y - Random.Range(-1f, 1f) * magnitude;

            selfCam.transform.position = new Vector3(x, y, originalPos.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        selfCam.transform.position = originalPos;
    }
    public void shakeHeroCam() {
        StartCoroutine(Shake(1f,1f));
    }
}