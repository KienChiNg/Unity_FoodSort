using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailSmoother : MonoBehaviour
{
    public Transform target; // object thật
    public float sampleRate = 0.005f; // càng nhỏ càng mượt
    private TrailRenderer trail;

    void Start()
    {
        trail = GetComponent<TrailRenderer>();
        StartCoroutine(SampleTrail());
    }

    System.Collections.IEnumerator SampleTrail()
    {
        while (true)
        {
            transform.position = target.position; // ép theo dõi
            yield return new WaitForSecondsRealtime(sampleRate);
        }
    }
}
