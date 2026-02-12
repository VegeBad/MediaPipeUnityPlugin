using System;
using EugeneC.ECS;
using Unity.Entities;

namespace ProjectionMapping
{
	[UpdateInGroup(typeof(Eu_EffectSystemGroup), OrderLast = true)]
	public partial class HandUISystemBase : SystemBase
	{
		public event Action<EHandPose, EHandPose> OnPoseChanged;
		
		protected override void OnCreate()
		{
			
		}

		protected override void OnUpdate()
		{
			
		}
	}
}