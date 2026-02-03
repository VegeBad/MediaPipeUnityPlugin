using Unity.Collections;
using Unity.Mathematics;

namespace EugeneC.Utilities
{
	public static partial class UtilityCollection
	{
		/// <summary>
		/// Bake the spline using Barry–Goldman algorithm
		/// Or formula of centripetal Catmull–Rom spline
		/// </summary>
		/// <remarks>
		/// https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
		/// </remarks>
		public static NativeList<float3> BakePoints(this Allocator allocator, NativeArray<float3> points,
			byte subdivisions = 100)
		{
			var baked = new NativeList<float3>(allocator);
			baked.Add(points[0]);

			for (var i = 0; i < points.Length; i++)
			{
				float3 p0 = GetPointAt(i - 1);
				float3 p1 = GetPointAt(i);
				float3 p2 = GetPointAt(i + 1);
				float3 p3 = GetPointAt(i + 2);

				float t0 = 0;
				float t1 = t0 + math.distance(p0, p1);
				float t2 = t1 + math.distance(p1, p2);
				float t3 = t2 + math.distance(p2, p3);

				for (int j = 0; j < subdivisions; j += 1)
				{
					float t = math.lerp(t1, t2, (1 + j) / (float)subdivisions);

					float3 a1 = ((t1 - t) * p0 + (t - t0) * p1) / (t1 - t0);
					float3 a2 = ((t2 - t) * p1 + (t - t1) * p2) / (t2 - t1);
					float3 a3 = ((t3 - t) * p2 + (t - t2) * p3) / (t3 - t2);

					float3 b1 = ((t2 - t) * a1 + (t - t0) * a2) / (t2 - t0);
					float3 b2 = ((t3 - t) * a2 + (t - t1) * a3) / (t3 - t1);

					float3 c = ((t2 - t) * b1 + (t - t1) * b2) / (t2 - t1);

					baked.Add(c);
				}
			}

			return baked;

			float3 GetPointAt(int index)
			{
				index %= points.Length;
				if (index < 0) index += points.Length;
				return points[index];
			}
		}
	}
}