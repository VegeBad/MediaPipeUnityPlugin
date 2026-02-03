using EugeneC.Utilities;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using static Unity.Physics.Math;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

// Revised version of the MousePickAuthoring script from the Unity Physics Samples repository
namespace EugeneC.ECS
{
	[DisallowMultipleComponent]
	public sealed class MouseGrabAuthoring : MonoBehaviour
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

		public class Baker : Baker<MouseGrabAuthoring>
		{
			public override void Bake(MouseGrabAuthoring authoring)
			{
				var e = GetEntity(TransformUsageFlags.None);
				AddComponent(e, new MouseInputISingleton());
				AddComponent(e, new MouseGrabISingleton
				{
					IgnoreStatic = authoring.ignoreStatic,
					IgnoreTriggers = authoring.ignoreTriggers,
					DeleteEntityOnClick = authoring.deleteAllEntityOnClick,
					DeleteTagEntityOnClick = authoring.deleteTagEntityOnClick
				});
			}
		}
	}

	public struct MouseInputISingleton : IComponentData
	{
		public float CurrentInput;
		public float PreviousInput;
		public float2 Position;
	}

	public struct MouseGrabISingleton : IComponentData
	{
		public bool IgnoreTriggers;
		public bool IgnoreStatic;
		public bool DeleteEntityOnClick;
		public bool DeleteTagEntityOnClick;
	}

	public struct SpringData
	{
		public Entity Entity;
		public bool Picked;
		public float3 PointOnBody;
		public float MouseDepth;
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
			RequireForUpdate<MouseGrabISingleton>();
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
					var grab = SystemAPI.GetSingleton<MouseGrabISingleton>();
					var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

					var camRay = cam.ScreenPointToRay(new Vector3(input.Position.x, input.Position.y, 0));

					Dependency = new PickIJob
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

#if !UNITY_WEBGL || UNITY_EDITOR
		[BurstCompile]
#endif
		public struct MouseGrabICollector : ICollector<RaycastHit>
		{
			public MouseGrabICollector(int dynamicCount, float maxFraction = 1f)
			{
				Hit = default;
				IgnoreTriggers = true;
				IgnoreStatic = true;
				_numDynamicBodies = dynamicCount;
				MaxFraction = maxFraction;
				NumHits = 0;
			}

			public RaycastHit Hit;
			public bool IgnoreTriggers;
			public bool IgnoreStatic;
			private readonly int _numDynamicBodies;

			// Below are all ICollector implementations
			public bool EarlyOutOnFirstHit => false;
			public float MaxFraction { get; private set; }
			public int NumHits { get; private set; }

			public bool AddHit(RaycastHit hit)
			{
				Assert.IsTrue(hit.Fraction <= MaxFraction);

				var passed = true;
				if (IgnoreStatic)
					passed &= hit.RigidBodyIndex >= 0 && hit.RigidBodyIndex < _numDynamicBodies;
				if (IgnoreTriggers)
					passed &= hit.Material.CollisionResponse != CollisionResponsePolicy.RaiseTriggerEvents;
				if (!passed) return false;

				Hit = hit;
				MaxFraction = hit.Fraction;
				NumHits = 1;
				return true;
			}
		}

#if !UNITY_WEBGL || UNITY_EDITOR
		[BurstCompile]
#endif
		public struct PickIJob : IJob
		{
			[ReadOnly] public CollisionWorld CollisionWorld;
			[ReadOnly] public bool IgnoreTriggers;
			[ReadOnly] public bool IgnoreStatic;

			public NativeReference<SpringData> SpringDataRef;
			public RaycastInput RayInput;
			public float Near;
			public float3 Forward;

			public void Execute()
			{
				var pickCollector = new MouseGrabICollector(CollisionWorld.NumDynamicBodies)
				{
					IgnoreTriggers = IgnoreTriggers,
					IgnoreStatic = IgnoreStatic
				};

				if (CollisionWorld.CastRay(RayInput, ref pickCollector))
				{
					var fraction = pickCollector.Hit.Fraction;
					var hitBody = CollisionWorld.Bodies[pickCollector.Hit.RigidBodyIndex];

					//Grab that specific point on the body instead of the center
					float3 pointOnBody;
					{
						//Convert world transform to local transform
						var localTrans = Inverse(new MTransform(hitBody.WorldFromBody));
						pointOnBody = Mul(localTrans, pickCollector.Hit.Position);
					}

					float rayDot;
					{
						var rayDir = math.normalize(RayInput.End - RayInput.Start);
						rayDot = math.dot(rayDir, Forward);
					}

					SpringDataRef.Value = new SpringData
					{
						Entity = hitBody.Entity,
						Picked = true,
						PointOnBody = pointOnBody,
						MouseDepth = Near + rayDot * fraction * MaxDistance
					};
				}
				else
				{
					SpringDataRef.Value = new SpringData
					{
						Picked = false
					};
				}
			}
		}
	}

	[UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
	public partial class MouseGrabFollowISystem : SystemBase
	{
		private MouseGrabInputSystemBase _grabSystemBase;

		protected override void OnCreate()
		{
			_grabSystemBase = World.GetOrCreateSystemManaged<MouseGrabInputSystemBase>();
			RequireForUpdate<MouseGrabISingleton>();
			RequireForUpdate<MouseInputISingleton>();
		}

		protected override void OnUpdate()
		{
			if (_grabSystemBase.PickJobHandle != null)
				JobHandle.CombineDependencies(Dependency, _grabSystemBase.PickJobHandle.Value).Complete();

			var data = _grabSystemBase.SpringDataRef.Value;
			if (!data.Picked) return;
			var entity = data.Entity;

			var grab = SystemAPI.GetSingleton<MouseGrabISingleton>();
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