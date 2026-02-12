using System.Collections;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Experimental;
using Mediapipe.Unity.Sample;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;
using UnityEngine.Rendering;
using RunningMode = Mediapipe.Tasks.Vision.Core.RunningMode;
using Image = Mediapipe.Image;

namespace ProjectionMapping
{
	[DisallowMultipleComponent]
    public sealed class HandSolution : MappingSolution<HandLandmarker, HandSolution>
    {
	    [SerializeField] private BridgeResultAnnotationController bridgeController;

		private TextureFramePool _textureFramePool;

		private readonly HandLandmarkDetectionConfig _config = new();

		public override void Stop()
		{
			base.Stop();
			_textureFramePool?.Dispose();
			_textureFramePool = null;
		}

		protected override IEnumerator Run()
		{
#if UNITY_EDITOR
			Debug.Log($"Delegate = {_config.Delegate}");
			Debug.Log($"Image Read Mode = {_config.ImageReadMode}");
			Debug.Log($"Running Mode = {_config.RunningMode}");
			Debug.Log($"NumHands = {_config.NumHands}");
			Debug.Log($"MinHandDetectionConfidence = {_config.MinHandDetectionConfidence}");
			Debug.Log($"MinHandPresenceConfidence = {_config.MinHandPresenceConfidence}");
			Debug.Log($"MinTrackingConfidence = {_config.MinTrackingConfidence}");
#endif
			yield return AssetLoader.PrepareAssetAsync(_config.ModelPath);

			var options = _config.GetHandLandmarkerOptions(
				_config.RunningMode == RunningMode.LIVE_STREAM ? OnHandLandmarkDetectionOutput : null);
			TaskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
			var imageSource = ImageSourceProvider.ImageSource;

			yield return imageSource.Play();

			if (!imageSource.isPrepared)
			{
				Debug.LogError("Failed to start ImageSource, exiting...");
				yield break;
			}

			// Use RGBA32 as the input format.
			// TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so maybe the following code needs to be fixed.
			_textureFramePool = new TextureFramePool(imageSource.textureWidth, imageSource.textureHeight,
				TextureFormat.RGBA32, 10);

			// NOTE: The screen will be resized later, keeping the aspect ratio.
			screen.Initialize(imageSource);

			SetupAnnotationController(bridgeController, imageSource);

			var transformationOptions = imageSource.GetTransformationOptions();
			var flipHorizontally = transformationOptions.flipHorizontally;
			var flipVertically = transformationOptions.flipVertically;
			var imageProcessingOptions =
				new ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

			AsyncGPUReadbackRequest req = default;
			var req1 = req;
			var waitUntilReqDone = new WaitUntil(() => req1.done);
			var waitForEndOfFrame = new WaitForEndOfFrame();
			var result = HandLandmarkerResult.Alloc(options.numHands);

			// NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
			var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 &&
			                     GpuManager.GpuResources != null;
			using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

			while (true)
			{
				if (IsPaused)
				{
					yield return new WaitWhile(() => IsPaused);
				}

				if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
				{
					yield return waitForEndOfFrame;
					continue;
				}

				// Build the input Image
				Image image;
				switch (_config.ImageReadMode)
				{
					case ImageReadMode.GPU:
						if (!canUseGpuImage)
						{
							throw new System.Exception("ImageReadMode.GPU is not supported");
						}

						textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally,
							flipVertically);
						image = textureFrame.BuildGPUImage(glContext);
						// TODO: Currently we wait here for one frame to make sure the texture is fully copied to the TextureFrame before sending it to MediaPipe.
						// This usually works but is not guaranteed. Find a proper way to do this. See: https://github.com/homuler/MediaPipeUnityPlugin/pull/1311
						yield return waitForEndOfFrame;
						break;
					case ImageReadMode.CPU:
						yield return waitForEndOfFrame;
						textureFrame.ReadTextureOnCPU(imageSource.GetCurrentTexture(), flipHorizontally,
							flipVertically);
						image = textureFrame.BuildCPUImage();
						textureFrame.Release();
						break;
					case ImageReadMode.CPUAsync:
					default:
						req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally,
							flipVertically);
						yield return waitUntilReqDone;

						if (req.hasError)
						{
							Debug.LogWarning($"Failed to read texture from the image source");
							continue;
						}

						image = textureFrame.BuildCPUImage();
						textureFrame.Release();
						break;
				}

				switch (TaskApi.RunningMode)
				{
					case RunningMode.IMAGE:
						bridgeController.DrawNow(
							TaskApi.TryDetect(image, imageProcessingOptions, ref result) ? result : default);
						break;
					case RunningMode.VIDEO:
						bridgeController.DrawNow(
							TaskApi.TryDetectForVideo(image, GetCurrentTimestampMilliSec(), imageProcessingOptions, ref result) ? result : default);
						break;
					case RunningMode.LIVE_STREAM:
						TaskApi.DetectAsync(image, GetCurrentTimestampMilliSec(), imageProcessingOptions);
						break;
				}
			}
		}

		private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Image image, long timestamp)
		{
			bridgeController.DrawLater(result);
		}
    }
}
