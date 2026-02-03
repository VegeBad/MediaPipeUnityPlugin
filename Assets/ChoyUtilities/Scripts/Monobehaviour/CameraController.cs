using System;
using System.Threading;
using System.Threading.Tasks;
using EugeneC.Singleton;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EugeneC.Utilities
{
	[DisallowMultipleComponent]
	public sealed class CameraController : GenericSingleton<CameraController>
	{
		[SerializeField] private Image blackScreenImg;
		[SerializeField] private float initialFadeOutTime = 5f;

		public event Action OnCameraReady;

		private CancellationTokenSource _token = new CancellationTokenSource();
		public void CameraCancellation() => _token.Cancel();

		private async void OnEnable()
		{
			try
			{
				await Awaitable.WaitForSecondsAsync(3f, _token.Token);
				;
				await RunFadeScreen(UtilityCollection.EFadeType.FadeOut, initialFadeOutTime);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		public async Task RunFadeScreen(UtilityCollection.EFadeType fadeType, float duration)
		{
			await Awaitable.EndOfFrameAsync(_token.Token);
			await _token.Token.FadeScreenAsync(blackScreenImg, fadeType, duration, Time.deltaTime);
			OnCameraReady?.Invoke();
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(CameraController))]
	public class CameraTrackerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var instance = (CameraController)target;
			EditorGUILayout.HelpBox(
				"Currently in ECS you can't just attach the camera to a subscene entity and called it a day",
				MessageType.Info);
			EditorGUILayout.HelpBox("That's where this singleton comes into play", MessageType.Info);
			EditorGUILayout.HelpBox("Attach this to the camera object", MessageType.Info);
			EditorGUILayout.HelpBox("Don't put the camera in the subscene, put it in normal hierarchy",
				MessageType.Warning);
			EditorGUILayout.HelpBox(
				"Keep Singleton true or not doesn't matter, if true it will just override the other scene's camera",
				MessageType.Info);

			base.OnInspectorGUI();
		}
	}

#endif
}