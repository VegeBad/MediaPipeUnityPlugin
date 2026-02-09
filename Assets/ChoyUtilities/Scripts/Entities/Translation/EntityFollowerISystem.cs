using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace EugeneC.ECS
{
	public struct EntityTransformIData : IComponentData
	{
		public UnityObjectRef<Transform> Transform;
		public float3 Offset;
		public float SmoothFollowSpeed;
	}
	
	[UpdateInGroup(typeof(Eu_PostTransformSystemGroup), OrderFirst = true)]
    public partial struct EntityFollowerISystem : ISystem
    {
	    public void OnUpdate(ref SystemState state)
	    {
		    var dt = SystemAPI.Time.DeltaTime;

		    foreach (var (entityTransform, lt) 
		             in SystemAPI.Query<RefRO<EntityTransformIData>,RefRW<LocalTransform>>())
		    {
			    var factor = entityTransform.ValueRO.SmoothFollowSpeed > 0 
				    ? entityTransform.ValueRO.SmoothFollowSpeed * dt 
				    : 1;

			    var obj = entityTransform.ValueRO.Transform;

			    lt.ValueRW.Position = math.lerp(lt.ValueRO.Position, (float3)obj.Value.position + entityTransform.ValueRO.Offset, factor);
			    lt.ValueRW.Rotation = math.slerp(lt.ValueRO.Rotation, obj.Value.rotation, factor);
		    }
	    }
    }
}
