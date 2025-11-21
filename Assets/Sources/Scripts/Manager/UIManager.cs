using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FoodSort
{
	public class UIManager : MonoBehaviour
	{
		public static UIManager Instance;
		private const float ALPHA_ONENABLE = 1f;
		private const float TIME_ANIM_FADE = 0.8f;

		[SerializeField] private TMP_Text _coin;
		[SerializeField] private GameObject _coinGO;
		[SerializeField] private UIHeader _uIHeader;
		[SerializeField] private GameObject _uIHeaderGameplay;
		[SerializeField] private GameObject _uIMainBGGameplay;
		[SerializeField] private Transform _stoveStorage;
		[SerializeField] private RawImage _firer;

		private GameManager _gameManager;

		private Tween _tweenFire;
		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			_gameManager = GameManager.Instance;

			_gameManager.OnCoinChange += SetCoinValue;
		}
		public void ShowTexFirer()
		{
			_tweenFire?.Kill();
			_tweenFire = _firer.DOFade(ALPHA_ONENABLE, TIME_ANIM_FADE).OnComplete(() =>
			{
				_tweenFire = _firer.DOFade(0, TIME_ANIM_FADE);
			});
		}
		public void SetupSpecial()
		{
			_uIHeaderGameplay.SetActive(false);
			// _uIMainBGGameplay.transform.position = new Vector3(0, -1.7f, 0);
			// _stoveStorage.position = new Vector3(0, -1.55f, 0);
		}
		public void ShowCoin()
		{
			SetCoinValue(_gameManager.Coin);
			_uIHeader.gameObject.SetActive(false);
			_coinGO.SetActive(true);
		}
		public void HideCoin()
		{
			_uIHeader.gameObject.SetActive(true);
			_coinGO.SetActive(false);
		}
		public void SetCoinValue(int value)
		{
			_coin.text = value.ToString();
		}
	}
}
