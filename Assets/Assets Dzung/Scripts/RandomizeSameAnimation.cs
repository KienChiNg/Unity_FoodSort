using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeSameAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetFloat("Speed", Random.Range(0.3f, 1f) );
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
