using EugeneC.ECS;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using UnityEngine;

namespace ProjectionMapping
{
	[DisallowMultipleComponent]
	public sealed class GrabableAuthoring : MonoBehaviour
	{
		private class GrabableAuthoringBaker : Baker<GrabableAuthoring>
		{
			public override void Bake(GrabableAuthoring authoring)
			{
				var e = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent<HandGrabbableITag>(e);
			}
		}
	}

	[UpdateInGroup(typeof(Eu_PostTransformSystemGroup))]
	public partial class HandGrabInputSystemBase : SystemBase
	{
		public NativeReference<SpringData> SpringDataRef;
		public JobHandle? PickJobHandle;

		private const float MaxDistance = 100.0f;
		
		public HandGrabInputSystemBase()
		{
			SpringDataRef =
				new NativeReference<SpringData>(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			SpringDataRef.Value = new SpringData();
		}

		protected override void OnCreate()
		{
			RequireForUpdate<ColliderCastISingleton>();
			RequireForUpdate<PhysicsWorldSingleton>();
			RequireForUpdate<HandTrackingISingleton>();
		}

		protected override void OnUpdate()
		{
			
		}

		protected override void OnDestroy()
		{
			
		}
	}
}