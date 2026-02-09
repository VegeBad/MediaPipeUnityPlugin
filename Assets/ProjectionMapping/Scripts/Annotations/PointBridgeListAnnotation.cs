using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Unity;
using Unity.Entities;
using Unity.Mathematics;

namespace ProjectionMapping
{
    public sealed class PointBridgeListAnnotation : ListAnnotation<PointBridgeAnnotation>
    {
	    public EntityManager EManager;
	    private byte _counter;
	    
	    private void Start()
	    {
		    Fill(HandLandmarkCollection.LandmarkCount);
	    }

	    public void Draw(IReadOnlyList<NormalizedLandmark> target)
	    {
		    if (ActivateFor(target))
		    {
			    CallActionForAll(target, (annotation, targets) => annotation?.Draw(targets));
		    }
	    }
	    
	    public void SetHandedness(IReadOnlyList<Category> handedness)
	    {
		    if (handedness == null || handedness.Count == 0 || handedness[0].categoryName == "Left")
		    {
			    
		    }
		    else if (handedness[0].categoryName == "Right")
		    {
			    
		    }
		    // ignore unknown label
	    }

	    protected override PointBridgeAnnotation InstantiateChild(bool active = true)
	    {
		    var c = base.InstantiateChild(active);
		    c.EManager = EManager;
		    c.id = _counter;
		    if(_counter % 4 == 0) c.isTracked = true;
		    _counter++;
		    return c;
	    }
    }
}
