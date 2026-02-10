using EugeneC.ECS;
using Mediapipe.Unity;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectionMapping
{
	public struct HandTrackingISingleton : IComponentData
	{
		public float LWrist2Thumb;
		public float LWrist2Index;
		public float LWrist2Middle;
		public float LWrist2Ring;
		public float LWrist2Pinky;
		public float LThumb2Index;
		public float LIndex2Middle;
		public float LMiddle2Ring;
		public float LRing2Pinky;
		
		public float RWrist2Thumb;
		public float RWrist2Index;
		public float RWrist2Middle;
		public float RWrist2Ring;
		public float RWrist2Pinky;
		public float RThumb2Index;
		public float RIndex2Middle;
		public float RMiddle2Ring;
		public float RRing2Pinky;
	}
	
	[BurstCompile]
	[UpdateInGroup(typeof(Eu_EffectSystemGroup), OrderFirst = true)]
    public partial struct HandPointISystem : ISystem
    {
	    private const int Wrist = 0;
	    private const int Thumb = 4;
	    private const int Index = 8;
	    private const int Middle = 12;
	    private const int Ring = 16;
	    private const int Pinky = 20;

	    public void OnCreate(ref SystemState state)
	    {
		    state.RequireForUpdate<HandTrackingISingleton>();
	    }

	    [BurstCompile]
	    public void OnUpdate(ref SystemState state)
	    {
		    var tracking = SystemAPI.GetSingleton<HandTrackingISingleton>();
		    
		    var leftPos = new NativeArray<float3>(21, Allocator.Temp);
		    var rightPos = new NativeArray<float3>(21, Allocator.Temp);
		    var leftId = new NativeArray<byte>(21, Allocator.Temp);
		    var rightId = new NativeArray<byte>(21, Allocator.Temp);

		    foreach (var (point, lt, entity) 
		             in SystemAPI.Query<RefRO<HandPointIData>, RefRO<LocalTransform>>().WithEntityAccess())
		    {
			    if (point.ValueRO.EHand == EHand.None) continue;
			    if (!point.ValueRO.IsTracked) continue;
			    
			    var pos = lt.ValueRO.Position;
			    var id = point.ValueRO.ID;
			    if (id >= 21) continue;

			    switch (point.ValueRO.EHand)
			    {
				    case EHand.Left:
					    leftPos[id] = pos;
					    leftId[id] = point.ValueRO.ID;
					    break;
				    case EHand.Right:
					    rightPos[id] = pos;
					    rightId[id] = point.ValueRO.ID;
					    break;
			    }
		    }
		    
		    tracking.LWrist2Thumb = Distance(leftPos, leftId, Wrist, Thumb);
		    tracking.LWrist2Index = Distance(leftPos, leftId, Wrist, Index);
		    tracking.LWrist2Middle = Distance(leftPos, leftId, Wrist, Middle);
		    tracking.LWrist2Ring = Distance(leftPos, leftId, Wrist, Ring);
		    tracking.LWrist2Pinky = Distance(leftPos, leftId, Wrist, Pinky);
		    
		    tracking.LThumb2Index = Distance(leftPos, leftId, Thumb, Index);
		    tracking.LIndex2Middle = Distance(leftPos, leftId, Index, Middle);
		    tracking.LMiddle2Ring = Distance(leftPos, leftId, Middle, Ring);
		    tracking.LRing2Pinky = Distance(leftPos, leftId, Ring, Pinky);
		    
		    tracking.RWrist2Thumb = Distance(rightPos, rightId, Wrist, Thumb);
		    tracking.RWrist2Index = Distance(rightPos, rightId, Wrist, Index);
		    tracking.RWrist2Middle = Distance(rightPos, rightId, Wrist, Middle);
		    tracking.RWrist2Ring = Distance(rightPos, rightId, Wrist, Ring);
		    tracking.RWrist2Pinky = Distance(rightPos, rightId, Wrist, Pinky);
		    
		    tracking.RThumb2Index = Distance(rightPos, rightId, Thumb, Index);
		    tracking.RIndex2Middle = Distance(rightPos, rightId, Index, Middle);
		    tracking.RMiddle2Ring = Distance(rightPos, rightId, Middle, Ring);
		    tracking.RRing2Pinky = Distance(rightPos, rightId, Ring, Pinky);
		    
		    SystemAPI.SetSingleton(tracking);

		    leftPos.Dispose();
		    rightPos.Dispose();
		    leftId.Dispose();
		    rightId.Dispose();
	    }

	    private float Distance(NativeArray<float3> pos, NativeArray<byte> id, int hand1, int hand2)
	    {
		    if (hand1 < 0 || hand1 >= 21 || hand2 < 0 || hand2 >= 21) return -1f;
		    if (id[hand1] != hand1 || id[hand2] != hand2) return -1f;
		    return math.distance(pos[hand1], pos[hand2]);
	    }
    }
}
