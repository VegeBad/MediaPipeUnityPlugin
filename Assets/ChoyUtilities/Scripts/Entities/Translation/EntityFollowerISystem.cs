using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace EugeneC.ECS
{
	public struct EntityTransformIData : IComponentData
	{
		public UnityObjectRef<Transform> Transform;
		public float3 Offset;
		public float SmoothFollowSpeed;
	}
	
	[BurstCompile]
	[UpdateInGroup(typeof(Eu_PostTransformSystemGroup), OrderFirst = true)]
    public partial struct EntityFollowerISystem : ISystem
    {
	    [BurstCompile]
	    public void OnUpdate(ref SystemState state)
	    {
		    
	    }
    }
}
