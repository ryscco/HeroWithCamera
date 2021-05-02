using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    private Vector3 mInitPosition = Vector3.zero;
    private int mHitCount = 0;
    private const int kHitLimit = 3;
    private const float kRepositionRange = 15f; // +- this value
    private Color mNormalColor = Color.white;


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
        alphaC = 1.0f;
        Wp.color = new Color(1.0f, 1.0f, 1.0f, alphaC);
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
        Vector3 orig = transform.position;

        float elapseT = 0.0f;

        while(elapseT < dur)
        {
            float x = Random.Range(-1f, 1f) * mag;
            float y = Random.Range(-1f, 1f) * mag;

            transform.localPosition = new Vector3(x, y, orig.z);
            elapseT += Time.smoothDeltaTime;
            yield return null;
        }
        transform.localPosition = orig;

    }
}
