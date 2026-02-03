// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Unity.CoordinateSystem;
using Unity.Mathematics;
using UnityEngine;
using mplt = Mediapipe.LocationData.Types;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
	using Color = UnityEngine.Color;

#pragma warning restore IDE0065

	public class RectangleAnnotation : HierarchicalAnnotation
	{
		[SerializeField] private LineRenderer lineRenderer;
		[SerializeField] private Color color = Color.red;
		[SerializeField, Range(0, 1)] private float lineWidth = 1.0f;

		private static readonly Vector3[] _EmptyPositions = new Vector3[] { };

		private void OnEnable()
		{
			ApplyColor(color);
			ApplyLineWidth(lineWidth);
		}

		private void OnDisable()
		{
			ApplyLineWidth(0.0f);
			lineRenderer.SetPositions(_EmptyPositions);
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this)) return;
			ApplyColor(color);
			ApplyLineWidth(lineWidth);
		}
#endif

		public void SetColor(Color col)
		{
			color = col;
			ApplyColor(color);
		}

		public void SetLineWidth(float width)
		{
			lineWidth = width;
			ApplyLineWidth(lineWidth);
		}

		public void Draw(Vector3[] positions)
		{
			lineRenderer.SetPositions(positions ?? _EmptyPositions);
		}

		public void Draw(Rect target, Vector2Int imageSize)
		{
			if (ActivateFor(target))
			{
				Draw(GetScreenRect().GetRectVertices(target, imageSize, rotationAngle, isMirrored));
			}
		}

		public void Draw(NormalizedRect target)
		{
			if (ActivateFor(target))
			{
				Draw(GetScreenRect().GetRectVertices(target, rotationAngle, isMirrored));
			}
		}

		public void Draw(LocationData target, int2 imageSize)
		{
			if (!ActivateFor(target)) return;
			switch (target.Format)
			{
				case mplt.Format.BoundingBox:
				{
					Draw(GetScreenRect().GetRectVertices(target.BoundingBox, imageSize, rotationAngle, isMirrored));
					break;
				}
				case mplt.Format.RelativeBoundingBox:
				{
					Draw(GetScreenRect().GetRectVertices(target.RelativeBoundingBox, rotationAngle, isMirrored));
					break;
				}
				case mplt.Format.Global:
				case mplt.Format.Mask:
				default:
				{
					throw new System.ArgumentException(
						$"The format of the LocationData must be BoundingBox or RelativeBoundingBox, but {target.Format}");
				}
			}
		}

		public void Draw(LocationData target)
		{
			if (ActivateFor(target))
			{
				switch (target.Format)
				{
					case mplt.Format.RelativeBoundingBox:
					{
						Draw(GetScreenRect().GetRectVertices(target.RelativeBoundingBox, rotationAngle, isMirrored));
						break;
					}
					case mplt.Format.BoundingBox:
					case mplt.Format.Global:
					case mplt.Format.Mask:
					default:
					{
						throw new System.ArgumentException(
							$"The format of the LocationData must be RelativeBoundingBox, but {target.Format}");
					}
				}
			}
		}

		private void ApplyColor(Color col)
		{
			if (lineRenderer is null) return;
			lineRenderer.startColor = col;
			lineRenderer.endColor = col;
		}

		private void ApplyLineWidth(float width)
		{
			if (lineRenderer is null) return;
			lineRenderer.startWidth = width;
			lineRenderer.endWidth = width;
		}
	}
}