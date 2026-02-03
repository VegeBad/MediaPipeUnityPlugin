using UnityEditor;
using System;
using UnityEngine;
using System.Linq;

//Origin: https://www.youtube.com/watch?v=EFh7tniBqkk

//Read this:
//In case of debugging and this is blocking the way just comment out [InitializeOnLoad]
//After finish debugging uncomment back

namespace EugeneC.Utilities
{
#if UNITY_EDITOR

	[InitializeOnLoad]
	internal static class LoadIconDisplay
	{
		static bool _hierarchyHasFocus;
		static EditorWindow _window;

		static LoadIconDisplay()
		{
			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
			EditorApplication.update += OnEditorUpdate;
		}

		static void OnEditorUpdate()
		{
			_window ??= Resources.FindObjectsOfTypeAll<EditorWindow>()
				.FirstOrDefault(window => window.GetType().Name == "SceneHierarchyWindow");

			_hierarchyHasFocus = EditorWindow.focusedWindow != null && EditorWindow.focusedWindow == _window;
		}

		static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
		{
			if (EditorUtility.InstanceIDToObject(instanceID) is not GameObject obj) return;

			//Is this a prefab? If yes return
			if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) != null) return;

			//Check if the game object has any component, if null return
			Component[] components = obj.GetComponents<Component>();
			if (components is null || components.Length == 0) return;

			//Get the second-highest arrangement component in the game object, if none use transform instead
			Component component = components.Length > 1 ? components[1] : components[0];

			//Get what type of the component
			Type type = component.GetType();

			//Tell unity to get the icon of the component, but will also return back the text, so set that as null
			GUIContent content = EditorGUIUtility.ObjectContent(component, type);
			content.text = null;
			//On mouse hover gives the context of the icon
			content.tooltip = type.Name;

			if (content.image is null) return;

			//Cover up the default box

			bool isSelected = Selection.instanceIDs.Contains(instanceID);
			bool isHovering = selectionRect.Contains(Event.current.mousePosition);

			Color color = EditorBackgroundColor.GetColor(isSelected, isHovering, _hierarchyHasFocus);
			Rect background = selectionRect;
			background.width = 18.5f;
			EditorGUI.DrawRect(background, color);

			EditorGUI.LabelField(selectionRect, content);
		}
	}

	public static class EditorBackgroundColor
	{
		static readonly Color DefaultLightColor = new Color(0.7843f, 0.7843f, 0.7843f);
		static readonly Color DefaultDarkColor = new Color(0.2196f, 0.2196f, 0.2196f);

		static readonly Color SelectedLightColor = new Color(0.22745f, 0.447f, 0.6902f);
		static readonly Color SelectedDarkColor = new Color(0.1725f, 0.3647f, 0.5294f);

		static readonly Color SelectedUnfocusedLightColor = new Color(0.68f, 0.68f, 0.68f);
		static readonly Color SelectedUnfocusedDarkColor = new Color(0.3f, 0.3f, 0.3f);

		static readonly Color HoverLightColor = new Color(0.698f, 0.698f, 0.698f);
		static readonly Color HoverDarkColor = new Color(0.2706f, 0.2706f, 0.2706f);

		public static Color GetColor(bool isSelected, bool isHovered, bool isWindowsFocused)
		{
			if (isSelected)
			{
				if (isWindowsFocused)
					return EditorGUIUtility.isProSkin ? SelectedDarkColor : SelectedLightColor;
				else
					return EditorGUIUtility.isProSkin ? SelectedUnfocusedDarkColor : SelectedUnfocusedLightColor;
			}
			else if (isHovered)
			{
				return EditorGUIUtility.isProSkin ? HoverDarkColor : HoverLightColor;
			}
			else
			{
				return EditorGUIUtility.isProSkin ? DefaultDarkColor : DefaultLightColor;
			}
		}
	}

#endif
}