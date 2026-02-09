using System;
using System.Collections;
using System.Diagnostics;
using EugeneC.Singleton;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using Unity.Mathematics;
using UnityEngine;
using Screen = Mediapipe.Unity.Screen;

namespace ProjectionMapping
{
	[DisallowMultipleComponent]
    public abstract class MappingSolution<T, TU> : GenericSingleton<TU>
		where T : BaseVisionTaskApi
		where TU : MonoBehaviour
    {
	    [SerializeField] private Bootstrap prefab;
	    [SerializeField] protected Screen screen;
	    
	    protected Bootstrap Bootstrap;
	    protected bool IsPaused;
	    private Coroutine _coroutine;
	    private readonly Stopwatch _stopwatch = new();

	    protected T TaskApi;

	    protected virtual IEnumerator Start()
	    {
		    KeepSingleton(true);
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
		    IsPaused = false;
		    _stopwatch.Restart();
		    _coroutine = StartCoroutine(Run());
	    }
	    
	    protected abstract IEnumerator Run();

	    /// <summary>
	    ///   Pause the main program.
	    /// </summary>
	    public virtual void Pause()
	    {
		    IsPaused = true;
		    ImageSourceProvider.ImageSource.Pause();
	    }

	    /// <summary>
	    ///    Resume the main program.
	    ///    If the main program has not begun, it'll do nothing.
	    /// </summary>
	    public virtual void Resume()
	    {
		    IsPaused = false;
		    StartCoroutine(ImageSourceProvider.ImageSource.Resume());
	    }

	    /// <summary>
	    ///   Stops the main program.
	    /// </summary>
	    public virtual void Stop()
	    {
		    IsPaused = true;
		    _stopwatch.Stop();
		    StopCoroutine(_coroutine);
		    ImageSourceProvider.ImageSource.Stop();
		    TaskApi?.Close();
		    TaskApi = null;
	    }

	    protected long GetCurrentTimestampMilliSec() =>
		    _stopwatch.IsRunning ? _stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond : -1;
	    
	    protected static void SetupAnnotationController<TA>(AnnotationController<TA> annotationController,
		    ImageSource imageSource, bool expectedToBeMirrored = false)
		    where TA : HierarchicalAnnotation
	    {
		    annotationController.isMirrored = expectedToBeMirrored;
		    annotationController.imageSize = new int2(imageSource.textureWidth, imageSource.textureHeight);
	    }
    }
}
