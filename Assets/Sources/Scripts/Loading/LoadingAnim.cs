using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingAnim : MonoBehaviour
{
    public Animator anim;
    void Start()
    {
        ChangeStateAnim(true);
    }
    public void ChangeStateAnim(bool state)
    {
        anim.SetBool("isOn", state);
    }
}
