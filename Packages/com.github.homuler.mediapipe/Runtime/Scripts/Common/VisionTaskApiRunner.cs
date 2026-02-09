// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Mediapipe.Unity.Sample
{
	public abstract class VisionTaskApiRunner<T> : BaseRunner
		where T : Tasks.Vision.Core.BaseVisionTaskApi
	{
		[SerializeField] protected Screen screen;

		private Coroutine _coroutine;
		protected T TaskApi;

		public RunningMode runningMode;

		public override void Play()
		{
			if (_coroutine != null)
			{
				Stop();
			}

			base.Play();
			_coroutine = StartCoroutine(Run());
		}

		public override void Pause()
		{
			base.Pause();
			ImageSourceProvider.ImageSource.Pause();
		}

		public override void Resume()
		{
			base.Resume();
			var _ = StartCoroutine(ImageSourceProvider.ImageSource.Resume());
		}

		public override void Stop()
		{
			base.Stop();
			StopCoroutine(_coroutine);
			ImageSourceProvider.ImageSource.Stop();
			TaskApi?.Close();
			TaskApi = null;
		}

		protected abstract IEnumerator Run();

		protected static void SetupAnnotationController<U>(AnnotationController<U> annotationController,
			ImageSource imageSource, bool expectedToBeMirrored = false)
			where U : HierarchicalAnnotation
		{
			annotationController.isMirrored = expectedToBeMirrored;
			annotationController.imageSize = new int2(imageSource.textureWidth, imageSource.textureHeight);
		}
	}
}