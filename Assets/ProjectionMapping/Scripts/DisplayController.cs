using EugeneC.Singleton;
using UnityEngine;

namespace ProjectionMapping
{
	[DisallowMultipleComponent]
	public class DisplayController : GenericSingleton<DisplayController>
	{
		private void Start()
		{
			ActivateDisplay();
		}

		private static void ActivateDisplay()
		{
			for (var i = 1; i < Display.displays.Length; i++)
			{
				Display.displays[i].Activate();
			}
		}
	}
}