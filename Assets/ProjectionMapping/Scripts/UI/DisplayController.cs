using EugeneC.Singleton;
using EugeneC.Utilities;
using UnityEngine;

namespace ProjectionMapping
{
	[DisallowMultipleComponent]
	public class DisplayController : GenericSingleton<DisplayController>
	{
		private void Start()
		{
			HelperCollection.ActivateDisplay();
		}
	}
}