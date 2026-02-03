using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using EugeneC.Singleton;
using EugeneC.Utilities;

namespace EugeneC.Obsolete
{
	/// <summary>
	/// Duplicate of Coroutine Manager but in async version
	/// </summary>
	/// <remarks>
	/// Obsolete, please use extensions from <see cref="UtilityMethods"/>>
	/// </remarks>
	public class AsyncManager : GenericSingleton<AsyncManager>
	{
		// Start is called before the first frame update
		void Start()
		{
			KeepSingleton(true);
		}

		public static async Task FadeScreenAsync(bool isFadein, Image fadeImage,
			float LoadDuration, Action OnDone = null)
		{
			if (fadeImage == null) return;

			float targetAlpha = isFadein ? 1f : 0f;
			float currentAlpha = fadeImage.color.a;
			float time = 0;

			while (time <= LoadDuration)
			{
				time += Time.unscaledDeltaTime;
				float alpha_ = Mathf.Lerp(currentAlpha, targetAlpha, time / LoadDuration);
				fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha_);

				// same as wait til next frame
				await Task.Yield();
			}

			fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
			OnDone?.Invoke();
		}

		public static async Task ChangeColorAsync(Image _image, Color target,
			float LoadDuration, Action OnDone = null)
		{
			if (_image == null) return;

			Color CurrentColor = _image.color;
			float time = 0;

			while (time <= LoadDuration)
			{
				time += Time.unscaledDeltaTime;
				Color col = Color.Lerp(CurrentColor, target, time / LoadDuration);
				_image.color = col;
				await Task.Yield();
			}

			_image.color = target;
			OnDone?.Invoke();
		}

		public static async Task MoveTransformAsync(GameObject obj, Transform targetPos,
			float moveDuration, Action onDone = null)
		{
			if (obj == null || targetPos == null) return;

			float time = 0;
			Vector3 startPos = obj.transform.position;
			Vector3 endPos = targetPos.position;

			while (time < moveDuration)
			{
				time += Time.unscaledDeltaTime;
				Vector3 Pos = Vector3.Lerp(startPos, endPos, time / moveDuration);
				obj.transform.position = Pos;
				await Task.Yield();
			}

			obj.transform.position = endPos;
			onDone?.Invoke();
		}

		public static async Task MoveVectorAsync(GameObject obj, Vector3 targetPos,
			float moveDuration, Action onDone = null)
		{
			if (obj == null) return;

			float time = 0;
			Vector3 startPos = obj.transform.position;
			Vector3 endPos = targetPos;

			while (time < moveDuration)
			{
				time += Time.unscaledDeltaTime;
				Vector3 Pos = Vector3.Lerp(startPos, endPos, time / moveDuration);
				obj.transform.position = Pos;
				await Task.Yield();
			}

			obj.transform.position = endPos;
			onDone?.Invoke();
		}

		public static async Task RotateObjectAsync(GameObject obj, Vector3 rotateTo,
			float rotateDuration, Action onDone = null)
		{
			if (obj == null) return;

			float time = 0f;
			Quaternion startRot = obj.transform.rotation;
			Quaternion endRot = Quaternion.Euler(rotateTo);

			while (time < rotateDuration)
			{
				time += Time.unscaledDeltaTime;
				obj.transform.rotation = Quaternion.Slerp(startRot, endRot, time / rotateDuration);
				await Task.Yield();
			}

			obj.transform.rotation = endRot;
			onDone?.Invoke();
		}

		public static async Task ScaleObjectAsync(GameObject obj, Vector3 scaleTo,
			float scalingDuration, Action onDone = null)
		{
			if (obj == null) return;

			float time = 0f;
			Vector3 StartScale = obj.transform.localScale;
			Vector3 EndScale = scaleTo;

			while (time < scalingDuration)
			{
				time += Time.unscaledDeltaTime;
				obj.transform.localScale = Vector3.Lerp(StartScale, EndScale, time / scalingDuration);
				await Task.Yield();
			}

			obj.transform.localScale = EndScale;
			onDone?.Invoke();
		}

		public static async Task DialogueAsync(List<string> dialogueList, float dialogueDuration,
			Action<string> displayTo, float timePerChar, Action onDone = null)
		{
			if (dialogueList == null || displayTo == null) return;

			foreach (string line in dialogueList)
			{
				if (line == null) continue;

				float timer = 0f;
				string textToDisplay = line;
				string currentDisplaying = "";

				while (currentDisplaying != textToDisplay)
				{
					timer += Time.deltaTime;
					int length = Mathf.CeilToInt(timer / timePerChar);
					length = Mathf.Clamp(length, 0, textToDisplay.Length);
					currentDisplaying = textToDisplay.Substring(0, length);
					displayTo(currentDisplaying);
					await Task.Yield();
				}

				await Task.Delay(TimeSpan.FromSeconds(dialogueDuration));
			}

			displayTo("");
			onDone?.Invoke();
		}

		public static async Task RollRightAngleAsync(Transform ob, float rollspeed,
			Vector3 dir, float rollcooldown = .1f)
		{
			Vector3 anchor = ob.position + (Vector3.down + dir) * 0.5f;
			Vector3 axis = Vector3.Cross(Vector3.up, dir);

			for (int i = 0; i <= (90 / rollspeed); i++)
			{
				ob.RotateAround(anchor, axis, i);
				await Task.Delay(TimeSpan.FromSeconds(rollcooldown));
			}
		}

		public static async Task TimeScaleAsync(float TargetScale = 0f,
			float LoadDuration = 2f, Action OnDone = null)
		{
			float CurrentScale = Time.timeScale;
			float unscaledTimer = 0;
			await Task.Yield();

			while (unscaledTimer <= LoadDuration)
			{
				unscaledTimer += Time.unscaledDeltaTime;
				float t = Mathf.Lerp(CurrentScale, TargetScale, unscaledTimer / LoadDuration);
				Time.timeScale = t;
				await Task.Yield();
			}

			OnDone?.Invoke();
		}
	}
}