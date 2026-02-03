using Unity.Mathematics;
using UnityEngine;

namespace ProjectionMapping
{
	[DisallowMultipleComponent]
	public sealed class MultiDisplaySync : MonoBehaviour
	{
		[SerializeField] private Camera projectionCam;
		[SerializeField] private Canvas webcamCanvas;
		[SerializeField, Range(0, 1)] private float dummySlider;

		private void OnValidate()
		{
			if (projectionCam is null || webcamCanvas is null) return;
			projectionCam.transform.position = new float3(0, 0, -webcamCanvas.planeDistance);
		}

		private void OnEnable()
		{
			OnValidate();
		}
	}
}