using UnityEngine;

namespace MaxPostnikov.LD37
{
    public class ProgressBar : MonoBehaviour
    {
        public Transform fillImage;

        Vector3 scale;

        public void SetProgress(float value)
        {
            scale = fillImage.localScale;
            scale.x = value;
            fillImage.localScale = scale;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
