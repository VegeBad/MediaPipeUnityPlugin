using Mediapipe.Unity;
using Unity.Entities;
using Unity.Mathematics;

namespace ProjectionMapping
{
    public struct HandPointIData : IComponentData
    {
	    public byte ID;
	    public EHand EHand;
	    public bool IsTracked;
    }

    public struct HandSettingISingleton : IComponentData
    {
	    public bool UseGrabAny;
    }

    public struct HandPoseISingleton : IComponentData
    {
	    public EHandPose LeftHandPose;
	    public EHandPose RightHandPose;
    }
    
    public struct PointSpawnIData : IComponentData
    {
	    public float CurrentTime;
    }

    public struct GrabbableData
    {
	    public bool Valid;
	    public Entity Target;
	    public float3 PointOnBody;
	    public float3 Origin;
    }

    public struct HandGrabbableIData : IComponentData
    {
	    public bool IsGrabbed;
    }
}
