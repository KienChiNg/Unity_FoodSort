using UnityEngine;

namespace FoodSort
{
	public class UICoin : MonoBehaviour
	{
		private SoundManager _soundManager;
		public AudioClip _audioCoinCollect;
		void Awake()
		{
			_soundManager = SoundManager.Instance;
		}
		public void PlayCoinCollect()
		{
			if (!_soundManager.IsMute)
				_soundManager.AudioSource.PlayOneShot(_audioCoinCollect, 0.6f);
		}
	}
}
