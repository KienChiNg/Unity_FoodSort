using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FoodSort
{
    public class UIBottomBarElement : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private GameObject _mainContent;
        [SerializeField] private GameObject _background;
        [SerializeField] private GameObject _icon;
        [SerializeField] private TMP_Text _txtEle;
        [SerializeField] private bool _isSelect;
        [SerializeField] private bool _isSelectHome;

        private Vector3 _iconPosInit;

        private Tween _tweenIconMoveY;
        private Tween _tweenIconScale;
        private Tween _tweenFadeText;
        void Start()
        {
            _iconPosInit = _icon.transform.localPosition;
            if (_isSelect) Select();
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            Select();
        }

        public void Select()
        {
            UIBottomBar.Instance.VfxHome.SetActive(_isSelectHome);

            UIBottomBar.Instance.UnSelectAll();
            _isSelect = true;

            _background.SetActive(true);
            if (_mainContent)
                _mainContent.SetActive(true);

            _tweenIconMoveY?.Kill();
            _tweenFadeText?.Kill();
            _tweenIconScale?.Kill();

            _tweenIconMoveY = _icon.transform.DOLocalMoveY(_iconPosInit.y + 80, 0.5f);
            _tweenIconScale = _icon.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f);
            _tweenFadeText = _txtEle.DOFade(1, 0.3f);
        }
        public void UnSelect()
        {
            if (!_isSelect) return;
            _isSelect = false;

            _background.SetActive(false);
            if (_mainContent)
                _mainContent.SetActive(false);

            _tweenIconMoveY?.Kill();
            _tweenFadeText?.Kill();
            _tweenIconScale?.Kill();

            _tweenIconMoveY = _icon.transform.DOLocalMoveY(_iconPosInit.y, 0.3f);
            _tweenIconScale = _icon.transform.DOScale(new Vector3(1, 1, 1), 0.3f);
            _tweenFadeText = _txtEle.DOFade(0, 0.2f);
        }
    }
}
