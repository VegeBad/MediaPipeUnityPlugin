using Mediapipe.Unity;
using Unity.Entities;

namespace ProjectionMapping
{
    public struct HandPointIData : IComponentData
    {
	    public byte ID;
	    public EHand EHand;
	    public bool IsTracked;
    }
    
    public struct PointSpawnIData : IComponentData
    {
	    public float CurrentTime;
    }
    
    public struct HandGrabbableITag : IComponentData { }
}
