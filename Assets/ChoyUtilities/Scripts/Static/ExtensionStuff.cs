using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace EugeneC.Utilities
{
	public static partial class UtilityCollection
	{
		// Time-constant style smoothing
		// -DeltaTime divide timeConstant, math.max just to avoid timeConstant is 0
		// More consistent interpolation with different frame rates
		public static float SmoothFactor(this float deltaTime, float timeConstant = 0.02f) =>
			1f - math.exp(-deltaTime / math.max(1e-4f, timeConstant));

		public static float3 GetNoiseOffsetPos(this float3 pos, float yOffset, float time, float height,
			float noiseScale, float depthOffset)
		{
			pos.y = height * noise.snoise(new float2(pos.x * noiseScale + time,
				pos.z * noiseScale + time)) + yOffset * depthOffset;
			return pos;
		}

		public static Quaternion RotateTowards(this Transform ob, Vector3 target, float speed)
		{
			Vector3 dir = (target - ob.position).normalized;
			Quaternion lookTowards = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
			return Quaternion.Slerp(ob.rotation, lookTowards, Time.deltaTime * speed);
		}

		public static Vector3 GetMidPoint(this Vector3 pointA, Vector3 pointB)
		{
			Vector3 midpoint = (pointA + pointB) * 0.5f;
			return midpoint;
		}

		public static bool FinishRotate(this Transform ob, Vector3 target, float threshold = 5f)
		{
			Vector3 dir = (target - ob.position).normalized;
			float angle = Vector3.Angle(ob.forward, dir);
			return angle < threshold;
		}

		public static Transform FindNearestWaypoint(this List<Transform> posList, Transform currentPosition)
		{
			if (posList is null || currentPosition is null) return null;

			Transform nearest = null;
			float distonearest = 0f;

			foreach (Transform pos in posList)
			{
				if (pos is null) continue;
				float distance = (currentPosition.position - pos.position).magnitude;
				if (nearest is null || distance < distonearest) //&& CurrentPosition.CanMoveThere(nearest))) 
				{
					nearest = pos;
					distonearest = distance;
				}
			}

			return nearest;
		}

		public static Transform FindNearestWaypoint(this List<Transform> posList, Transform currentPosition,
			List<Transform> prevPos)
		{
			if (posList is null || currentPosition is null) return null;

			Transform nearest = null;
			float disToNearest = 0f;

			foreach (Transform pos in posList)
			{
				if (pos is null || prevPos is null) continue;
				if (prevPos.Contains(pos)) continue;
				float distance = (currentPosition.position - pos.position).magnitude;
				if (nearest is null || distance < disToNearest) //&& CurrentPosition.CanMoveThere(nearest))) 
				{
					nearest = pos;
					disToNearest = distance;
				}
			}

			return nearest;
		}

		public static bool CanMoveThere(this Transform pos, Vector3 target, string tag)
		{
			Vector3 dir = (target - pos.position).normalized;
			Ray ray = new Ray(pos.position, dir);
			if (Physics.Raycast(ray, out RaycastHit hitInfo))
			{
				if (hitInfo.collider.CompareTag(tag))
					return false;
			}

			return true;
		}

		public static Transform FindNearestEnemy(this GameObject bot, List<GameObject> objectList)
		{
			Transform target = null;
			float disToNearest = 0f;

			foreach (var potentialTarget in objectList)
			{
				if (potentialTarget == bot) continue;
				float distance = (bot.transform.position - potentialTarget.transform.position).magnitude;

				if (target is not null && !(distance < disToNearest)) continue;
				target = potentialTarget.transform;
				disToNearest = distance;
			}

			return target;
		}

		public static GameObject FindNearestObjectInRange(this Transform ob, List<GameObject> oblist, float maxRange)
		{
			GameObject nearest = null;
			float distanceToNearest = 0;

			foreach (var spawned in oblist)
			{
				if (spawned.transform == ob) continue;
				float distance = Vector3.Distance(ob.position, spawned.transform.position);

				if (!(distance <= maxRange)) continue;
				if (nearest is not null && !(distance < distanceToNearest)) continue;

				nearest = spawned;
				distanceToNearest = distance;
			}

			return nearest;
		}
	}
}