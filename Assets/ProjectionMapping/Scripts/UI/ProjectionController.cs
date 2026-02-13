using System;
using EugeneC.Singleton;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace ProjectionMapping
{
    public class ProjectionController : GenericSingleton<ProjectionController>
    {
	    [SerializeField] private TMP_Text poseText;
	    [SerializeField] private TMP_Text dataText;
	    
	    private World _world;
	    
	    private async void Start()
	    {
		    try
		    {
			    await Awaitable.EndOfFrameAsync();
			    _world = World.DefaultGameObjectInjectionWorld;
			    var system = _world.GetExistingSystemManaged<HandDataEventSystemBase>();
			    system.OnPoseChanged += OnGetHandPoses;
			    system.OnHandDataChanged += OnGetHandData;
		    }
		    catch (Exception e){ Debug.Log(e);}
	    }

	    private void OnGetHandPoses(EHandPose left, EHandPose right)
	    {
		    poseText.text = $"Left: {left}\nRight: {right}";
	    }
	    
	    private void OnGetHandData(HandData left, HandData right)
	    {
		    dataText.text = $"Left: \n{DataToText(left)}\nRight: \n{DataToText(right)}";
	    }

	    // Bruh
	    private static string DataToText(HandData data) => $"Wrist2Thumb : {data.Wrist2Thumb.Current:F}\n" +
	                                                       $"Wrist2Index : {data.Wrist2Index.Current:F}\n" +
	                                                       $"Wrist2Middle : {data.Wrist2Middle.Current:F}\n" +
	                                                       $"Wrist2Ring : {data.Wrist2Ring.Current:F}\n" +
	                                                       $"Wrist2Pinky : {data.Wrist2Pinky.Current:F}\n" +
	                                                       $"Thumb2Index : {data.Thumb2Index.Current:F}\n" +
	                                                       $"Index2Middle : {data.Index2Middle.Current:F}\n" +
	                                                       $"Middle2Ring : {data.Middle2Ring.Current:F}\n" +
	                                                       $"Ring2Pinky : {data.Ring2Pinky.Current:F}\n" +
	                                                       $"Pinky2Thumb : {data.Pinky2Thumb.Current:F}\n";
    }
}
