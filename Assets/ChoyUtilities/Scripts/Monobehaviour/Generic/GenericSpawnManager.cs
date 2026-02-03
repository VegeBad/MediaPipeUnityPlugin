using System;
using System.Collections.Generic;
using UnityEngine;

namespace EugeneC.Singleton
{
	public abstract class GenericSpawnManager<T> : GenericSingleton<GenericSpawnManager<T>>
		where T : Enum
	{
		public SpawnSerialize<T>[] serializedOb;
		Dictionary<T, SpawnSerialize<T>> _spawnDictionary = new();

		public List<GameObject> spawnedObjects = new();

		protected override void Awake()
		{
			base.Awake();
			foreach (var spawnPrefab in serializedOb)
				_spawnDictionary[spawnPrefab.spawnId] = spawnPrefab;
		}

		public GameObject SpawnObject(T id, Vector3 pos, Quaternion rot)
		{
			GameObject newob = null;
			if (_spawnDictionary.TryGetValue(id, out SpawnSerialize<T> spawnPrefab))
			{
				int key = UnityEngine.Random.Range(0, spawnPrefab.prefab.Length);
				GameObject c = spawnPrefab.prefab[key];
				newob = Instantiate(c, pos, rot);
				spawnedObjects.Add(newob);
			}

			return newob;
		}

		public void DespawnObject(GameObject despawnob)
		{
			if (spawnedObjects.Contains(despawnob))
			{
				spawnedObjects.Remove(despawnob);
				Destroy(despawnob);
			}
		}

		public void DespawnEverything()
		{
			foreach (var ob in spawnedObjects.ToArray())
				DespawnObject(ob);
		}
	}

	[Serializable]
	public struct SpawnSerialize<T>
		where T : Enum
	{
		public T spawnId;
		public GameObject[] prefab;
	}
}