using System.Collections;
using System.Collections.Generic;
using Solo.MOST_IN_ONE;
using UnityEngine;

namespace FoodSort
{
	public class SoundManager : Singleton<SoundManager>
	{
		private const float DELAY_BETWEEN_CLIPS = 0.3f;
		private const float DELAY_FADE_DURATION = 0.5f;
		[SerializeField] private AudioSource _audioSource;
		[SerializeField] private AudioSource _audioSourceBackground;
		[SerializeField] private AudioSource _audioSourceMusic;
		public List<AudioClip> _audioBackground;
		public AudioClip _audioBackgroundHome;
		// public AudioClip musicBackgroundHome;
		public AudioClip musicBackgroundGameplay;
		public AudioClip audioSceneStart;
		public AudioClip audioDishMove;
		public AudioClip audioFoodSelect;
		public AudioClip audioFoodMove;
		public AudioClip audioStoveCover;
		public List<AudioClip> audioStoveSparks;
		public AudioClip audioSkewerDone;
		public AudioClip audioWin;
		public AudioClip audioLose;
		public AudioClip audioBtnClick;
		public AudioClip audioUseBooster;
		public HapticsExample hapticsExample;

		private LevelManager _levelManager;
		private float _isVolume = 1;
		private float _isVolumeMusic = 1;
		private bool _isMute;
		private bool _isMuteMusic;
		private bool _isHaptic = true;
		private bool _isPlay;

		public bool IsMute { get => _isMute; set => _isMute = value; }
		public bool IsHaptic { get => _isHaptic; set => _isHaptic = value; }
		public bool IsMuteMusic { get => _isMuteMusic; set => _isMuteMusic = value; }
		public AudioSource AudioSource { get => _audioSource; set => _audioSource = value; }

		void Awake()
		{
			_audioSource = GetComponent<AudioSource>();
			_levelManager = LevelManager.Instance;

			if (PlayerPrefs.GetInt(Consts.MUSIC_FOODSORT, 1) == 1)
				_isMuteMusic = false;
			else
				_isMuteMusic = true;

			if (PlayerPrefs.GetInt(Consts.SOUND_FOODSORT, 1) == 1)
				_isMute = false;
			else
				_isMute = true;

			if (PlayerPrefs.GetInt(Consts.HAPTIC_FOODSORT, 1) == 1)
				_isHaptic = false;
			else
				_isHaptic = true;

			if (_isMute) HandleVolume(0);
			if (_isMuteMusic) HandleVolumeMusic(0);
		}
		public void Play(string scene, AudioClip musicBackgroundHome = null)
		{
			StartCoroutine(PlayRandomLoop(scene, musicBackgroundHome));
		}
		public void HandleVolume(float volume)
		{
			_isVolume = volume;
			_audioSourceBackground.volume = volume;
			// _audioSourceMusic.volume = volume;
		}
		public void HandleVolumeMusic(float volume)
		{
			_isVolumeMusic = volume;
			// _audioSourceBackground.volume = volume;
			_audioSourceMusic.volume = volume;
		}
		IEnumerator PlayRandomLoop(string scene, AudioClip musicBackgroundHome)
		{
			_audioSourceBackground.volume = _isVolume;
			_audioSourceMusic.volume = _isVolumeMusic;

			_audioSourceMusic.clip = scene == Consts.SCENE_HOME ? musicBackgroundHome : musicBackgroundGameplay;
			_audioSourceMusic.Play();
			while (true)
			{
				int index = Random.Range(0, _audioBackground.Count);
				_audioSourceBackground.clip = scene == Consts.SCENE_HOME ? _audioBackgroundHome : _audioBackground[index];
				_audioSourceBackground.Play();

				yield return new WaitForSeconds(_audioSourceBackground.clip.length + DELAY_BETWEEN_CLIPS);
			}
		}
		IEnumerator FadeOutCoroutine(float delayFadeDuration)
		{
			float startVolume = _audioSourceBackground.volume;
			float startVolumeMusic = _audioSourceMusic.volume;

			while (_audioSourceBackground.volume > 0 && _audioSourceMusic.volume > 0)
			{
				_audioSourceBackground.volume -= startVolume * Time.deltaTime / delayFadeDuration;
				_audioSourceMusic.volume -= startVolumeMusic * Time.deltaTime / delayFadeDuration;
				yield return null;
			}

			_audioSourceBackground.Stop();
			_audioSourceMusic.Stop();
		}
		public void TurnOffMusicBackground(float delayFadeDuration)
		{
			StartCoroutine(FadeOutCoroutine(delayFadeDuration));
		}
		public void PlaySceneStart()
		{
			_audioSource.PlayOneShot(audioSceneStart, _isVolume);
		}
		public void PlayFoodSelect()
		{
			_audioSource.PlayOneShot(audioFoodSelect, _isVolume);

			HapticPlay();
		}
		public void PlayFoodMove()
		{
			_audioSource.PlayOneShot(audioFoodMove, _isVolume * 0.5f);

			HapticPlay();
		}
		public void PlaySkewerDone()
		{
			_audioSource.PlayOneShot(audioSkewerDone, _isVolume);
		}
		public void PlayDishMove()
		{
			_audioSource.PlayOneShot(audioDishMove, _isVolume);
		}
		public void PlayStoveCover()
		{
			_audioSource.PlayOneShot(audioStoveCover, _isVolume);
		}
		public void PlayStoveSpark(int inx)
		{
			_audioSource.PlayOneShot(audioStoveSparks[inx], _isVolume * 0.45f);
		}
		public void PlayWin()
		{
			_audioSource.PlayOneShot(audioWin, _isVolume);
		}
		public void PlayLose()
		{
			_audioSource.PlayOneShot(audioLose, _isVolume);
		}
		public void PlayBtnClick()
		{
			_audioSource.PlayOneShot(audioBtnClick, _isVolume);
		}
		public void PlayUseBooster()
		{
			_audioSource.PlayOneShot(audioUseBooster, _isVolume);
		}
		public void HapticPlay()
		{
			if (!IsHaptic)
				hapticsExample.LightImpactHaptic();
		}
	}
}
