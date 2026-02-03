// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Unity.Mathematics;
using UnityEngine;

namespace Mediapipe.Unity
{
	public enum RotationAngle : ushort
	{
		Rotation0 = 0,
		Rotation90 = 90,
		Rotation180 = 180,
		Rotation270 = 270,
	}

	public static class RotationAngleExtension
	{
		public static RotationAngle Add(this RotationAngle rotationAngle, RotationAngle angle) =>
			(RotationAngle)(((int)rotationAngle + (int)angle) % 360);

		public static RotationAngle Subtract(this RotationAngle rotationAngle, RotationAngle angle) =>
			(RotationAngle)(((int)rotationAngle - (int)angle) % 360);

		public static RotationAngle Reverse(this RotationAngle rotationAngle) =>
			(RotationAngle)((360 - (int)rotationAngle) % 360);

		public static float3 GetEulerAngles(this RotationAngle rotationAngle) => new(0, 0, (int)rotationAngle);
	}
}