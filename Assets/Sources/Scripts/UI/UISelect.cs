using DG.Tweening;
using UnityEngine;

namespace FoodSort
{
	public class UISelect : MonoBehaviour
	{
		private Tween _tween;
		private SoundManager _soundManager;
		void Awake()
		{
			_soundManager = SoundManager.Instance;
		}
		public void OnClick()
		{
			_tween?.Kill();
			SoundManager.Instance.PlayBtnClick();
			transform.localScale = Vector3.one;
			_tween = transform.DOScale(Vector3.one * 0.9f, 0.2f)
				.SetEase(Ease.OutBack)
				.OnComplete(() =>
				{
					transform.DOScale(Vector3.one, 0.1f);
				});
		}
	}
}
