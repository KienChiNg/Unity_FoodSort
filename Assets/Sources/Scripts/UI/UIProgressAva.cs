using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressAva : MonoBehaviour
{
    [SerializeField] private Image _badgeRound;
    [SerializeField] private Image _avatar;
    [SerializeField] private TMP_Text _level;
    [SerializeField] private TMP_Text _nameAva;
    [SerializeField] private GameObject _vSyntax;
    [SerializeField] private List<Sprite> _badgeRoundList;

    public void SetHiddenAvatar(Sprite hiddenAva, int levelStart)
    {
        _avatar.sprite = hiddenAva;
        _badgeRound.sprite = _badgeRoundList[1];
        _level.text = levelStart.ToString();
        _nameAva.text = "?????";
        _vSyntax.SetActive(false);
    }
    public void SetAvatar(Sprite ava, int levelStart, string nameAva)
    {
        _avatar.sprite = ava;
        _badgeRound.sprite = _badgeRoundList[0];
        _level.text = levelStart.ToString();
        _nameAva.text = nameAva;
        _vSyntax.SetActive(true);
    }
}
