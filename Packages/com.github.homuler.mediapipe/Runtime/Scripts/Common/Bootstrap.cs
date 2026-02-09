// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using UnityEngine;

namespace Mediapipe.Unity.Sample
{
	[DisallowMultipleComponent]
	public sealed class Bootstrap : MonoBehaviour
	{
		[SerializeField] private AppSettings appSettings;

		public InferenceMode InferenceMode { get; private set; }
		public bool IsFinished { get; private set; }
		private bool _isGlogInitialized;

		private void OnEnable()
		{
			StartCoroutine(Init());
		}

		private IEnumerator Init()
		{
#if !DEBUG && !DEVELOPMENT_BUILD
      Debug.LogWarning("Logging for the MediaPipeUnityPlugin will be suppressed. To enable logging, please check the 'Development Build' option and build.");
#endif

			Logger.MinLogLevel = appSettings.logLevel;

			Protobuf.SetLogHandler(Protobuf.DefaultLogHandler);

			Debug.Log("Setting global flags...");
			appSettings.ResetGlogFlags();
			Glog.Initialize("MediaPipeUnityPlugin");
			_isGlogInitialized = true;

			Debug.Log("Initializing AssetLoader...");
			switch (appSettings.assetLoaderType)
			{
				case AppSettings.AssetLoaderType.AssetBundle:
				{
					AssetLoader.Provide(new AssetBundleResourceManager("mediapipe"));
					break;
				}
				case AppSettings.AssetLoaderType.StreamingAssets:
				{
					AssetLoader.Provide(new StreamingAssetsResourceManager());
					break;
				}
				case AppSettings.AssetLoaderType.Local:
				{
#if UNITY_EDITOR
					AssetLoader.Provide(new LocalResourceManager());
					break;
#else
            Debug.LogError("LocalResourceManager is only supported on UnityEditor." +
              "To avoid this error, consider switching to the StreamingAssetsResourceManager and copying the required resources under StreamingAssets, for example.");
            yield break;
#endif
				}
				default:
				{
#if UNITY_EDITOR					
					Debug.LogError($"AssetLoaderType is unknown: {appSettings.assetLoaderType}");
#endif					
					yield break;
				}
			}

			DecideInferenceMode();
			if (InferenceMode == InferenceMode.GPU)
			{
#if UNITY_EDITOR				
				Debug.Log("Initializing GPU resources...");
#endif				
				yield return GpuManager.Initialize();

				if (!GpuManager.IsInitialized)
				{
#if UNITY_EDITOR					
					Debug.LogWarning(
						"If your native library is built for CPU, change 'Preferable Inference Mode' to CPU from the Inspector Window for AppSettings");
#endif					
				}
			}
#if UNITY_EDITOR			
			Debug.Log("Preparing ImageSource...");
#endif			
			ImageSourceProvider.Initialize(
				appSettings.BuildWebCamSource(), appSettings.BuildStaticImageSource(), appSettings.BuildVideoSource());
			ImageSourceProvider.Switch(appSettings.defaultImageSource);

			IsFinished = true;
		}

		private void DecideInferenceMode()
		{
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
			if (appSettings.preferableInferenceMode == InferenceMode.GPU)
			{
#if UNITY_EDITOR				
				Debug.LogWarning("Current platform does not support GPU inference mode, so falling back to CPU mode");
#endif				
			}

			InferenceMode = InferenceMode.CPU;
#else
      InferenceMode = appSettings.preferableInferenceMode;
#endif
		}

		private void OnApplicationQuit()
		{
			GpuManager.Shutdown();

			if (_isGlogInitialized)
			{
				Glog.Shutdown();
			}

			Protobuf.ResetLogHandler();
		}
	}
}