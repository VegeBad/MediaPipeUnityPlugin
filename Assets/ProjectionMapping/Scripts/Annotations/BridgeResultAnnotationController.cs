using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity;

namespace ProjectionMapping
{
    public sealed class BridgeResultAnnotationController : AnnotationController<MultiPointBridgeListAnnotation>
    {
	    private readonly object _currentTargetLock = new object();
	    private HandLandmarkerResult _currentTarget;
	    
	    public void DrawNow(HandLandmarkerResult target)
	    {
		    lock (_currentTargetLock)
		    {
			    target.CloneTo(ref _currentTarget);
		    }

		    SyncNow();
	    }
	    
	    public void DrawLater(HandLandmarkerResult target) => UpdateCurrentTarget(target);
	    
	    private void UpdateCurrentTarget(HandLandmarkerResult newTarget)
	    {
		    lock (_currentTargetLock)
		    {
			    newTarget.CloneTo(ref _currentTarget);
			    isStale = true;
		    }
	    }
	    
	    protected override void SyncNow()
	    {
		    lock (_currentTargetLock)
		    {
			    isStale = false;
			    annotation.SetHandedness(_currentTarget.handedness);
			    annotation.Draw(_currentTarget.handLandmarks);
		    }
	    }
    }
}
