using UnityEngine;
using UnityEngine.Localization;

namespace EugeneC.Utility
{
	[CreateAssetMenu(fileName = "NewBasicScriptable", menuName = "Choy Utilities/Basic")]
	public class BasicProperties : ScriptableObject
	{
		public LocalizedString localizedName;
		public Sprite picture;
	}
}