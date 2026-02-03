using UnityEngine;
using UnityEditor;

namespace FolderColor
{
#if UNITY_EDITOR

	public class CustomWindowFileImage : EditorWindow
	{
		string assetPath;

		public static void ShowWindow(string assetPathGive)
		{
			CustomWindowFileImage window = GetWindow<CustomWindowFileImage>("Custom Folder");
			window.assetPath = assetPathGive;
			window.Show();
		}

		private void OnGUI()
		{
			if (GUI.Button(new Rect(0, 0, 100, 100), "None"))
			{
				if (ProjectAssetViewerCustomisation.ModificationData.assetModified.Contains(assetPath))
				{
					RemoveReference(assetPath);
					ProjectAssetViewerCustomisation.SaveData();
				}

				Close();
			}

			string path = ProjectAssetViewerCustomisation.FindScriptPathByName("CustomWindowFileImage");
			path = path.Replace("/Editor/CustomWindowFileImage.cs", "");

			string[] texturesPath = AssetDatabase.FindAssets("t:texture2D", new[] { path });

			int buttonsPerRow = 4;
			float buttonPadding = 10f;

			for (int i = 0; i < texturesPath.Length; i++)
			{
				Texture2D texture =
					(Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(texturesPath[i]),
						typeof(Texture2D));

				float buttonWidth = (position.width - (buttonsPerRow + 1) * buttonPadding) / buttonsPerRow;
				float buttonHeight = 100f;

				float x = (i % buttonsPerRow) * (buttonWidth + buttonPadding) + buttonPadding;
				float y = Mathf.Floor(i / buttonsPerRow) * (buttonHeight + buttonPadding) + buttonPadding + 100;

				if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), texture))
				{
					if (ProjectAssetViewerCustomisation.ModificationData.assetModified.Contains(assetPath))
						RemoveReference(assetPath);

					ProjectAssetViewerCustomisation.ModificationData.assetModified.Add(assetPath);
					ProjectAssetViewerCustomisation.ModificationData.assetModifiedTexturePath.Add(
						AssetDatabase.GUIDToAssetPath(texturesPath[i]));
					ProjectAssetViewerCustomisation.SaveData();

					Close();
				}
			}
		}

		private static void RemoveReference(string assetPath)
		{
			int i = ProjectAssetViewerCustomisation.ModificationData.assetModified.IndexOf(assetPath);
			ProjectAssetViewerCustomisation.ModificationData.assetModified.RemoveAt(i);
			ProjectAssetViewerCustomisation.ModificationData.assetModifiedTexturePath.RemoveAt(i);
		}
	}
#endif
}