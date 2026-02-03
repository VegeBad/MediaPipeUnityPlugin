using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EugeneC.Singleton;

namespace EugeneC.Obsolete
{
	public class CoroutineManager : GenericSingleton<CoroutineManager>
	{
		// Start is called before the first frame update
		void Start()
		{
			KeepSingleton(true);
		}

		public static IEnumerator FadeScreenCoroutine(bool isFadein, Image fadeImage,
			float LoadDuration, Action OnDone = null)
		{
			if (fadeImage == null) yield break;

			float targetAlpha = isFadein ? 1f : 0f;
			float currentAlpha = fadeImage.color.a;
			float time = 0;

			while (time <= LoadDuration)
			{
				time += Time.unscaledDeltaTime;
				float alpha_ = Mathf.Lerp(currentAlpha, targetAlpha, time / LoadDuration);
				fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha_);
				yield return null;
			}

			fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
			OnDone?.Invoke();
		}

		public static IEnumerator ChangeColorCoroutine(Image _image, Color target,
			float LoadDuration, Action OnDone = null)
		{
			if (_image == null) yield break;

			Color CurrentColor = _image.color;
			float time = 0;

			while (time <= LoadDuration)
			{
				time += Time.unscaledDeltaTime;
				Color col = Color.Lerp(CurrentColor, target, time / LoadDuration);
				CurrentColor = col;
			}

			_image.color = target;
			OnDone?.Invoke();
		}

		public static IEnumerator MoveTransformCoroutine(GameObject obj, Transform targetPos,
			float moveDuration, Action onDone = null)
		{
			if (obj == null || targetPos == null) yield break;

			float time = 0;
			Vector3 startPos = obj.transform.position;
			Vector3 endPos = targetPos.position;

			while (time < moveDuration)
			{
				time += Time.unscaledDeltaTime;
				Vector3 Pos = Vector3.Lerp(startPos, endPos, time / moveDuration);
				obj.transform.position = Pos;
				yield return null;
			}

			obj.transform.position = endPos;
			onDone?.Invoke();
		}

		public static IEnumerator MoveVectorCoroutine(GameObject obj, Vector3 targetPos,
			float moveDuration, Action onDone = null)
		{
			if (obj == null || targetPos == null) yield break;

			float time = 0;
			Vector3 startPos = obj.transform.position;
			Vector3 endPos = targetPos;

			while (time < moveDuration)
			{
				time += Time.unscaledDeltaTime;
				Vector3 Pos = Vector3.Lerp(startPos, endPos, time / moveDuration);
				obj.transform.position = Pos;
				yield return null;
			}

			obj.transform.position = endPos;
			onDone?.Invoke();
		}

		public static IEnumerator RotateObjectCoroutine(GameObject obj, Vector3 rotateTo,
			float rotateDuration, Action onDone = null)
		{
			if (obj == null) yield break;

			float time = 0f;
			Quaternion startRot = obj.transform.rotation;
			Quaternion endRot = Quaternion.Euler(rotateTo);

			while (time < rotateDuration)
			{
				time += Time.unscaledDeltaTime;
				obj.transform.rotation = Quaternion.Slerp(startRot, endRot, time / rotateDuration);
				yield return null;
			}

			obj.transform.rotation = endRot; // Ensure final rotation is set
			onDone?.Invoke();
		}

		public static IEnumerator ScaleObjectCoroutine(GameObject obj, Vector3 scaleTo,
			float scalingduration, Action onDone = null)
		{
			if (obj == null) yield break;
			float time = 0f;
			Vector3 StartScale = obj.transform.localScale;
			Vector3 EndScale = scaleTo;

			while (time < scalingduration)
			{
				time += Time.unscaledDeltaTime;
				obj.transform.localScale = Vector3.Lerp(StartScale, EndScale, time / scalingduration);
				yield return null;
			}

			obj.transform.localScale = EndScale;
			onDone?.Invoke();
		}

		public static IEnumerator DialogueCoroutine(List<string> dialogueList, float dialogueDuration,
			Action<string> displayTo, float timePerChar, Action onDone = null)
		{
			if (dialogueList == null || displayTo == null) yield break;

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

					yield return null;
				}

				yield return new WaitForSeconds(dialogueDuration);
			}

			displayTo("");
			onDone?.Invoke();
		}

		public static IEnumerator RollRightAngleCoroutine(Transform ob, float rollspeed,
			Vector3 dir, float rollcooldown = .1f)
		{
			Vector3 anchor = ob.position + (Vector3.down + dir) * 0.5f;
			Vector3 axis = Vector3.Cross(Vector3.up, dir);

			for (int i = 0; i <= (90 / rollspeed); i++)
			{
				ob.RotateAround(anchor, axis, i);
				yield return new WaitForSeconds(rollcooldown);
			}
		}

		public static IEnumerator TimeScaleCoroutine(float TargetScale = 0f,
			float LoadDuration = 2f, Action OnDone = null)
		{
			float CurrentScale = Time.timeScale;
			float unscaledTimer = 0;
			yield return new WaitForEndOfFrame();

			while (unscaledTimer <= LoadDuration)
			{
				unscaledTimer += Time.unscaledDeltaTime;
				float t = Mathf.Lerp(CurrentScale, TargetScale, unscaledTimer / LoadDuration);
				Time.timeScale = t;
				yield return new WaitForEndOfFrame();
			}

			OnDone?.Invoke();
		}
	}
}