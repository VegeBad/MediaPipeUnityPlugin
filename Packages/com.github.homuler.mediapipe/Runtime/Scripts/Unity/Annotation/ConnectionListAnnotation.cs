// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
	using Color = UnityEngine.Color;

#pragma warning restore IDE0065

	public sealed class ConnectionListAnnotation : ListAnnotation<ConnectionAnnotation>
	{
		[SerializeField] private Color color = Color.red;
		[SerializeField, Range(0, 1)] private float lineWidth = 1.0f;

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this)) return;
			ApplyColor(color);
			ApplyLineWidth(lineWidth);
		}
#endif

		public void Fill(IReadOnlyList<(int, int)> connections, PointListAnnotation points)
		{
			Draw(connections.Select(pair => new Connection(points[pair.Item1], points[pair.Item2])).ToList());
		}

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

		public void Draw(IReadOnlyList<Connection> targets)
		{
			if (ActivateFor(targets))
			{
				CallActionForAll(targets, (annotation, target) => annotation?.Draw(target));
			}
		}

		public void Redraw()
		{
			foreach (var connection in children)
			{
				connection?.Redraw();
			}
		}

		protected override ConnectionAnnotation InstantiateChild(bool isActive = true)
		{
			var annotation = base.InstantiateChild(isActive);
			annotation.SetColor(color);
			annotation.SetLineWidth(lineWidth);
			return annotation;
		}

		private void ApplyColor(Color col)
		{
			foreach (var line in children)
			{
				line?.SetColor(col);
			}
		}

		private void ApplyLineWidth(float width)
		{
			foreach (var line in children)
			{
				line?.SetLineWidth(width);
			}
		}
	}
}