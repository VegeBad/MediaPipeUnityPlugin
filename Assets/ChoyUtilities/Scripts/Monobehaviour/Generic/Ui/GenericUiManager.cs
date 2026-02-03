using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EugeneC.Mono;
using Unity.Mathematics;
using UnityEngine;

namespace EugeneC.Singleton
{
	public abstract class GenericUiManager<T, U> : GenericSingleton<U>
		where T : Enum
		where U : MonoBehaviour
	{
		[Serializable]
		public struct UiSerialize
		{
			public T id;
			public UiHelper prefab;
		}

		[SerializeField] protected Canvas canvasRef;
		[SerializeField] protected UiSerialize[] uiElements;

		protected UiHelper[] UiObjects;
		protected RectTransform CanvasPos;
		protected readonly List<UiHelper> OpenedUi = new();
		protected bool IsTransitioning;
		protected CancellationTokenSource TokenSource;

		public void CancelTask() => TokenSource.Cancel();
		public event Action OnOpenUi;
		public event Action OnCloseUi;

		protected virtual async void Start()
		{
			try
			{
				await Awaitable.NextFrameAsync();
				if (canvasRef is null) return;
				CanvasPos = (RectTransform)canvasRef.transform;

				UiObjects = new UiHelper[uiElements.Length];
				for (var i = 0; i < uiElements.Length; i++)
				{
					if (uiElements[i].prefab is null) continue;
					var spawn = Instantiate(uiElements[i].prefab, CanvasPos);
					spawn.OnSpawn();
					UiObjects[i] = spawn;
					spawn.gameObject.SetActive(false);
				}
			}
			catch (Exception e)
			{
				print(e);
			}
		}

		private async void OnDisable()
		{
			try
			{
				await CloseAll();
			}
			catch (Exception e)
			{
				print(e);
			}
		}

		public virtual async Task<(UiHelper, bool)> Open(T id)
		{
			if (!Enum.IsDefined(typeof(T), id)) return (null, false);
			var index = Array.FindIndex(uiElements, i => EqualityComparer<T>.Default.Equals(i.id, id));

			IsTransitioning = true;
			var newUi = UiObjects[index];
			newUi.gameObject.SetActive(true);
			OpenedUi.Add(newUi);

			OnOpenUi?.Invoke();
			var t = newUi.OnStartOpen();
			await Awaitable.WaitForSecondsAsync(math.abs(t));
			newUi.OnEndOpen();
			IsTransitioning = false;

			return (newUi, true);
		}

		public virtual async Task<(UiHelper, bool)> Close(T id, float time)
		{
			if (!Enum.IsDefined(typeof(T), id)) return (null, false);
			var index = Array.FindIndex(uiElements, i => EqualityComparer<T>.Default.Equals(i.id, id));

			IsTransitioning = true;
			var newUi = UiObjects[index];

			OnCloseUi?.Invoke();
			var t = newUi.OnStartClose();
			await Awaitable.WaitForSecondsAsync(math.abs(t));
			newUi.OnEndClose();

			await Awaitable.NextFrameAsync();
			newUi.gameObject.SetActive(false);
			OpenedUi.Remove(newUi);
			IsTransitioning = false;

			return (newUi, true);
		}

		public virtual async Task<bool> CloseAll()
		{
			IsTransitioning = true;
			var i = 0f;
			foreach (var ui in OpenedUi)
			{
				OnCloseUi?.Invoke();
				var t = ui.OnStartClose();
				//Get the highest value
				i = i < t ? t : i;
			}

			await Awaitable.WaitForSecondsAsync(i);

			foreach (var ui in OpenedUi)
			{
				ui.OnEndClose();
				await Awaitable.NextFrameAsync();
				ui.gameObject.SetActive(false);
			}

			IsTransitioning = false;
			OpenedUi.Clear();
			return true;
		}

		public virtual async Task<bool> Replace(T id)
		{
			if (!Enum.IsDefined(typeof(T), id)) return false;
			var c = await CloseAll();
			if (!c) return false;
			await Open(id);
			return true;
		}
	}
}