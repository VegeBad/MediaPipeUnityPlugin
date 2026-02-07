using UnityEngine;

namespace EugeneC.Utilities
{
	[RequireComponent(typeof(RectTransform))]
    public sealed class AutoResize : MonoBehaviour
    {
        [SerializeField] private FitMode fitMode;
        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;

        private void OnEnable()
        {
	        _rectTransform = GetComponent<RectTransform>();
	        _parentRectTransform = transform.parent?.GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
	        if (_parentRectTransform is null) return;
	        if(_rectTransform.rect.width == 0 || _rectTransform.rect.height == 0) return;
	        var (width, height) = _rectTransform.GetBoundingBoxSize();
	        var ratio = _parentRectTransform.rect.width / width;
	        var newHeight = height * ratio;

	        if (fitMode == FitMode.FitWidth || (fitMode == FitMode.Expand && newHeight >= _parentRectTransform.rect.height) ||
	            (fitMode == FitMode.Shrink && newHeight <= _parentRectTransform.rect.height))
	        {
		        _rectTransform.offsetMin *= ratio;
		        _rectTransform.offsetMax *= ratio;
		        return;
	        }

	        ratio = _rectTransform.rect.height / height;

	        _rectTransform.offsetMin *= ratio;
	        _rectTransform.offsetMax *= ratio;
        }
    }
}
