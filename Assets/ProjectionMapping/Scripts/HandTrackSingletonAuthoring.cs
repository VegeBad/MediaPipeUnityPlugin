using Unity.Entities;
using UnityEngine;

namespace ProjectionMapping
{
	[DisallowMultipleComponent]
    public sealed class HandTrackSingletonAuthoring : MonoBehaviour
    {
	    [SerializeField] private bool useGrabAny;
	    
	    private void OnValidate()
	    {
		    
	    }

	    public class Baker : Baker<HandTrackSingletonAuthoring>
	    {
		    public override void Bake(HandTrackSingletonAuthoring authoring)
		    {
			    var e = GetEntity(TransformUsageFlags.None);
			    AddComponent<HandTrackingISingleton>(e);
			    AddComponent<HandPoseISingleton>(e);
			    AddComponent(e, new HandSettingISingleton
			    {
				    UseGrabAny = authoring.useGrabAny
			    });
		    }
	    }
    }
}
