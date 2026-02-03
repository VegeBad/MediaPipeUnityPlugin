using System.Globalization;
using UnityEngine;

namespace EugeneC.Utilities
{
	[AddComponentMenu("Eugene/FPS Counter")]
	public sealed class MonoFpsCounter : MonoBehaviour
	{
		[SerializeField] private TMPro.TMP_Text displayText;

		public float FPS => _frames / _time;

		private float _frames;
		private float _time;

		private void Start()
		{
			Application.targetFrameRate = -1;
			InvokeRepeating(nameof(UpdateText), .1f, .5f);
		}

		private void Update()
		{
			_time += Time.unscaledDeltaTime;
			_frames++;

			if (_time < 0.5f) return;
			_frames = 0;
			_time = 0;
		}

		private void UpdateText()
		{
			if (displayText is null) return;
			displayText.text = $"FPS: {FPS:f0}";
		}
	}
}