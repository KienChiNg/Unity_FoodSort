using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace FoodSort
{
	public class Pointer : MonoBehaviour
	{
		private const float TIME_ANIM_FADE = 0.15f;

		[SerializeField] private Animator _anim;
		[SerializeField] private SpriteRenderer _backGround;

		private Tween _tweenBackground;
		public async Task OnFade()
		{
			Color color = _backGround.color;
			color.a = 0;
			_backGround.color = color;

			_tweenBackground?.Kill();
			_tweenBackground = _backGround.DOFade(1, TIME_ANIM_FADE);

			await _tweenBackground.AsyncWaitForCompletion();
		}
		public async Task OffFade()
		{
			if (_backGround == null) return;

			Color color = _backGround.color;
			color.a = 1;
			_backGround.color = color;

			_tweenBackground?.Kill();
			_tweenBackground = _backGround.DOFade(0, TIME_ANIM_FADE);

			await _tweenBackground.AsyncWaitForCompletion();
		}
	}
}
