// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Unity.CoordinateSystem;
using Unity.Mathematics;
using UnityEngine;
using mplt = Mediapipe.LocationData.Types;
using mptcc = Mediapipe.Tasks.Components.Containers;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
	using Color = UnityEngine.Color;

#pragma warning restore IDE0065

	public class PointAnnotation : HierarchicalAnnotation
	{
		[SerializeField] private Color color = Color.green;
		[SerializeField] private float radius = 15.0f;
		
		private Renderer _renderer;

		private void OnEnable()
		{
			_renderer = GetComponent<Renderer>();
			ApplyColor(color);
			ApplyRadius(radius);
		}

		private void OnDisable()
		{
			ApplyRadius(0.0f);
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

		public void Draw(float3 position)
		{
			SetActive(true);
			transform.localPosition = position;
		}

		public void Draw(Landmark target, float3 scale, bool visualizeZ = true)
		{
			if (!ActivateFor(target)) return;
			var position = GetScreenRect().GetPoint(target, scale, rotationAngle, isMirrored);
			if (!visualizeZ)
			{
				position.z = 0.0f;
			}

			transform.localPosition = position;
		}

		public void Draw(NormalizedLandmark target, bool visualizeZ = true)
		{
			if (!ActivateFor(target)) return;
			var position = GetScreenRect().GetPoint(target, rotationAngle, isMirrored);
			if (!visualizeZ)
			{
				position.z = 0.0f;
			}

			transform.localPosition = position;
		}

		public void Draw(mptcc.NormalizedLandmark target, bool visualizeZ = true)
		{
			if (!ActivateFor(target)) return;
			var position = GetScreenRect().GetPoint(in target, rotationAngle, isMirrored);
			if (!visualizeZ)
			{
				position.z = 0.0f;
			}

			transform.localPosition = position;
		}

		public void Draw(mplt.RelativeKeypoint target, float threshold = 0.0f)
		{
			if (!ActivateFor(target)) return;
			var value = GetScreenRect().GetPoint(target, rotationAngle, isMirrored);
			Draw(new float3(value.x, value.y, 0.0f));
			SetColor(GetColor(target.Score, threshold));
		}

		public void Draw(mptcc.NormalizedKeypoint target, float threshold = 0.0f)
		{
			if (!ActivateFor(target)) return;
			var value = GetScreenRect().GetPoint(target, rotationAngle, isMirrored);
			Draw(new float3(value.x, value.y, 0.0f));
			SetColor(GetColor(target.score ?? 1.0f, threshold));
		}

		private void ApplyColor(Color col) => _renderer.material.color = col;

		private void ApplyRadius(float radi) => transform.localScale = radi * Vector3.one;

		private Color GetColor(float score, float threshold)
		{
			var t = (score - threshold) / (1 - threshold);
			var h = math.lerp(90, 0, t) / 360; // from yellow-green to red
			return Color.HSVToRGB(h, 1, 1);
		}
	}
}