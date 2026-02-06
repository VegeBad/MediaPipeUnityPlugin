// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using UnityEngine;
using mptcc = Mediapipe.Tasks.Components.Containers;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
	using Color = UnityEngine.Color;

#pragma warning restore IDE0065

	public sealed class HandLandmarkListAnnotation : HierarchicalAnnotation
	{
		[SerializeField] private PointListAnnotation pointListAnnotation;
		[SerializeField] private ConnectionListAnnotation connectionListAnnotation;
		[SerializeField] private Color leftLandmarkColor = Color.green;
		[SerializeField] private Color rightLandmarkColor = Color.green;

		public Hand handedness = Hand.None;
		public Action<Hand, float> OnFingerDistanceChanged;
		
		public enum Hand : byte
		{
			Left,
			Right,
			None = byte.MaxValue, 
		}

		private const int LandmarkCount = 21;

		private readonly List<(int, int)> _connections = new List<(int, int)>
		{
			(0, 1),
			(1, 2),
			(2, 3),
			(3, 4),
			(0, 5),
			(5, 9),
			(9, 13),
			(13, 17),
			(0, 17),
			(5, 6),
			(6, 7),
			(7, 8),
			(9, 10),
			(10, 11),
			(11, 12),
			(13, 14),
			(14, 15),
			(15, 16),
			(17, 18),
			(18, 19),
			(19, 20),
		};

		public override bool isMirrored
		{
			set
			{
				pointListAnnotation.isMirrored = value;
				connectionListAnnotation.isMirrored = value;
				base.isMirrored = value;
			}
		}

		public override RotationAngle rotationAngle
		{
			set
			{
				pointListAnnotation.rotationAngle = value;
				connectionListAnnotation.rotationAngle = value;
				base.rotationAngle = value;
			}
		}

		public PointAnnotation this[int index] => pointListAnnotation[index];

		private void Start()
		{
			pointListAnnotation.Fill(LandmarkCount);
			connectionListAnnotation.Fill(_connections, pointListAnnotation);
			
			pointListAnnotation.hand = this;
		}

		public void SetLeftLandmarkColor(Color leftColor) => leftLandmarkColor = leftColor;

		public void SetRightLandmarkColor(Color rightColor) => rightLandmarkColor = rightColor;

		public void SetLandmarkRadius(float landmarkRadius)
		{
			pointListAnnotation.SetRadius(landmarkRadius);
		}

		public void SetConnectionColor(Color connectionColor)
		{
			connectionListAnnotation.SetColor(connectionColor);
		}

		public void SetConnectionWidth(float connectionWidth)
		{
			connectionListAnnotation.SetLineWidth(connectionWidth);
		}

		// For some reason, left & right results are inverted
		public void SetHandedness(Hand handed)
		{
			switch (handed)
			{
				case Hand.Left:
					handedness = Hand.Right;
					pointListAnnotation.SetColor(rightLandmarkColor);
					break;
				case Hand.Right:
					handedness = Hand.Left;
					pointListAnnotation.SetColor(leftLandmarkColor);
					break;
			}
		}

		public void SetHandedness(IReadOnlyList<Classification> handedness)
		{
			if (handedness == null || handedness.Count == 0 || handedness[0].Label == "Left")
			{
				SetHandedness(Hand.Left);
			}
			else if (handedness[0].Label == "Right")
			{
				SetHandedness(Hand.Right);
			}
			// ignore unknown label
		}

		public void SetHandedness(ClassificationList handedness)
		{
			SetHandedness(handedness.Classification);
		}

		public void SetHandedness(IReadOnlyList<mptcc.Category> handedness)
		{
			if (handedness == null || handedness.Count == 0 || handedness[0].categoryName == "Left")
			{
				SetHandedness(Hand.Left);
			}
			else if (handedness[0].categoryName == "Right")
			{
				SetHandedness(Hand.Right);
			}
			// ignore unknown label
		}

		public void SetHandedness(mptcc.Classifications handedness)
		{
			SetHandedness(handedness.categories);
		}

		public void Draw(IReadOnlyList<NormalizedLandmark> target, bool visualizeZ = false)
		{
			if (!ActivateFor(target)) return;
			pointListAnnotation.Draw(target, visualizeZ);
			// Draw explicitly because connection annotation's targets remain the same.
			connectionListAnnotation.Redraw();
		}

		public void Draw(NormalizedLandmarkList target, bool visualizeZ = false)
		{
			Draw(target?.Landmark, visualizeZ);
		}

		public void Draw(IReadOnlyList<mptcc.NormalizedLandmark> target, bool visualizeZ = false)
		{
			if (!ActivateFor(target)) return;
			pointListAnnotation.Draw(target, visualizeZ);
			// Draw explicitly because connection annotation's targets remain the same.
			connectionListAnnotation.Redraw();
		}

		public void Draw(mptcc.NormalizedLandmarks target, bool visualizeZ = false)
		{
			Draw(target.landmarks, visualizeZ);
		}
	}
}