using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace EugeneC.ECS
{
	[DisallowMultipleComponent]
    public sealed class GridSpawnAuthoring : MonoBehaviour
    {
	    [SerializeField] private GameObject prefab;
	    [SerializeField] private int3 size = new(100, 1,100);
	    [SerializeField] private float3 spacing = 10;
	    [SerializeField, Min(0.001f)] private float scale = 1;
	    
	    public class Baker : Baker<GridSpawnAuthoring>
	    {
		    public override void Bake(GridSpawnAuthoring authoring)
		    {
			    DependsOn(authoring.prefab);
			    
			    var e = GetEntity(TransformUsageFlags.Renderable);
			    var p = GetEntity(authoring.prefab, TransformUsageFlags.Renderable);
			    
			    AddComponent(e, new GridSpawnIData
			    {
				    Prefab = p,
				    Size = authoring.size,
				    Spacing = authoring.spacing,
				    Scale = authoring.scale
			    });
		    }
	    }
    }

	public struct GridSpawnIData : IComponentData
	{
		public Entity Prefab;
		public int3 Size;
		public float3 Spacing;
		public float Scale;
	}

	[BurstCompile]
	[UpdateInGroup(typeof(Eu_InitializationSystemGroup), OrderFirst = true)]
	public partial struct SpawnGridISystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
			var em = state.EntityManager;

			foreach (var (grid, ltw, entity) 
			         in SystemAPI.Query<RefRO<GridSpawnIData>, RefRO<LocalToWorld>>().WithEntityAccess())
			{
				var start = ltw.ValueRO.Position - (grid.ValueRO.Size * grid.ValueRO.Spacing / 2);
				
				var total = grid.ValueRO.Size.x * grid.ValueRO.Size.y * grid.ValueRO.Size.z;
				using var instances = em.Instantiate(grid.ValueRO.Prefab, total, state.WorldUpdateAllocator);

				var count = 0;
				for (var x = 0; x < grid.ValueRO.Size.x; x++)
				{
					for (var y = 0; y < grid.ValueRO.Size.y; y++)
					{
						for (var z = 0; z < grid.ValueRO.Size.z; z++, count++)
						{
							var lt = em.GetComponentData<LocalTransform>(instances[count]);
							lt.Position = start + new float3(x * grid.ValueRO.Spacing.x, y * grid.ValueRO.Spacing.y, z * grid.ValueRO.Spacing.z);
							lt.Scale = grid.ValueRO.Scale;
							em.SetComponentData(instances[count], lt);
						}
					}
				}
				ecb.RemoveComponent<GridSpawnIData>(entity);
			}
			ecb.Playback(em);
		}
	}
}
