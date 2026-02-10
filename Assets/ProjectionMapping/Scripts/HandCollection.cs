
namespace ProjectionMapping
{
	public enum ETrackingTarget : byte
	{
		LWrist2Thumb,
		LWrist2Index,
		LWrist2Middle,
		LWrist2Ring,
		LWrist2Pinky,
		LThumb2Index,
		LIndex2Middle,
		LMiddle2Ring,
		LRing2Pinky,
		RWrist2Thumb,
		RWrist2Index,
		RWrist2Middle,
		RWrist2Ring,
		RWrist2Pinky,
		RThumb2Index,
		RIndex2Middle,
		RMiddle2Ring,
		RRing2Pinky,
	}
	
    public static class HandCollection
    {
	    public static float GetValue(this HandTrackingISingleton singleton, ETrackingTarget target)
	    {
		    return target switch
		    {
			    ETrackingTarget.LWrist2Thumb => singleton.LWrist2Thumb,
			    ETrackingTarget.LWrist2Index => singleton.LWrist2Index,
			    ETrackingTarget.LWrist2Middle => singleton.LWrist2Middle,
			    ETrackingTarget.LWrist2Ring => singleton.LWrist2Ring,
			    ETrackingTarget.LWrist2Pinky => singleton.LWrist2Pinky,
			    ETrackingTarget.LThumb2Index => singleton.LThumb2Index,
			    ETrackingTarget.LIndex2Middle => singleton.LIndex2Middle,
			    ETrackingTarget.LMiddle2Ring => singleton.LMiddle2Ring,
			    ETrackingTarget.LRing2Pinky => singleton.LRing2Pinky,
			    ETrackingTarget.RWrist2Thumb => singleton.RWrist2Thumb,
			    ETrackingTarget.RWrist2Index => singleton.RWrist2Index,
			    ETrackingTarget.RWrist2Middle => singleton.RWrist2Middle,
			    ETrackingTarget.RWrist2Ring => singleton.RWrist2Ring,
			    ETrackingTarget.RWrist2Pinky => singleton.RWrist2Pinky,
			    ETrackingTarget.RThumb2Index => singleton.RThumb2Index,
			    ETrackingTarget.RIndex2Middle => singleton.RIndex2Middle,
			    ETrackingTarget.RMiddle2Ring => singleton.RMiddle2Ring,
			    ETrackingTarget.RRing2Pinky => singleton.RRing2Pinky,
			    _ => -1f
		    };
	    }
    }
}
