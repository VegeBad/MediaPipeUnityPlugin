using System;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Unity;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectionMapping
{
    public sealed class MultiPointBridgeListAnnotation : ListAnnotation<PointBridgeListAnnotation>
    {
	    private EntityManager _entityManager;
	    
	    private async void Start()
	    {
		    try
		    {
			    await Awaitable.EndOfFrameAsync();
			    _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		    }
		    catch (Exception e){ Debug.Log(e);}
	    }

	    public void Draw(IReadOnlyList<NormalizedLandmarks> targets)
	    {
		    if (ActivateFor(targets))
		    {
			    CallActionForAll(targets, (annotation, target) => { annotation?.Draw(target.landmarks); });
		    }
	    }
	    
	    public void SetHandedness(IReadOnlyList<Classifications> handedness)
	    {
		    var count = handedness?.Count ?? 0;
		    for (var i = 0; i < math.min(count, children.Count); i++)
		    {
			    if (handedness != null) 
				    children[i].SetHandedness(handedness[i].categories);
		    }

		    for (var i = count; i < children.Count; i++)
		    {
			    children[i].SetHandedness((IReadOnlyList<Category>)null);
		    }
	    }

	    protected override PointBridgeListAnnotation InstantiateChild(bool active = true)
	    {
		    var c = base.InstantiateChild(active);
		    c.EManager = _entityManager;
		    return c;
	    }
    }
}
