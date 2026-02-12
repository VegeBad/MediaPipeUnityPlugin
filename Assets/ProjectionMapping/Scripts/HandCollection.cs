using Mediapipe.Unity;
using Unity.Mathematics;

namespace ProjectionMapping
{
	//DO NOT TOUCH
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
	
	public enum EHandPose : byte
	{
		None = 0,
		ClenchedFist = 1 << 0,
		ThumbsUp = 1 << 1,
		MiddleFinger = 1 << 2,
		PhoneSign = 1 << 3,
		PeaceSign = 1 << 4,
		RockNRoll = 1 << 5,
		OkSign = 1 << 6,
		HighFive = 1 << 7,
	}
	
    public static class HandCollection
    {
	    public static (float, float3) GetValue(this HandTrackingISingleton singleton, ETrackingTarget target)
	    {
		    return target switch
		    {
			    ETrackingTarget.LWrist2Thumb => (singleton.LeftHand.Wrist2Thumb.Current, singleton.LeftHand.Wrist2Thumb.Position),
			    ETrackingTarget.LWrist2Index => (singleton.LeftHand.Wrist2Index.Current, singleton.LeftHand.Wrist2Index.Position),
			    ETrackingTarget.LWrist2Middle => (singleton.LeftHand.Wrist2Middle.Current, singleton.LeftHand.Wrist2Middle.Position),
			    ETrackingTarget.LWrist2Ring => (singleton.LeftHand.Wrist2Ring.Current, singleton.LeftHand.Wrist2Ring.Position),
			    ETrackingTarget.LWrist2Pinky => (singleton.LeftHand.Wrist2Pinky.Current, singleton.LeftHand.Wrist2Pinky.Position),
			    ETrackingTarget.LThumb2Index => (singleton.LeftHand.Thumb2Index.Current, singleton.LeftHand.Thumb2Index.Position),
			    ETrackingTarget.LIndex2Middle => (singleton.LeftHand.Index2Middle.Current, singleton.LeftHand.Index2Middle.Position),
			    ETrackingTarget.LMiddle2Ring => (singleton.LeftHand.Middle2Ring.Current, singleton.LeftHand.Middle2Ring.Position),
			    ETrackingTarget.LRing2Pinky => (singleton.LeftHand.Ring2Pinky.Current, singleton.LeftHand.Ring2Pinky.Position),
			    ETrackingTarget.RWrist2Thumb => (singleton.RightHand.Wrist2Thumb.Current, singleton.RightHand.Wrist2Thumb.Position),
			    ETrackingTarget.RWrist2Index => (singleton.RightHand.Wrist2Index.Current, singleton.RightHand.Wrist2Index.Position),
			    ETrackingTarget.RWrist2Middle => (singleton.RightHand.Wrist2Middle.Current, singleton.RightHand.Wrist2Middle.Position),
			    ETrackingTarget.RWrist2Ring => (singleton.RightHand.Wrist2Ring.Current, singleton.RightHand.Wrist2Ring.Position),
			    ETrackingTarget.RWrist2Pinky => (singleton.RightHand.Wrist2Pinky.Current, singleton.RightHand.Wrist2Pinky.Position),
			    ETrackingTarget.RThumb2Index => (singleton.RightHand.Thumb2Index.Current, singleton.RightHand.Thumb2Index.Position),
			    ETrackingTarget.RIndex2Middle => (singleton.RightHand.Index2Middle.Current, singleton.RightHand.Index2Middle.Position),
			    ETrackingTarget.RMiddle2Ring => (singleton.RightHand.Middle2Ring.Current, singleton.RightHand.Middle2Ring.Position),
			    ETrackingTarget.RRing2Pinky => (singleton.RightHand.Ring2Pinky.Current, singleton.RightHand.Ring2Pinky.Position),
			    _ => (-1f, float3.zero)
		    };
	    }

	    public static float GetPrevious(this HandTrackingISingleton singleton, ETrackingTarget target)
	    {
		    return target switch
		    {
			    ETrackingTarget.LWrist2Thumb => singleton.LeftHand.Wrist2Thumb.Previous,
			    ETrackingTarget.LWrist2Index => singleton.LeftHand.Wrist2Index.Previous,
			    ETrackingTarget.LWrist2Middle => singleton.LeftHand.Wrist2Middle.Previous,
			    ETrackingTarget.LWrist2Ring => singleton.LeftHand.Wrist2Ring.Previous,
			    ETrackingTarget.LWrist2Pinky => singleton.LeftHand.Wrist2Pinky.Previous,
			    ETrackingTarget.LThumb2Index => singleton.LeftHand.Thumb2Index.Previous,
			    ETrackingTarget.LIndex2Middle => singleton.LeftHand.Index2Middle.Previous,
			    ETrackingTarget.LMiddle2Ring => singleton.LeftHand.Middle2Ring.Previous,
			    ETrackingTarget.LRing2Pinky => singleton.LeftHand.Ring2Pinky.Previous,
			    ETrackingTarget.RWrist2Thumb => singleton.RightHand.Wrist2Thumb.Previous,
			    ETrackingTarget.RWrist2Index => singleton.RightHand.Wrist2Index.Previous,
			    ETrackingTarget.RWrist2Middle => singleton.RightHand.Wrist2Middle.Previous,
			    ETrackingTarget.RWrist2Ring => singleton.RightHand.Wrist2Ring.Previous,
			    ETrackingTarget.RWrist2Pinky => singleton.RightHand.Wrist2Pinky.Previous,
			    ETrackingTarget.RThumb2Index => singleton.RightHand.Thumb2Index.Previous,
			    ETrackingTarget.RIndex2Middle => singleton.RightHand.Index2Middle.Previous,
			    ETrackingTarget.RMiddle2Ring => singleton.RightHand.Middle2Ring.Previous,
			    ETrackingTarget.RRing2Pinky => singleton.RightHand.Ring2Pinky.Previous,
			    _ => -1f
		    };
	    }

	    public static (bool, HandData) GetHand(this HandTrackingISingleton singleton, EHand hand)
	    {
		    return hand switch
		    {
			    EHand.Left => (true, singleton.LeftHand),
			    EHand.Right => (true, singleton.RightHand),
			    _ => (false, default)
		    };
	    }

	    private static EHandPose GetPose(HandData data)
	    {
		    
		    return EHandPose.None;
	    }
    }
}
