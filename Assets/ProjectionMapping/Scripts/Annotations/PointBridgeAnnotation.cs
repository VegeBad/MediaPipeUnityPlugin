using System;
using EugeneC.ECS;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Unity;
using Mediapipe.Unity.CoordinateSystem;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ProjectionMapping
{
	[DisallowMultipleComponent]
    public sealed class PointBridgeAnnotation : HierarchicalAnnotation
    {
	    public EntityManager EManager;
	    public EntityArchetype EntityArchetype;
	    
	    public Hand hand;
	    public byte id;
	    public bool isTracked;
	    private Entity _entity;

	    public void Draw(NormalizedLandmark target)
	    {
		    if (!ActivateFor(target)) return;
		    var position = GetScreenRect().GetPoint(target, rotationAngle, isMirrored);
		    
		    if(_entity == Entity.Null)
			    _entity = EManager.CreateEntity(EntityArchetype);
		    
		    EManager.SetComponentData(_entity, new LocalTransform
		    {
			    Position = position
		    });
		    EManager.SetComponentData(_entity, new HandPointIData
		    {
			    ID = id,
			    Hand = hand,
			    IsTracked = isTracked
		    });
	    }
	    
    }
}
