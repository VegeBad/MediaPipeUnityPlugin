using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace EugeneC.Singleton
{
	public abstract class GenericEventController<T> : GenericSingleton<T>
		where T : MonoBehaviour

	{
		public async Task<bool> CallEvent(EventSerialize[] events)
		{
			if (events.Length == 0) return false;
			for (int i = 0; i <= events.Length - 1; i++)
			{
				var e = events[i];
				if (e.response.GetPersistentEventCount() == 0) return false;
				e.response?.Invoke();
				await Awaitable.WaitForSecondsAsync(e.duration);
			}

			return true;
		}
	}

	[System.Serializable]
	public class EventSerialize
	{
		public UnityEvent response;
		public float duration;
	}
}