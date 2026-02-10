using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using static Unity.Physics.Math;
using UnityEngine;

// Revised version of the MousePickAuthoring script from the Unity Physics Samples repository
namespace EugeneC.ECS
{
	[DisallowMultipleComponent]
	public sealed class ColliderCastAuthoring : MonoBehaviour
	{
		[SerializeField] private bool ignoreTriggers = true;
		[SerializeField] private bool ignoreStatic = true;
		[SerializeField] private bool deleteAllEntityOnClick;
		[SerializeField] private bool deleteTagEntityOnClick;

		private Entity _entity;
		private EntityManager _entityManager;

		private void OnValidate()
		{
			if (deleteAllEntityOnClick)
				deleteTagEntityOnClick = false;
		}

		public class Baker : Baker<ColliderCastAuthoring>
		{
			public override void Bake(ColliderCastAuthoring authoring)
			{
				var e = GetEntity(TransformUsageFlags.None);
				AddComponent(e, new ColliderCastISingleton
				{
					IgnoreStatic = authoring.ignoreStatic,
					IgnoreTriggers = authoring.ignoreTriggers,
					DeleteEntityOnClick = authoring.deleteAllEntityOnClick,
					DeleteTagEntityOnClick = authoring.deleteTagEntityOnClick
				});
			}
		}
	}

	public struct ColliderCastISingleton : IComponentData
	{
		public bool IgnoreTriggers;
		public bool IgnoreStatic;
		public bool DeleteEntityOnClick;
		public bool DeleteTagEntityOnClick;
	}
	
	[UpdateInGroup(typeof(Eu_PostTransformSystemGroup))]
	public partial class MouseGrabInputSystemBase : SystemBase
	{
		public NativeReference<SpringData> SpringDataRef;
		public JobHandle? PickJobHandle;

		private const float MaxDistance = 100.0f;

		public MouseGrabInputSystemBase()
		{
			SpringDataRef =
				new NativeReference<SpringData>(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			SpringDataRef.Value = new SpringData();
		}

		protected override void OnCreate()
		{
			RequireForUpdate<ColliderCastISingleton>();
			RequireForUpdate<MouseInputISingleton>();
			RequireForUpdate<PhysicsWorldSingleton>();
		}

		protected override void OnUpdate()
		{
			if (Camera.main is null) return;

			var cam = Camera.main;
			var input = SystemAPI.GetSingleton<MouseInputISingleton>();

			switch (input)
			{
				case { CurrentInput: >= 0.5f, PreviousInput: < 0.5f }:
				{
					var grab = SystemAPI.GetSingleton<ColliderCastISingleton>();
					var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

					var camRay = cam.ScreenPointToRay(new Vector3(input.Position.x, input.Position.y, 0));

					Dependency = new CastIJob
					{
						CollisionWorld = physicsWorld.CollisionWorld,
						IgnoreStatic = grab.IgnoreStatic,
						IgnoreTriggers = grab.IgnoreTriggers,

						SpringDataRef = SpringDataRef,
						RayInput = new RaycastInput
						{
							Start = camRay.origin,
							End = camRay.origin + camRay.direction * MaxDistance,
							Filter = CollisionFilter.Default,
						},
						Near = cam.nearClipPlane,
						Forward = cam.transform.forward,
						MaxDistance = MaxDistance
					}.Schedule(Dependency);

					PickJobHandle = Dependency;
					break;
				}
				case { CurrentInput: < 0.5f, PreviousInput: >= 0.5f }:
					PickJobHandle?.Complete();
					SpringDataRef.Value = new SpringData();
					break;
			}
		}

		protected override void OnDestroy()
		{
			SpringDataRef.Dispose();
		}
	}

	[UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
	public partial class MouseGrabFollowISystem : SystemBase
	{
		private MouseGrabInputSystemBase _grabSystemBase;

		protected override void OnCreate()
		{
			_grabSystemBase = World.GetOrCreateSystemManaged<MouseGrabInputSystemBase>();
			RequireForUpdate<ColliderCastISingleton>();
			RequireForUpdate<MouseInputISingleton>();
		}

		protected override void OnUpdate()
		{
			if (_grabSystemBase.PickJobHandle != null)
				JobHandle.CombineDependencies(Dependency, _grabSystemBase.PickJobHandle.Value).Complete();

			var data = _grabSystemBase.SpringDataRef.Value;
			if (!data.Picked) return;
			var entity = data.Entity;

			var grab = SystemAPI.GetSingleton<ColliderCastISingleton>();
			if (grab.DeleteEntityOnClick)
			{
				EntityManager.DestroyEntity(entity);
				_grabSystemBase.SpringDataRef.Value = new SpringData();
				return;
			}

			if (grab.DeleteTagEntityOnClick && SystemAPI.HasComponent<DestroyIEnableableTag>(entity))
			{
				SystemAPI.SetComponentEnabled<DestroyIEnableableTag>(entity, true);
				_grabSystemBase.SpringDataRef.Value = new SpringData();
				return;
			}

			if (Camera.main is null) return;
			if (!SystemAPI.HasComponent<PhysicsMass>(entity)) return;
			if (SystemAPI.HasComponent<PhysicsMassOverride>(entity)) return;

			var cam = Camera.main;
			var input = SystemAPI.GetSingleton<MouseInputISingleton>();

			var mass = SystemAPI.GetComponent<PhysicsMass>(entity);
			var massOverride = SystemAPI.GetComponentLookup<PhysicsMassOverride>(true);
			var vel = SystemAPI.GetComponent<PhysicsVelocity>(entity);
			var lt = SystemAPI.GetComponent<LocalTransform>(entity);

			if (mass.HasInfiniteMass ||
			    massOverride.HasComponent(entity) && massOverride[entity].IsKinematic != 0) return;
			var worldFromBody = new MTransform(lt.Rotation, lt.Position);

			var bodyFromMotion = new MTransform(mass.InertiaOrientation, mass.CenterOfMass);
			var worldFromMotion = Mul(worldFromBody, bodyFromMotion);

			const float gain = 0.95f;
			vel.Linear *= gain;
			vel.Angular *= gain;

			var bodyCenterNPointWorldPos = Mul(worldFromBody, data.PointOnBody);
			var camWorldPos =
				(float3)cam.ScreenToWorldPoint(new Vector3(input.Position.x, input.Position.y, data.MouseDepth));

			var bodyCenterNPointLocalPos = Mul(Inverse(bodyFromMotion), data.PointOnBody);
			float3 deltaVel;
			{
				var diff = bodyCenterNPointWorldPos - camWorldPos;
				float3 relativeVelInWorld;
				{
					var tangentVel = math.cross(vel.Angular, bodyCenterNPointLocalPos);
					var relativeVelInBody = vel.Linear + math.mul(worldFromMotion.Rotation, tangentVel);
					relativeVelInWorld = Mul(worldFromMotion, relativeVelInBody);
				}

				const float elasticity = 0.1f;
				const float damping = 0.5f;
				deltaVel = -diff * (elasticity / SystemAPI.Time.DeltaTime) - damping * relativeVelInWorld;
			}

			float3x3 effectiveMassMatrix;
			{
				float3 arm = bodyCenterNPointWorldPos - worldFromMotion.Translation;
				var skew = new float3x3(
					new float3(0.0f, arm.z, -arm.y),
					new float3(-arm.z, 0.0f, arm.x),
					new float3(arm.y, -arm.x, 0.0f)
				);

				// world space inertia = worldFromMotion * inertiaInMotionSpace * motionFromWorld
				var invInertiaWs = new float3x3(
					mass.InverseInertia.x * worldFromMotion.Rotation.c0,
					mass.InverseInertia.y * worldFromMotion.Rotation.c1,
					mass.InverseInertia.z * worldFromMotion.Rotation.c2
				);
				invInertiaWs = math.mul(invInertiaWs, math.transpose(worldFromMotion.Rotation));

				float3x3 invEffMassMatrix = math.mul(math.mul(skew, invInertiaWs), skew);
				invEffMassMatrix.c0 = new float3(mass.InverseMass, 0.0f, 0.0f) - invEffMassMatrix.c0;
				invEffMassMatrix.c1 = new float3(0.0f, mass.InverseMass, 0.0f) - invEffMassMatrix.c1;
				invEffMassMatrix.c2 = new float3(0.0f, 0.0f, mass.InverseMass) - invEffMassMatrix.c2;

				effectiveMassMatrix = math.inverse(invEffMassMatrix);
			}

			// Calculate impulse to cause the desired change in velocity
			var impulse = math.mul(effectiveMassMatrix, deltaVel);

			// Clip the impulse
			const float maxAcceleration = 250.0f;
			float maxImpulse = math.rcp(mass.InverseMass) * SystemAPI.Time.DeltaTime * maxAcceleration;
			impulse *= math.min(1.0f, math.sqrt((maxImpulse * maxImpulse) / math.lengthsq(impulse)));
			{
				vel.Linear += impulse * mass.InverseMass;

				float3 impulseLs = math.mul(math.transpose(worldFromMotion.Rotation), impulse);
				float3 angularImpulseLs = math.cross(bodyCenterNPointLocalPos, impulseLs);
				vel.Angular += angularImpulseLs * mass.InverseInertia;
			}

			SystemAPI.SetComponent(entity, vel);
		}
	}
}