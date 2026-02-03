using UnityEngine;

namespace EugeneC.Singleton
{
	public abstract class GenericSingleton<T> : MonoBehaviour
		where T : MonoBehaviour
	{
		public static T Instance { get; protected set; }

		protected virtual void InitSingleton()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = (T)(MonoBehaviour)this;
		}

		protected virtual void UnInitSingleton()
		{
			if (Instance == this)
				Instance = null;
		}

		protected void KeepSingleton(bool keep)
		{
			if (keep) DontDestroyOnLoad(this);
		}

		protected virtual void Awake()
		{
			InitSingleton();
		}

		protected virtual void OnDestroy()
		{
			UnInitSingleton();
		}
	}
}