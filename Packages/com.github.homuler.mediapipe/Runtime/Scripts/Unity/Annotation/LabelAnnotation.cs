// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
	using Color = UnityEngine.Color;

#pragma warning restore IDE0065

	public class LabelAnnotation : HierarchicalAnnotation
	{
		[SerializeField] private Text _labelText;
		[SerializeField] private Transform _backgroundTransform;

		public void Draw(string text, float3 position, Color color, float maxWidth, float maxHeight)
		{
			if (!ActivateFor(text)) return;
			// move to the front to show background plane.
			_labelText.transform.localPosition = new float3(position.x, position.y, -1);
			_labelText.transform.localRotation = quaternion.Euler(0, 0, -(int)rotationAngle);
			_labelText.text = text;
			_labelText.color = DecideTextColor(color);
			_labelText.fontSize = GetFontSize(text, maxWidth, math.min(maxHeight, 48.0f));

			var width = math.min(_labelText.preferredWidth + 24, maxWidth); // add margin
			var height = _labelText.preferredHeight;
			var rectTransform = _labelText.GetComponent<RectTransform>();
			rectTransform.sizeDelta = new float2(width, height);

			_backgroundTransform.localScale = new float3(width / 10, 1, height / 10);
			_backgroundTransform.gameObject.GetComponent<Renderer>().material.color = color;
		}

		private int GetFontSize(string text, float maxWidth, float maxHeight)
		{
			var ch = math.min(maxWidth / text.Length, maxHeight);
			return (int)math.clamp(ch, 24.0f, 72.0f);
		}

		private Color DecideTextColor(Color backgroundColor)
		{
			var lw = CalcContrastRatio(Color.white, backgroundColor);
			var lb = CalcContrastRatio(backgroundColor, Color.black);
			return lw < lb ? Color.black : Color.white;
		}

		private float CalcRelativeLuminance(Color color)
		{
			var r = color.r <= 0.03928f ? color.r / 12.92f : math.pow((color.r + 0.055f) / 1.055f, 2.4f);
			var g = color.g <= 0.03928f ? color.g / 12.92f : math.pow((color.g + 0.055f) / 1.055f, 2.4f);
			var b = color.b <= 0.03928f ? color.b / 12.92f : math.pow((color.b + 0.055f) / 1.055f, 2.4f);
			return (0.2126f * r) + (0.7152f * g) + (0.0722f * b);
		}

		private float CalcContrastRatio(Color lighter, Color darker)
		{
			var l1 = CalcRelativeLuminance(lighter);
			var l2 = CalcRelativeLuminance(darker);
			return (l1 + 0.05f) / (l2 + 0.05f);
		}

		private bool ActivateFor(string text) => base.ActivateFor(string.IsNullOrEmpty(text) ? null : text);
	}
}