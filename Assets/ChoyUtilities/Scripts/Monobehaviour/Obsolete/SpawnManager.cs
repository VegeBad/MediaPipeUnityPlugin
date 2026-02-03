using System;
using System.Collections.Generic;
using EugeneC.Singleton;
using UnityEngine;

namespace EugeneC.Obsolete
{
	public class SpawnManager : GenericSingleton<SpawnManager>
	{
		public SpawnObject[] spawnObject;
		Dictionary<SpawnEnum, SpawnObject> SpawnDictionary = new Dictionary<SpawnEnum, SpawnObject>();
		public List<GameObject> SpawnedObjects = new();

		protected override void Awake()
		{
			base.Awake();
			foreach (SpawnObject SpawnPrefab in spawnObject)
			{
				SpawnDictionary[SpawnPrefab.SpawnId] = SpawnPrefab;
			}
		}

		public GameObject Spawning(SpawnEnum spawnid, Vector3 pos, Quaternion rot)
		{
			GameObject newobject = null;
			if (SpawnDictionary.TryGetValue(spawnid, out SpawnObject SpawnPrefab))
			{
				newobject = Instantiate(SpawnPrefab.Prefab, pos, rot);
				SpawnedObjects.Add(newobject);
			}

			return newobject;
		}

		public void DeSpawning(GameObject SpawnPrefab)
		{
			SpawnedObjects.Remove(SpawnPrefab);
			Destroy(SpawnPrefab);
		}

		public void DeSpawnAll()
		{
			foreach (GameObject objects in SpawnedObjects.ToArray())
			{
				DeSpawning(objects);
			}
		}
	}

	[Serializable]
	public class SpawnObject
	{
		public SpawnEnum SpawnId;
		public GameObject Prefab;
	}

	public enum SpawnEnum
	{
	}
}