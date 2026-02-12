using EugeneC.ECS;
using Mediapipe.Unity;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectionMapping
{
	public struct PointData
	{
		public float Current;
		public float Previous;
		public float3 Position;
	}

	public struct HandData
	{
		public PointData Wrist2Thumb;
		public PointData Wrist2Index;
		public PointData Wrist2Middle;
		public PointData Wrist2Ring;
		public PointData Wrist2Pinky;
		public PointData Thumb2Index;
		public PointData Index2Middle;
		public PointData Middle2Ring;
		public PointData Ring2Pinky;
		public PointData Pinky2Thumb;
	}
	
	public struct HandTrackingISingleton : IComponentData
	{
		public HandData LeftHand;
		public HandData RightHand;
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
		    state.RequireForUpdate<HandPoseISingleton>();
	    }

	    [BurstCompile]
	    public void OnUpdate(ref SystemState state)
	    {
		    var tracking = SystemAPI.GetSingleton<HandTrackingISingleton>();
		    var pose = SystemAPI.GetSingleton<HandPoseISingleton>();
		    
		    var leftPos = new NativeArray<float3>(21, Allocator.Temp);
		    var rightPos = new NativeArray<float3>(21, Allocator.Temp);
		    var leftId = new NativeArray<byte>(21, Allocator.Temp);
		    var rightId = new NativeArray<byte>(21, Allocator.Temp);

		    foreach (var (point, lt, _) 
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
		    
		    // Maybe there's a better way to do this, but keep it as it is for now
		    tracking.LeftHand.Wrist2Thumb.Previous = tracking.LeftHand.Wrist2Thumb.Current;
		    tracking.LeftHand.Wrist2Index.Previous = tracking.LeftHand.Wrist2Index.Current;
		    tracking.LeftHand.Wrist2Middle.Previous = tracking.LeftHand.Wrist2Middle.Current;
		    tracking.LeftHand.Wrist2Ring.Previous = tracking.LeftHand.Wrist2Ring.Current;
		    tracking.LeftHand.Wrist2Pinky.Previous = tracking.LeftHand.Wrist2Pinky.Current;
		    
		    tracking.LeftHand.Thumb2Index.Previous = tracking.LeftHand.Thumb2Index.Current;
		    tracking.LeftHand.Index2Middle.Previous = tracking.LeftHand.Index2Middle.Current;
		    tracking.LeftHand.Middle2Ring.Previous = tracking.LeftHand.Middle2Ring.Current;
		    tracking.LeftHand.Ring2Pinky.Previous = tracking.LeftHand.Ring2Pinky.Current;
		    tracking.LeftHand.Pinky2Thumb.Previous = tracking.LeftHand.Pinky2Thumb.Current;
		    
		    tracking.RightHand.Wrist2Thumb.Previous = tracking.RightHand.Wrist2Thumb.Current;
		    tracking.RightHand.Wrist2Index.Previous = tracking.RightHand.Wrist2Index.Current;
		    tracking.RightHand.Wrist2Middle.Previous = tracking.RightHand.Wrist2Middle.Current;
		    tracking.RightHand.Wrist2Ring.Previous = tracking.RightHand.Wrist2Ring.Current;
		    tracking.RightHand.Wrist2Pinky.Previous = tracking.RightHand.Wrist2Pinky.Current;
		    
		    tracking.RightHand.Thumb2Index.Previous = tracking.RightHand.Thumb2Index.Current;
		    tracking.RightHand.Index2Middle.Previous = tracking.RightHand.Index2Middle.Current;
		    tracking.RightHand.Middle2Ring.Previous = tracking.RightHand.Middle2Ring.Current;
		    tracking.RightHand.Ring2Pinky.Previous = tracking.RightHand.Ring2Pinky.Current;
		    tracking.RightHand.Pinky2Thumb.Previous = tracking.RightHand.Pinky2Thumb.Current;
		    
		    (tracking.LeftHand.Wrist2Thumb.Current, tracking.LeftHand.Wrist2Thumb.Position) 
			    = DistanceBetween(leftPos, leftId, Wrist, Thumb);
		    (tracking.LeftHand.Wrist2Index.Current, tracking.LeftHand.Wrist2Index.Position) 
			    = DistanceBetween(leftPos, leftId, Wrist, Index);
		    (tracking.LeftHand.Wrist2Middle.Current, tracking.LeftHand.Wrist2Middle.Position) 
			    = DistanceBetween(leftPos, leftId, Wrist, Middle);
		    (tracking.LeftHand.Wrist2Ring.Current, tracking.LeftHand.Wrist2Ring.Position) 
			    = DistanceBetween(leftPos, leftId, Wrist, Ring);
		    (tracking.LeftHand.Wrist2Pinky.Current, tracking.LeftHand.Wrist2Pinky.Position) 
			    = DistanceBetween(leftPos, leftId, Wrist, Pinky);
		    
		    (tracking.LeftHand.Thumb2Index.Current, tracking.LeftHand.Thumb2Index.Position) 
			    = DistanceBetween(leftPos, leftId, Thumb, Index);
		    (tracking.LeftHand.Index2Middle.Current, tracking.LeftHand.Index2Middle.Position) 
			    = DistanceBetween(leftPos, leftId, Index, Middle);
		    (tracking.LeftHand.Middle2Ring.Current, tracking.LeftHand.Middle2Ring.Position) 
			    = DistanceBetween(leftPos, leftId, Middle, Ring);
		    (tracking.LeftHand.Ring2Pinky.Current, tracking.LeftHand.Ring2Pinky.Position) 
			    = DistanceBetween(leftPos, leftId, Ring, Pinky);
		    (tracking.LeftHand.Pinky2Thumb.Current, tracking.LeftHand.Pinky2Thumb.Position) 
			    = DistanceBetween(leftPos, leftId, Pinky, Thumb);
		    
		    (tracking.RightHand.Wrist2Thumb.Current, tracking.RightHand.Wrist2Thumb.Position) 
			    = DistanceBetween(rightPos, rightId, Wrist, Thumb);
		    (tracking.RightHand.Wrist2Index.Current, tracking.RightHand.Wrist2Index.Position) 
			    = DistanceBetween(rightPos, rightId, Wrist, Index);
		    (tracking.RightHand.Wrist2Middle.Current, tracking.RightHand.Wrist2Middle.Position) 
			    = DistanceBetween(rightPos, rightId, Wrist, Middle);
		    (tracking.RightHand.Wrist2Ring.Current, tracking.RightHand.Wrist2Ring.Position) 
			    = DistanceBetween(rightPos, rightId, Wrist, Ring);
		    (tracking.RightHand.Wrist2Pinky.Current, tracking.RightHand.Wrist2Pinky.Position) 
			    = DistanceBetween(rightPos, rightId, Wrist, Pinky);
		    
		    (tracking.RightHand.Thumb2Index.Current, tracking.RightHand.Thumb2Index.Position) 
			    = DistanceBetween(rightPos, rightId, Thumb, Index);
		    (tracking.RightHand.Index2Middle.Current, tracking.RightHand.Index2Middle.Position) 
			    = DistanceBetween(rightPos, rightId, Index, Middle);
		    (tracking.RightHand.Middle2Ring.Current, tracking.RightHand.Middle2Ring.Position) 
			    = DistanceBetween(rightPos, rightId, Middle, Ring);
		    (tracking.RightHand.Ring2Pinky.Current, tracking.RightHand.Ring2Pinky.Position) 
			    = DistanceBetween(rightPos, rightId, Ring, Pinky);
		    (tracking.RightHand.Pinky2Thumb.Current, tracking.RightHand.Pinky2Thumb.Position) 
			    = DistanceBetween(rightPos, rightId, Pinky, Thumb);
		    
		    SystemAPI.SetSingleton(tracking);

		    leftPos.Dispose();
		    rightPos.Dispose();
		    leftId.Dispose();
		    rightId.Dispose();

		    var (lValid, left) = tracking.GetHand(EHand.Left);
		    pose.LeftHandPose = lValid ? left.GetPose() : EHandPose.None;
		    var (rValid, right) = tracking.GetHand(EHand.Right);
		    pose.RightHandPose = rValid ? right.GetPose() : EHandPose.None;
		    SystemAPI.SetSingleton(pose);
	    }

	    private (float, float3) DistanceBetween(NativeArray<float3> pos, NativeArray<byte> id, int id1, int id2)
	    {
		    if (id[id1] != id1 || id[id2] != id2) return (-1f, float3.zero);
		    return (math.distance(pos[id1], pos[id2]), math.lerp(pos[id1], pos[id2], 0.5f));
	    }
    }
}
