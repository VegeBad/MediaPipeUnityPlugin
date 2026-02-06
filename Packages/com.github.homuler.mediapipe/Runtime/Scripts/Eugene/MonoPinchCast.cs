using System;
using System.Collections.Generic;
using Mediapipe.Unity;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectionMapping
{
    public sealed class MonoPinchCast : MonoBehaviour
    {
	    public PointListAnnotation pointList;
        private readonly List<GameObject> _targetList = new();
        private const float Countdown = 0.5f;
        private float _timer;

        private void Start()
        {
	        pointList.hand.OnFingerDistanceChanged += (h, i) =>
	        {
		        if (i < 0.75)
		        {
			        if (_targetList.Count == 0)
			        {
				        if (!Physics.Raycast(transform.position, Vector3.down, out var hit, float.MaxValue)) return;
				        var success= hit.collider.TryGetComponent<IGrabbable>(out _);
				        if (success) _targetList.Add(hit.collider.gameObject);
			        }
			        else
			        {
				        _targetList[0].transform.position = 
					        math.lerp(_targetList[0].transform.position, 
						        new float3(transform.position.x, _targetList[0].transform.position.y, transform.position.z), 10 * Time.deltaTime);
			        }
		        }
		        else
		        {
			        _targetList.Clear();
		        }
	        };
        }
    }
}
