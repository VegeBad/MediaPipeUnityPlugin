using Unity.Mathematics;
using UnityEngine;

namespace EugeneC.Utilities
{
    public static partial class HelperCollection
    {
	    public static void ActivateDisplay()
	    {
		    for (var i = 1; i < Display.displays.Length; i++)
		    {
			    Display.displays[i].Activate();
		    }
	    }
	    
	    public static (float, float) GetBoundingBoxSize(this RectTransform rectTransform)
	    {
		    var rect = rectTransform.rect;
		    var center = rect.center;

		    var topLeftRel  = new float2(rect.xMin - center.x, rect.yMin - center.y);
		    var topRightRel = new float2(rect.xMax - center.x, rect.yMin - center.y);

		    // Rotate in 2D using Z (RectTransform is effectively 2D around Z)
		    var zRad = math.radians(rectTransform.localEulerAngles.z);
		    var sin = math.sin(zRad);
		    var cos = math.cos(zRad);

		    var rotatedTopLeftRel  = Rotate(topLeftRel);
		    var rotatedTopRightRel = Rotate(topRightRel);

		    var wMax = math.max(math.abs(rotatedTopLeftRel.x), math.abs(rotatedTopRightRel.x));
		    var hMax = math.max(math.abs(rotatedTopLeftRel.y), math.abs(rotatedTopRightRel.y));

		    return (2f * wMax, 2f * hMax);

		    float2 Rotate(float2 rel) => new (cos * rel.x - sin * rel.y, sin * rel.x + cos * rel.y);
	    }
    }
}
