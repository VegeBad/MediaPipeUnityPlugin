using Unity.Mathematics;
using UnityEngine;

namespace EugeneC.Mono
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Eugene/Follower")]
	public class MonoFollower : MonoBehaviour
	{
		[SerializeField] private Transform target;
		[SerializeField] private float3 offset;
		[SerializeField] [Range(0f, 30f)] private float smoothFollowSpeed;

		private float _factor;

		private void OnValidate()
		{
			if (target is null) return;
			offset = transform.position - target.position;
		}

		private void OnEnable()
		{
			if (target is null) return;
			_factor = smoothFollowSpeed > 0 ? smoothFollowSpeed : 1f;
		}

		private void Update()
		{
			if (target is null) return;
			transform.position =
				math.lerp(transform.position, (float3)target.position + offset, _factor * Time.deltaTime);
		}
	}
}