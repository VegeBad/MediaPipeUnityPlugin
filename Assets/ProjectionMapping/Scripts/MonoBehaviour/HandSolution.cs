using System.Collections;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ProjectionMapping
{
    public class HandSolution : MappingSolution<HandLandmarker>
    {
	    protected override IEnumerator Run()
	    {
		    yield return null;
	    }
    }
}
