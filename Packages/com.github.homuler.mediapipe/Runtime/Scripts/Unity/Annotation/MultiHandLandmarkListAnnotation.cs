// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using mptcc = Mediapipe.Tasks.Components.Containers;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
	using Color = UnityEngine.Color;

#pragma warning restore IDE0065

	public sealed class MultiHandLandmarkListAnnotation : ListAnnotation<HandLandmarkListAnnotation>
	{
		[SerializeField] private Color leftLandmarkColor = Color.green;
		[SerializeField] private Color rightLandmarkColor = Color.green;
		[SerializeField] private float landmarkRadius = 15.0f;
		[SerializeField] private Color connectionColor = Color.white;
		[SerializeField, Range(0, 1)] private float connectionWidth = 1.0f;
		
		public delegate void FingerDistanceChangedHandler(HandLandmarkListAnnotation.Hand hand, float distance);
		public event FingerDistanceChangedHandler OnFingerDistanceChanged;

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this)) return;
			ApplyLeftLandmarkColor(leftLandmarkColor);
			ApplyRightLandmarkColor(rightLandmarkColor);
			ApplyLandmarkRadius(landmarkRadius);
			ApplyConnectionColor(connectionColor);
			ApplyConnectionWidth(connectionWidth);
		}
#endif

		public void SetLeftLandmarkColor(Color leftColor)
		{
			leftLandmarkColor = leftColor;
			ApplyLeftLandmarkColor(leftLandmarkColor);
		}

		public void SetRightLandmarkColor(Color rightColor)
		{
			rightLandmarkColor = rightColor;
			ApplyRightLandmarkColor(rightLandmarkColor);
		}

		public void SetLandmarkRadius(float landmarkRadi)
		{
			landmarkRadius = landmarkRadi;
			ApplyLandmarkRadius(landmarkRadius);
		}

		public void SetConnectionColor(Color col)
		{
			connectionColor = col;
			ApplyConnectionColor(connectionColor);
		}

		public void SetConnectionWidth(float width)
		{
			connectionWidth = width;
			ApplyConnectionWidth(connectionWidth);
		}

		public void SetHandedness(IReadOnlyList<ClassificationList> handedness)
		{
			var count = handedness?.Count ?? 0;
			for (var i = 0; i < math.min(count, children.Count); i++)
			{
				if (handedness != null) 
					children[i].SetHandedness(handedness[i]);
			}

			for (var i = count; i < children.Count; i++)
			{
				children[i].SetHandedness((IReadOnlyList<Classification>)null);
			}
		}

		public void SetHandedness(IReadOnlyList<mptcc.Classifications> handedness)
		{
			var count = handedness?.Count ?? 0;
			for (var i = 0; i < math.min(count, children.Count); i++)
			{
				if (handedness != null) 
					children[i].SetHandedness(handedness[i]);
			}

			for (var i = count; i < children.Count; i++)
			{
				children[i].SetHandedness((IReadOnlyList<mptcc.Category>)null);
			}
		}

		public void Draw(IReadOnlyList<NormalizedLandmarkList> targets, bool visualizeZ = false)
		{
			if (ActivateFor(targets))
			{
				CallActionForAll(targets, (annotation, target) => { annotation?.Draw(target, visualizeZ); });
			}
		}

		public void Draw(IReadOnlyList<mptcc.NormalizedLandmarks> targets, bool visualizeZ = false)
		{
			if (ActivateFor(targets))
			{
				CallActionForAll(targets, (annotation, target) => { annotation?.Draw(target, visualizeZ); });
			}
		}

		protected override HandLandmarkListAnnotation InstantiateChild(bool isActive = true)
		{
			var annotation = base.InstantiateChild(isActive);
			annotation.SetLeftLandmarkColor(leftLandmarkColor);
			annotation.SetRightLandmarkColor(rightLandmarkColor);
			annotation.SetLandmarkRadius(landmarkRadius);
			annotation.SetConnectionColor(connectionColor);
			annotation.SetConnectionWidth(connectionWidth);
			annotation.OnFingerDistanceChanged += (hand, distance) => OnFingerDistanceChanged?.Invoke(hand, distance);
			return annotation;
		}

		private void ApplyLeftLandmarkColor(Color color)
		{
			foreach (var handLandmarkList in children)
			{
				if (handLandmarkList != null)
				{
					handLandmarkList.SetLeftLandmarkColor(color);
				}
			}
		}

		private void ApplyRightLandmarkColor(Color color)
		{
			foreach (var handLandmarkList in children)
			{
				handLandmarkList?.SetRightLandmarkColor(color);
			}
		}

		private void ApplyLandmarkRadius(float radi)
		{
			foreach (var handLandmarkList in children)
			{
				handLandmarkList?.SetLandmarkRadius(radi);
			}
		}

		private void ApplyConnectionColor(Color col)
		{
			foreach (var handLandmarkList in children)
			{
				handLandmarkList?.SetConnectionColor(col);
			}
		}

		private void ApplyConnectionWidth(float width)
		{
			foreach (var handLandmarkList in children)
			{
				handLandmarkList?.SetConnectionWidth(width);
			}
		}
	}
}