using System;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Unity;
using Mediapipe.Unity.CoordinateSystem;
using Unity.Entities;
using UnityEngine;

namespace ProjectionMapping
{
	[DisallowMultipleComponent]
    public sealed class PointBridgeAnnotation : HierarchicalAnnotation
    {
	    public EntityManager EManager;
	    public byte id;
	    public bool isTracked;
	    private Entity _entity;

	    private async void Start()
	    {
		    try
		    {
			    await Awaitable.WaitForSecondsAsync(.1f);
			    _entity = EManager.CreateEntity();
		    }
		    catch (Exception e)
		    {
			    Debug.LogException(e);
		    }
	    }

	    public void Draw(NormalizedLandmark target)
	    {
		    if (!ActivateFor(target)) return;
		    var position = GetScreenRect().GetPoint(target, rotationAngle, isMirrored);
		    transform.localPosition = position;
	    }
    }
}
