using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    public Camera wpCam;
    private Vector3 mInitPosition = Vector3.zero;
    private int mHitCount = 0;
    private const int kHitLimit = 3;
    private const float kRepositionRange = 15f; // +- this value
    private Color mNormalColor = Color.white;

    // Shake
    Vector3 init;
    public float mag = 0f;
    public float dur = 0f;

    SpriteRenderer Wp;
    private float alphaC = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        mInitPosition = transform.position;
    }


    private void Reposition()
    {
        Vector3 p = mInitPosition;
        p += new Vector3(Random.Range(-kRepositionRange, kRepositionRange),
                         Random.Range(-kRepositionRange, kRepositionRange),
                         0f);
        transform.position = p;
        Wp.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Egg(Clone)")
        {

            Wp = GetComponent<SpriteRenderer>();
            Wp.color = new Color(1.0f, 1.0f, 1.0f, alphaC);
            mHitCount++;
            if (mHitCount == 1)
            {
                StartCoroutine(Shake(1, 1));
            }
            else if (mHitCount == 2)
            {
                StartCoroutine(Shake(2, 2));
            }
            else if (mHitCount == 3)
            {
                StartCoroutine(Shake(3, 3));
            }
            else if (mHitCount == 4)
            {
                if (mHitCount > kHitLimit)
                {
                    Reposition();
                }
                mHitCount = 0;
            }
        }
    }

    IEnumerator Shake(float dur, float mag)
    {
        // Set WayPointCam original position to current position
        // If camera is not active (viewing another waypoint) then
        //   turn it on and watch the waypoint shake, then deactivate
        Vector2 wpCamOrigPos = wpCam.gameObject.transform.localPosition;
        if (!wpCam.gameObject.activeSelf) {
            wpCam.gameObject.transform.localPosition = gameObject.transform.localPosition;
            wpCam.gameObject.SetActive(true);
        }

        Vector2 orig = transform.position;

        float elapseT = 0.0f;
        dur = dur / 2;

        while (elapseT < dur)
        {
            float x = orig.x + (Random.Range(0f, 1f) * mag);
            float y = orig.y + (Random.Range(0f, 1f) * mag);

            transform.position = new Vector2(x, y);
            elapseT += Time.smoothDeltaTime;
            yield return null;
        }
        transform.position = orig;
        // Return WayPointCam to its original position and turn it off
        wpCam.gameObject.transform.localPosition = wpCamOrigPos;
        wpCam.gameObject.SetActive(false);
    }
}