// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
	public class HandLandmarkerRunner : VisionTaskApiRunner<HandLandmarker>
	{
		[SerializeField] private HandLandmarkerResultAnnotationController handLandmarkerResultAnnotationController;

		private Experimental.TextureFramePool _textureFramePool;

		public readonly HandLandmarkDetectionConfig Config = new();

		public override void Stop()
		{
			base.Stop();
			_textureFramePool?.Dispose();
			_textureFramePool = null;
		}

		protected override IEnumerator Run()
		{
#if UNITY_EDITOR
			Debug.Log($"Delegate = {Config.Delegate}");
			Debug.Log($"Image Read Mode = {Config.ImageReadMode}");
			Debug.Log($"Running Mode = {Config.RunningMode}");
			Debug.Log($"NumHands = {Config.NumHands}");
			Debug.Log($"MinHandDetectionConfidence = {Config.MinHandDetectionConfidence}");
			Debug.Log($"MinHandPresenceConfidence = {Config.MinHandPresenceConfidence}");
			Debug.Log($"MinTrackingConfidence = {Config.MinTrackingConfidence}");
#endif
			yield return AssetLoader.PrepareAssetAsync(Config.ModelPath);

			var options = Config.GetHandLandmarkerOptions(
				Config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnHandLandmarkDetectionOutput : null);
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
			_textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight,
				TextureFormat.RGBA32, 10);

			// NOTE: The screen will be resized later, keeping the aspect ratio.
			screen.Initialize(imageSource);

			SetupAnnotationController(handLandmarkerResultAnnotationController, imageSource);

			var transformationOptions = imageSource.GetTransformationOptions();
			var flipHorizontally = transformationOptions.flipHorizontally;
			var flipVertically = transformationOptions.flipVertically;
			var imageProcessingOptions =
				new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

			AsyncGPUReadbackRequest req = default;
			var waitUntilReqDone = new WaitUntil(() => req.done);
			var waitForEndOfFrame = new WaitForEndOfFrame();
			var result = HandLandmarkerResult.Alloc(options.numHands);

			// NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
			var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 &&
			                     GpuManager.GpuResources != null;
			using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

			while (true)
			{
				if (isPaused)
				{
					yield return new WaitWhile(() => isPaused);
				}

				if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
				{
					yield return new WaitForEndOfFrame();
					continue;
				}

				// Build the input Image
				Image image;
				switch (Config.ImageReadMode)
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
					case Tasks.Vision.Core.RunningMode.IMAGE:
						handLandmarkerResultAnnotationController.DrawNow(
							TaskApi.TryDetect(image, imageProcessingOptions, ref result) ? result : default);

						break;
					case Tasks.Vision.Core.RunningMode.VIDEO:
						handLandmarkerResultAnnotationController.DrawNow(
							TaskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions,
								ref result)
								? result
								: default);
						break;
					case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
						TaskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
						break;
				}
			}
		}

		private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Image image, long timestamp)
		{
			handLandmarkerResultAnnotationController.DrawLater(result);
		}
	}
}