using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace EugeneC.Utilities
{
	public static partial class HelperCollection
	{
		public static float Modulo(float x, float y) => (x % y + y) % y;

		public static int Modulo(int x, int y) => (x % y + y) % y;

		public static float Random01(Entity entity, double et)
		{
			var ran = Random.CreateFromIndex((uint)entity.Index + (uint)et << 4 + 1);
			return ran.NextFloat();
		}

		public static float Random01(GameObject obj)
		{
			var ran = Random.CreateFromIndex((uint)obj.GetInstanceID() + (uint)Time.time << 4 + 1);
			return ran.NextFloat();
		}

		public static float RandomRange(float min, float max)
		{
			var ran = Random.CreateFromIndex((uint)System.Environment.TickCount << 4 + 1);
			return ran.NextFloat(min, max);
		}

		public static uint RandomUInt()
		{
			var ran = Random.CreateFromIndex((uint)System.Environment.TickCount << 4 + 1);
			return ran.NextUInt();
		}

		public static float3 GetClosestPointInSplineSegment(float3 lineStart, float3 lineEnd, float3 point, out float t)
		{
			t = 0f;
			var vec = lineEnd - lineStart;
			var lenSq = math.lengthsq(vec);

			if (lenSq <= 0f) return lineStart;

			t = math.clamp(math.dot(point - lineStart, vec) / lenSq, 0f, 1f);
			return lineStart + vec * t;
		}

		public static void SampleAtDistance(ref SplineVectorBlob spline, float targetDist,
			out float3 position, out quaternion rotation)
		{
			ref var posArr = ref spline.Position;
			ref var dstArr = ref spline.Distance;
			ref var rotArr = ref spline.Rotation;

			var count = posArr.Length;
			switch (count)
			{
				case 0:
					position = default;
					rotation = quaternion.identity;
					return;
				case 1:
					position = posArr[0];
					rotation = rotArr[0];
					return;
			}

			int idx = dstArr.LowerBound(targetDist);
			idx = math.clamp(idx, 0, count - 2);

			float d0 = dstArr[idx];
			float d1 = dstArr[idx + 1];
			float segLen = math.max(1e-6f, d1 - d0);
			float t = math.saturate((targetDist - d0) / segLen);

			float3 p0 = posArr[idx];
			float3 p1 = posArr[idx + 1];
			position = math.lerp(p0, p1, t);

			quaternion r0 = rotArr[idx];
			quaternion r1 = rotArr[idx + 1];
			rotation = math.slerp(r0, r1, t);
		}
	}

	public struct SplineVectorBlob
	{
		public BlobArray<float3> Position;
		public BlobArray<float> Distance;
		public BlobArray<quaternion> Rotation;
		public BlobArray<float3> Tangent;
	}

	public struct PositionRotationBlob
	{
		public BlobArray<float3> Position;
		public BlobArray<quaternion> Rotation;
	}

	public struct EntityBlob
	{
		public BlobArray<Entity> Entities;
	}
}