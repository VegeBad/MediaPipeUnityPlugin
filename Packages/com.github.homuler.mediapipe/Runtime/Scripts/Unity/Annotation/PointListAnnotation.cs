// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ProjectionMapping;
using Unity.Mathematics;
using UnityEngine;
using mplt = Mediapipe.LocationData.Types;
using mptcc = Mediapipe.Tasks.Components.Containers;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
	using Color = UnityEngine.Color;

#pragma warning restore IDE0065

	public class PointListAnnotation : ListAnnotation<PointAnnotation>
	{
		[SerializeField] private Color color = Color.green;
		[SerializeField] private float radius = 15.0f;
		public HandLandmarkListAnnotation hand;
		
		private byte _pointCount = 0;
		private PointAnnotation _trackedThumb;
		private PointAnnotation _trackedIndex;
		private MonoPinchCast _pinch;

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this)) return;
			ApplyColor(color);
			ApplyRadius(radius);
		}
#endif
		// Ignore Y level (Although didn't change much)
		private void Update()
		{
			if(_trackedIndex is null || _trackedThumb is null || hand is null) return;
			var a = new float2(_trackedThumb.transform.position.x, _trackedThumb.transform.position.z);
			var b = new float2(_trackedIndex.transform.position.x, _trackedIndex.transform.position.z);
			var dis = math.distance(a, b);
			hand.OnFingerDistanceChanged?.Invoke(hand.handedness, dis);
		}

		public void SetColor(Color col)
		{
			color = col;
			ApplyColor(color);
		}

		public void SetRadius(float radi)
		{
			radius = radi;
			ApplyRadius(radius);
		}

		public void Draw(IReadOnlyList<Vector3> targets)
		{
			if (ActivateFor(targets))
			{
				CallActionForAll(targets, (annotation, target) => annotation?.Draw(target));
			}
		}

		public void Draw(IReadOnlyList<Landmark> targets, float3 scale, bool visualizeZ = true)
		{
			if (ActivateFor(targets))
			{
				CallActionForAll(targets, (annotation, target) => annotation?.Draw(target, scale, visualizeZ));
			}
		}

		public void Draw(LandmarkList targets, float3 scale, bool visualizeZ = true)
		{
			Draw(targets.Landmark, scale, visualizeZ);
		}

		public void Draw(IReadOnlyList<NormalizedLandmark> targets, bool visualizeZ = true)
		{
			if (ActivateFor(targets))
			{
				CallActionForAll(targets, (annotation, target) => annotation?.Draw(target, visualizeZ));
			}
		}

		public void Draw(NormalizedLandmarkList targets, bool visualizeZ = true)
		{
			Draw(targets.Landmark, visualizeZ);
		}

		public void Draw(IReadOnlyList<mptcc.NormalizedLandmark> targets, bool visualizeZ = true)
		{
			if (ActivateFor(targets))
			{
				CallActionForAll(targets, (annotation, target) => annotation?.Draw(in target, visualizeZ));
			}
		}

		public void Draw(mptcc.NormalizedLandmarks targets, bool visualizeZ = true) => Draw(targets.landmarks, visualizeZ);

		public void Draw(IReadOnlyList<mplt.RelativeKeypoint> targets, float threshold = 0.0f)
		{
			if (ActivateFor(targets))
			{
				CallActionForAll(targets, (annotation, target) => annotation.Draw(target, threshold));
			}
		}

		public void Draw(IReadOnlyList<mptcc.NormalizedKeypoint> targets, float threshold = 0.0f)
		{
			if (ActivateFor(targets))
			{
				CallActionForAll(targets, (annotation, target) => annotation?.Draw(target, threshold));
			}
		}

		protected override PointAnnotation InstantiateChild(bool isActive = true)
		{
			var annotation = base.InstantiateChild(isActive);
			annotation.SetColor(color);
			annotation.SetRadius(radius);
			
			_pointCount++;
			// Get [4] and [8] points
			switch (_pointCount)
			{
				case 5:
					_trackedThumb = annotation;
					_pinch = annotation.gameObject.AddComponent<MonoPinchCast>();
					_pinch.pointList = this;
					break;
				case 9:
					_trackedIndex = annotation;
					break;
			}
			return annotation;
		}

		private void ApplyColor(Color col)
		{
			foreach (var point in children)
			{
				point?.SetColor(col);
			}
		}

		private void ApplyRadius(float radi)
		{
			foreach (var point in children)
			{
				point?.SetRadius(radi);
			}
		}
	}
}