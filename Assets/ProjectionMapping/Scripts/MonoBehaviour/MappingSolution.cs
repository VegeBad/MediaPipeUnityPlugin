using System;
using System.Collections;
using System.Diagnostics;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using Unity.Mathematics;
using UnityEngine;
using Screen = UnityEngine.Screen;

namespace ProjectionMapping
{
	[DisallowMultipleComponent]
    public abstract class MappingSolution<T> : MonoBehaviour
		where T : BaseVisionTaskApi
    {
	    [SerializeField] private Bootstrap prefab;
	    [SerializeField] protected Screen screen;
	    
	    protected Bootstrap Bootstrap;
	    private bool _isPaused;
	    private Coroutine _coroutine;
	    private readonly Stopwatch _stopwatch = new();

	    protected T TaskApi;

	    protected virtual IEnumerator Start()
	    {
		    Bootstrap = Instantiate(prefab, transform);
		    yield return new WaitUntil(() => Bootstrap.IsFinished);
		    
		    Play();
	    }
	    
	    /// <summary>
	    ///   Start the main program from the beginning.
	    /// </summary>
	    public virtual void Play()
	    {
		    if (_coroutine != null) Stop();
		    _isPaused = false;
		    _stopwatch.Restart();
		    _coroutine = StartCoroutine(Run());
	    }
	    
	    protected abstract IEnumerator Run();

	    /// <summary>
	    ///   Pause the main program.
	    /// </summary>
	    public virtual void Pause()
	    {
		    _isPaused = true;
		    ImageSourceProvider.ImageSource.Pause();
	    }

	    /// <summary>
	    ///    Resume the main program.
	    ///    If the main program has not begun, it'll do nothing.
	    /// </summary>
	    public virtual void Resume()
	    {
		    _isPaused = false;
		    StartCoroutine(ImageSourceProvider.ImageSource.Resume());
	    }

	    /// <summary>
	    ///   Stops the main program.
	    /// </summary>
	    public virtual void Stop()
	    {
		    _isPaused = true;
		    _stopwatch.Stop();
		    StopCoroutine(_coroutine);
		    ImageSourceProvider.ImageSource.Stop();
		    TaskApi?.Close();
		    TaskApi = null;
	    }

	    protected long GetCurrentTimestampMilliSec() =>
		    _stopwatch.IsRunning ? _stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond : -1;
	    
	    protected static void SetupAnnotationController<TU>(AnnotationController<TU> annotationController,
		    ImageSource imageSource, bool expectedToBeMirrored = false)
		    where TU : HierarchicalAnnotation
	    {
		    annotationController.isMirrored = expectedToBeMirrored;
		    annotationController.imageSize = new int2(imageSource.textureWidth, imageSource.textureHeight);
	    }
    }
}
