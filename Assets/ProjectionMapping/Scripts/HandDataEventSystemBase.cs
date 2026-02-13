using System;
using EugeneC.ECS;
using Unity.Entities;

namespace ProjectionMapping
{
	[UpdateInGroup(typeof(Eu_EffectSystemGroup), OrderLast = true)]
	public partial class HandDataEventSystemBase : SystemBase
	{
		public event Action<EHandPose, EHandPose> OnPoseChanged;
		public event Action<HandData, HandData> OnHandDataChanged;
		
		protected override void OnCreate()
		{
			RequireForUpdate<HandTrackingISingleton>();
			RequireForUpdate<HandPoseISingleton>();
		}

		protected override void OnUpdate()
		{
			var tracking = SystemAPI.GetSingleton<HandTrackingISingleton>();
			var pose = SystemAPI.GetSingleton<HandPoseISingleton>();
			OnPoseChanged?.Invoke(pose.LeftHandPose, pose.RightHandPose);
			OnHandDataChanged?.Invoke(tracking.LeftHand, tracking.RightHand);
		}
	}
}