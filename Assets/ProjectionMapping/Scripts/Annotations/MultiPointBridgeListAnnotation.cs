using System;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Unity;
using Unity.Entities;
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

	    protected override PointBridgeListAnnotation InstantiateChild(bool active = true)
	    {
		    var c = base.InstantiateChild(active);
		    c.EManager = _entityManager;
		    return c;
	    }
    }
}
