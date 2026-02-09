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
	    public EntityArchetype EntityArchetype;
	    
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
		    foreach (var c in children)
		    {
			    if (handedness == null || handedness.Count == 0)
			    {
					c.hand = Hand.None;
			    }
			    else
				    c.hand = handedness[0].categoryName switch
				    {
					    // dk why its always inverted
					    "Right" => Hand.Left,
					    "Left" => Hand.Right,
					    _ => c.hand
				    };
			    // ignore unknown label
		    }
	    }

	    protected override PointBridgeAnnotation InstantiateChild(bool active = true)
	    {
		    var c = base.InstantiateChild(active);
		    c.EManager = EManager;
		    c.EntityArchetype = EntityArchetype;
		    c.id = _counter;
		    if(_counter % 4 == 0) c.isTracked = true;
		    _counter++;
		    return c;
	    }
    }
}
