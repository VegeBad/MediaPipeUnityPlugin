using System;
using System.Collections.Generic;
using EugeneC.Singleton;
using UnityEngine;

namespace EugeneC.Obsolete
{
	//Combined method of AudioManager and SpawnManager
	public class SpawnRandomManager : GenericSingleton<SpawnRandomManager>
	{
		public SpawnTerrain[] terrainPrefab;
		Dictionary<TerrainType, SpawnTerrain> _terrainDictionary = new();
		public List<GameObject> spawnedObstacles = new();

		protected override void Awake()
		{
			base.Awake();
			foreach (SpawnTerrain terrain in terrainPrefab)
			{
				_terrainDictionary[terrain.TerrainId] = terrain;
			}
		}

		public GameObject ProceduralSpawn(TerrainType terrainID, Vector3 loc, Quaternion rot)
		{
			GameObject newObject = null;
			if (_terrainDictionary.TryGetValue(terrainID, out SpawnTerrain terrain))
			{
				int key = UnityEngine.Random.Range(0, terrain.Prefab.Length);
				GameObject c = terrain.Prefab[key];
				newObject = Instantiate(c, loc, rot);
				spawnedObstacles.Add(newObject);
			}

			return newObject;
		}

		public void RemoveThisPrefab(GameObject objects)
		{
			spawnedObstacles.Remove(objects);
			Destroy(objects);
		}

		public void RemoveAllTerrain()
		{
			foreach (GameObject objects in spawnedObstacles.ToArray())
			{
				RemoveThisPrefab(objects);
			}
		}
	}

	[Serializable]
	public class SpawnTerrain
	{
		public TerrainType TerrainId;
		public GameObject[] Prefab;
	}

	public enum TerrainType
	{
		Area1,
		Area2,
		Area3,
		Area4,
		Area5,

		FarView1 = 20,
		FarView2,
		FarView3,
		FarView4,
		FarView5,

		ObstacleSet1 = 40,
		ObstacleSet2,
		ObstacleSet3,
		ObstacleSet4,
		ObstacleSet5,

		Cloud1 = 60,
		Cloud2,
		Cloud3,
		Cloud4,
		Cloud5,

		DestroyCube1 = 80,
		DestroyCube2,
		DestroyCube3,
		DestroyCube4,
		DestroyCube5,
	}
}