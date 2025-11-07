using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FoodSort
{
	public class Loading : Singleton<Loading>
	{
		private const float TIME_DEFAULT_ANIM_LOADING = 2f;
		private const float TIME_STOP_ANIM_LOADING = 0.2f;
		private LoadingAnim _loadingAnim;
		void Awake()
		{
			_loadingAnim = FindObjectOfType<LoadingAnim>();
		}
		public void LoadSceneWithLoading(string sceneName)
		{
			StartCoroutine(LoadAsync(sceneName));
		}

		private IEnumerator LoadAsync(string sceneName)
		{
			var firebaseTask = AnalyticHandle.Instance.InitializeFireBase();
			while (!firebaseTask.IsCompleted) yield return null;

			AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

			operation.allowSceneActivation = false;

			while (!operation.isDone)
			{
				float progress = Mathf.Clamp01(operation.progress / 0.9f);

				if (progress >= 1f)
				{
					yield return new WaitForSeconds(TIME_DEFAULT_ANIM_LOADING);
					_loadingAnim?.ChangeStateAnim(false);
					yield return new WaitForSeconds(TIME_STOP_ANIM_LOADING);
					operation.allowSceneActivation = true;
				}

				yield return null;
			}
		}
	}
}
