using UnityEngine;
using UnityEngine.UI;

namespace ProjectionMapping
{
	[RequireComponent(typeof(Button))]
    public sealed class UIMirrorButton : MonoBehaviour
    {
        [SerializeField] private Button flipButton;
        [SerializeField] private RectTransform rectTransform;

        private void Start()
        {
	        flipButton = GetComponent<Button>();
	        flipButton.onClick.AddListener(Flip);
        }

        private void Flip()
        {
            var euler = rectTransform.localEulerAngles;
            euler.y = euler.y < 90 ? 180f : 0f;
            rectTransform.localEulerAngles = euler;
        }
    }
}
