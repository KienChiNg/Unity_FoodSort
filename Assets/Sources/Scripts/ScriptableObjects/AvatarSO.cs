using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarSO", menuName = "ScriptableObjects/AvatarSO", order = 2)]
public class AvatarSO : ScriptableObject
{
    public int levelStart;
    public int levelEnd;
    public Sprite avatar;
    public Sprite hiddenAvatar;
    public AudioClip audioClipBG;
    public string avaName;
}
