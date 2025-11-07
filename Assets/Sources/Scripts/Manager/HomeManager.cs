using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace FoodSort
{
    public class HomeManager : MonoBehaviour
    {
        private const float SLIDER_WIDTH_MAX = 650;
        private const float TIME_DEFAULT_ANIM_LOADING = 2f;
        private const float TIME_STOP_ANIM_LOADING = 0.05f;
        public static HomeManager Instance;
        [SerializeField] private UIClick _uIClickPlay;
        [SerializeField] private Image _backgroundLoading;
        [SerializeField] private RectTransform _slider;
        [SerializeField] private RectTransform _bottomBar;
        #region LOADING
        [SerializeField] private GameObject _loading;
        [SerializeField] private Transform _iconLoading;
        [SerializeField] private LoadingAnim _loadingAnim;
        #endregion
        [SerializeField] private TMP_Text _levelRank;
        [SerializeField] private TMP_Text _levelBtn;
        [SerializeField] private Transform _textNoticeTF;

        private Tween _tweenFade;
        private Tween _tweenScale;
        private Tween _tweenText;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

        }
        void Start()
        {
            SoundManager.Instance.Play(Consts.SCENE_HOME);
            _uIClickPlay.ActionAfterClick += Play;

            if (MaxMediationController.instance.DisplayedBanner)
            {
                _bottomBar.anchoredPosition = new Vector3(0, 280, 0);
            }
            LoadLevelDisplayRank();
        }
        public void LoadLevelDisplayRank()
        {
            int level = PlayerPrefs.GetInt(Consts.LEVEL_SAVE, 1);
            int levelSlider = Mathf.Clamp(level, 0, 100);


            _levelBtn.text = $"Level {level}";
            _levelRank.text = $"{levelSlider}/100";

            float preValue = SLIDER_WIDTH_MAX * levelSlider / 100;
            Vector2 sizeX = _slider.sizeDelta;
            sizeX.x = preValue;
            _slider.sizeDelta = sizeX;
        }
        public async void Play()
        {
            SoundManager.Instance.TurnOffMusicBackground(1);

            _loading.SetActive(true);

            Color c = _backgroundLoading.color;
            c.a = 0;
            _backgroundLoading.color = c;

            _iconLoading.localScale = Vector3.zero;

            _tweenFade?.Kill();
            _tweenScale?.Kill();

            _tweenFade = _backgroundLoading.DOFade(1, 0.3f);
            _tweenScale = _iconLoading.DOScale(Vector3.one, 0.3f);
            _uIClickPlay.ActionAfterClick -= Play;
            await Task.WhenAll(
                _tweenFade.AsyncWaitForCompletion(),
                _tweenScale.AsyncWaitForCompletion()
            );

            LoadSceneWithLoading(Consts.SCENE_GAMEPLAY);
        }
        public void SetAnimTextNotice()
        {
            _tweenText?.Kill();
            _textNoticeTF.gameObject.SetActive(true);
            _textNoticeTF.localScale = new Vector3(1, 0, 0);
            _tweenText = _textNoticeTF.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    _tweenText = DOVirtual.DelayedCall(0.5f, () =>
                    {
                        _tweenText = _textNoticeTF.DOScale(new Vector3(1, 0, 0), 0.5f)
                            .SetEase(Ease.InBack)
                            .OnComplete(() =>
                            {
                                _textNoticeTF.localScale = Vector3.one;
                                _textNoticeTF.gameObject.SetActive(false);
                            });

                    });
                });
        }
        public void LoadSceneWithLoading(string sceneName)
        {
            StartCoroutine(LoadAsync(sceneName));
        }

        private IEnumerator LoadAsync(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);

                if (progress >= 1f)
                {
                    yield return new WaitForSeconds(TIME_DEFAULT_ANIM_LOADING);
                    // _loadingAnim?.ChangeStateAnim(false);
                    // yield return new WaitForSeconds(TIME_STOP_ANIM_LOADING);
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }
}
