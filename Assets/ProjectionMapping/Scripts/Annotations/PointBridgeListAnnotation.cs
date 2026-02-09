using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Unity;
using Unity.Entities;

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
