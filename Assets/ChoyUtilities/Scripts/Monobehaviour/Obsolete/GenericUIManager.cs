using System;
using System.Collections.Generic;
using EugeneC.Singleton;
using UnityEngine;

namespace EugeneC.Obsolete
{
	public abstract class GenericUIManager<T> : GenericSingleton<GenericUIManager<T>>
		where T : Enum
	{
		public Canvas canvasPrefab;
		public UISerialize<T>[] uiPrefab;

		RectTransform _canvasPos;
		Dictionary<T, UISerialize<T>> _uiDictionary = new();
		List<GameObject> _openedUI = new();

		protected override void Awake()
		{
			base.Awake();
			_canvasPos = (RectTransform)Instantiate(canvasPrefab).transform;

			foreach (var prefab in uiPrefab)
				_uiDictionary[prefab.uiId] = prefab;
		}

		private GameObject GeneratePrefab(T prefab)
		{
			GameObject newUI = null;

			if (_uiDictionary.TryGetValue(prefab, out UISerialize<T> template))
				newUI = Instantiate(template.prefab, _canvasPos);
			else
				throw new Exception(prefab + " couldn't be found");

			return newUI;
		}

		public GameObject Open(T prefab)
		{
			GameObject newUI = GeneratePrefab(prefab);
			if (newUI != null) _openedUI.Add(newUI);
			return newUI;
		}

		public GameObject Replace(T prefab)
		{
			CloseAll();
			return Open(prefab);
		}

		public void Close(GameObject prefab)
		{
			_openedUI.Remove(prefab);
			Destroy(prefab);
		}

		public void CloseAll()
		{
			foreach (var prefab in _openedUI.ToArray())
				Close(prefab);
		}
	}

	[Serializable]
	public struct UISerialize<T>
		where T : Enum
	{
		public T uiId;
		public GameObject prefab;
	}
}