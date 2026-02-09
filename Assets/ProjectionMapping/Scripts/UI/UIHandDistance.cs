using System;
using Mediapipe.Unity;
using TMPro;
using UnityEngine;

namespace ProjectionMapping
{
    public sealed class UIHandDistance : MonoBehaviour
    {
        [SerializeField] private MultiHandLandmarkListAnnotation multiHand;
        [SerializeField] private TMP_Text leftText;
        [SerializeField] private TMP_Text rightText;

        private void Start()
        {
	        multiHand.OnFingerDistanceChanged += ShowFingerDistanceChanged;
        }

        private void ShowFingerDistanceChanged(Hand arg1, float arg2)
        {
	        switch (arg1)
	        {
		        case Hand.Left:
			        leftText.text = $"Left: {arg2:F2}";
			        break;
		        case Hand.Right:
			        rightText.text = $"Right: {arg2:F2}";
			        break;
	        }
        }
    }
}
