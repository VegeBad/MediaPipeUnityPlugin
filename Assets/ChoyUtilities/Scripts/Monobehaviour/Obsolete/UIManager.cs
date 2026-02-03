using System;
using System.Collections.Generic;
using EugeneC.Singleton;
using UnityEngine;

namespace EugeneC.Obsolete
{
	public class UIManager : GenericSingleton<UIManager>
	{
		public UIObject[] uiObject;
		public Canvas MainCanvas;
		public const float TimeOverChar = 0.075f;
		RectTransform mainCanvasPos = null;

		Dictionary<UIType, UIObject> UIDictionary = new();
		List<GameObject> OpenedUI = new();

		protected override void Awake()
		{
			base.Awake();
			GetPrefab();
		}

		void GetPrefab()
		{
			mainCanvasPos = (RectTransform)Instantiate(MainCanvas).transform;

			foreach (UIObject prefab in uiObject)
			{
				UIDictionary[prefab.UI_Id] = prefab;
			}
		}

		GameObject GeneratePrefab(UIType UIPrefab, Action OnDone)
		{
			GameObject newUI = null;
			if (UIDictionary.TryGetValue(UIPrefab, out UIObject UItemplate))
			{
				newUI = Instantiate(UItemplate.Prefab, mainCanvasPos);
			}
			else
			{
				throw new Exception(UIPrefab + " Couldn't be found");
			}

			return newUI;
		}

		//Open
		public GameObject Open(UIType UIPrefab)
		{
			GameObject newUI = GeneratePrefab(UIPrefab, null);
			if (newUI != null) OpenedUI.Add(newUI);
			return newUI;
		}

		//Open Replace
		public GameObject OpenReplace(UIType UIPrefab)
		{
			CloseAll();
			return Open(UIPrefab);
		}

		//Open Persist
		public GameObject OpenPersist(UIType UIPrefab)
		{
			return GeneratePrefab(UIPrefab, null);
		}

		//Close
		public void Close(GameObject UIPrefab)
		{
			OpenedUI.Remove(UIPrefab);
			Destroy(UIPrefab);
		}

		//Close All
		public void CloseAll()
		{
			foreach (GameObject UIPrefab in OpenedUI.ToArray())
				Close(UIPrefab);
		}
	}

	[Serializable]
	public class UIObject
	{
		public UIType UI_Id;
		public GameObject Prefab;
	}

	public enum UIType
	{
		Start,
		BlankDark,
		BlankWhite,
		Bedroom
	}
}