using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FoodSort
{
	public class Stove : MonoBehaviour
	{
		private const float TIME_MIN_DELAY = 5;
		private const float TIME_MAX_DELAY = 20;
		private const float TIME_ANIM_MOVE = 0.8f;
		private const float TIME_ANIM_SCALE = 0.1f;
		private const float TIME_ANIM_MOVE_STOVE_COVER = 0.2f;
		private const float SIZE_STOVE = 1f;
		private const float POS_Y_INIT_SKEWER_SPAWN = -0.5f;
		private const float ROTATION_Z_INIT_SKEWER_SPAWN = -15;

		[SerializeField] private SpriteRenderer _stoveCover;
		[SerializeField] private GameObject _bgNormal;
		[SerializeField] private GameObject _bgSpecial6;
		[SerializeField] private GameObject _bgSpecial8;
		[SerializeField] private Transform _skewerStorage;
		[SerializeField] private Skewer _skewerTemple;
		[SerializeField] private List<ParticleSystem> _sparkLight;
		[SerializeField] private ParticleSystem _closeCover;

		private LevelManager _levelManager;
		private SoundManager _soundManager;

		private Skewer _skewerOnStove;

		private Tween _tweenMove;
		private Tween _tweenScale;
		private Tween _tweenStoveCover;

		private StoveData _stoveData;

		private int _inxStove;

		public Skewer SkewerOnStove { get => _skewerOnStove; set => _skewerOnStove = value; }
		public Transform SkewerStorage { get => _skewerStorage; set => _skewerStorage = value; }

		void Awake()
		{
			_soundManager = SoundManager.Instance;
			_levelManager = LevelManager.Instance;
		}
		void Start()
		{
			StartCoroutine(CallRandomly());

		}

		private IEnumerator CallRandomly()
		{
			while (true)
			{
				float waitTime = UnityEngine.Random.Range(TIME_MIN_DELAY, TIME_MAX_DELAY);

				yield return new WaitForSeconds(waitTime);

				if (!_levelManager.FullStoveSmokeLight() && _skewerOnStove && !_skewerOnStove.OnPlate && !_skewerOnStove.CheckEmptySlotOnSkewer())
					PlayVFXSmokeLight();
			}
		}
		public void SetupStove(Vector2 des, int inx, StoveData stoveData)
		{
			// if (_levelManager.IsLevelSpecial)
			// {
			// 	_bgNormal.SetActive(false);
			// 	if (_levelManager.SkekwerFoodCount == 8)
			// 		_bgSpecial8.SetActive(true);
			// 	else
			// 		_bgSpecial6.SetActive(true);
			// }

			this._inxStove = inx;
			this._stoveData = stoveData;

			CalInitPosStove(des);

			List<Func<Task>> actions = new List<Func<Task>>();

			actions.Add(() => StoveMove(des));
			// actions.Add(() => StoveScale(new Vector2(0.9f, 1f)));
			actions.Add(() => CreateSkewer());

			RunSequence(actions);
		}
		//* Hàm tính toán giá trị ban đầu
		void CalInitPosStove(Vector2 targetPoint)
		{
			float halfHeight = Camera.main.orthographicSize; //*Chiều dài tình từ tâm
			float halfWidth = halfHeight * Camera.main.aspect; //*Aspect màn

			float edgeScene = Camera.main.transform.position.x + halfWidth;

			float xInitStove = (edgeScene + SIZE_STOVE) * (targetPoint.x >= 0 ? 1 : -1);

			this.transform.position = new Vector3(xInitStove, targetPoint.y, 0);
		}
		async void PlayVFXSmokeLight()
		{
			_levelManager.HandleNumStoveSmokeLight(true);

			int inx = Random.Range(0, _sparkLight.Count);
			_soundManager.PlayStoveSpark(inx);
			await PlayVFX(inx);

			_levelManager.HandleNumStoveSmokeLight(false);
		}
		public async Task PlayVFX(int inx)
		{
			_sparkLight[inx].Stop();
			_sparkLight[inx].Play();
			await WaitForVFX(_sparkLight[inx]);
		}
		private async Task WaitForVFX(ParticleSystem ps)
		{
			while (ps != null && ps.IsAlive(true))
				await Task.Yield();
		}
		//*Trình chạy anim
		async void RunSequence(List<Func<Task>> actions)
		{
			foreach (var act in actions)
			{
				await act();
			}
		}
		public async Task CloseLid()
		{
			_tweenStoveCover?.Kill();
			Transform stoveCoverTF = _stoveCover.gameObject.transform;
			_stoveCover.gameObject.SetActive(true);
			stoveCoverTF.localPosition = new Vector3(0, 2, 0);
			Color color = _stoveCover.color;
			color.a = 0;
			_stoveCover.color = color;

			_stoveCover.DOFade(1f, TIME_ANIM_MOVE_STOVE_COVER);
			_tweenStoveCover = stoveCoverTF.DOLocalMove(_skewerStorage.localPosition, TIME_ANIM_MOVE_STOVE_COVER)
											.SetEase(Ease.Linear)
											.OnComplete(() =>
											{
												_soundManager.PlayStoveCover();
												_closeCover?.Stop();
												_closeCover?.Play();
												_ = StoveCoverScale();
											});
			await _tweenStoveCover.AsyncWaitForCompletion();
		}
		public async Task OpenLid()
		{
			_tweenStoveCover?.Kill();
			Transform stoveCoverTF = _stoveCover.gameObject.transform;
			_stoveCover.gameObject.SetActive(true);
			stoveCoverTF.localPosition = _skewerStorage.localPosition;
			Color color = _stoveCover.color;
			color.a = 1;
			_stoveCover.color = color;

			_stoveCover.DOFade(0, TIME_ANIM_MOVE_STOVE_COVER);
			_tweenStoveCover = stoveCoverTF.DOLocalMove(new Vector3(0, 2, 0), TIME_ANIM_MOVE_STOVE_COVER)
											.SetEase(Ease.Linear)
											.OnComplete(() =>
											{
												_stoveCover.gameObject.SetActive(false);
												// _closeCover?.Stop();
												// _closeCover?.Play();
												// _ = StoveCoverScale();
											});
			await _tweenStoveCover.AsyncWaitForCompletion();
		}
		public async Task StoveMove(Vector2 targetPoint)
		{
			_tweenMove?.Kill();
			_tweenMove = this.transform.DOMove(new Vector3(targetPoint.x, targetPoint.y, 0f), TIME_ANIM_MOVE);

			await _tweenMove.AsyncWaitForCompletion();
		}
		async Task StoveScale(Vector2 targetScale)
		{
			_tweenScale?.Kill();
			_tweenScale = this.transform.DOScale(new Vector3(targetScale.x, targetScale.y, 0), TIME_ANIM_SCALE).SetEase(Ease.OutBack);

			await _tweenScale.AsyncWaitForCompletion();
		}
		async Task StoveCoverScale()
		{
			await StoveScale(new Vector3(1.2f, 0.9f, 0));
			await StoveScale(new Vector3(0.9f, 1.1f, 0));
			await StoveScale(Vector3.one);
		}
		async Task CreateSkewer()
		{
			Skewer skewer = Instantiate(_skewerTemple, _skewerStorage);
			_skewerOnStove = skewer;
			skewer.SetupSkewer(_levelManager.SkekwerFoodCount, _stoveData.skewerData, this, _inxStove);
			skewer.transform.localPosition = new Vector3(0, POS_Y_INIT_SKEWER_SPAWN, 0);
			skewer.transform.localRotation = Quaternion.Euler(0, 0, ROTATION_Z_INIT_SKEWER_SPAWN);

			await Task.CompletedTask;
		}
		public async Task AfterWinGame()
		{
			if (!_skewerOnStove.IsOnPlate())
			{
				await _skewerOnStove.SkeweFade(0);
				await CloseLid();
			}
		}
		public bool HaveSkewerInStove()
		{
			return _skewerOnStove != null;
		}
		public void AfterSkewerDone()
		{
			_skewerOnStove = null;
		}
	}
}
