using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EugeneC.ECS
{
	[DisallowMultipleComponent]
	public class SphereSpawnAuthoring : MonoBehaviour
	{
		public GameObject prefab;
		public float radius = 10f;
		public byte amount = 10;
		public float height = 10f;
		[Range(0, 2f)] public float speed = .2f;

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position, radius);
		}

		public class Baker : Baker<SphereSpawnAuthoring>
		{
			public override void Bake(SphereSpawnAuthoring authoring)
			{
				if (authoring.prefab is null || authoring.radius <= 0) return;

				var entity = GetEntity(TransformUsageFlags.Renderable);
				var prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);

				AddComponent(entity, new SphereSpawnIData
				{
					Prefab = prefab,
					Radius = authoring.radius,
					Amount = authoring.amount,
					Height = authoring.height,
					Speed = authoring.speed,
				});
			}
		}
	}

	public struct SphereSpawnIData : IComponentData
	{
		public Entity Prefab;
		public float Radius;
		public byte Amount;
		public float Height;
		public float Speed;
	}

#if !UNITY_WEBGL
	[BurstCompile]
#endif
	[UpdateInGroup(typeof(Eu_InitializationSystemGroup), OrderFirst = true)]
	public partial struct SpawnInSphereISystem : ISystem
	{
#if !UNITY_WEBGL
		[BurstCompile]
#endif
		public void OnUpdate(ref SystemState state)
		{
			var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
			var em = state.EntityManager;

			foreach (var (sphere, ltw, entity)
			         in SystemAPI.Query<RefRO<SphereSpawnIData>, RefRO<LocalToWorld>>().WithEntityAccess())
			{
				using var instances = em.Instantiate(sphere.ValueRO.Prefab, sphere.ValueRO.Amount,
					state.WorldUpdateAllocator);
				for (var i = 0; i < sphere.ValueRO.Amount; i++)
				{
					var lt = em.GetComponentData<LocalTransform>(instances[i]);
					var newPos = ltw.ValueRO.Position + (float3)Random.insideUnitSphere * sphere.ValueRO.Radius;
					lt.Position = newPos;
					em.SetComponentData(instances[i], lt);
					ecb.AddComponent(instances[i], new WaveMoveIData
					{
						YOffset = newPos.y,
						Height = sphere.ValueRO.Height,
						Speed = sphere.ValueRO.Speed,
					});
				}

				ecb.RemoveComponent<SphereSpawnIData>(entity);
			}

			ecb.Playback(em);
		}
	}
}