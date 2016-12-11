using UnityEngine;
using UnityEngine.UI;

namespace MaxPostnikov.LD37
{
    public class ProgressBar : MonoBehaviour
    {
        public Text scoreText;
        public Transform fillImage;

        Vector3 scale;

        public void SetScore(int value)
        {
            scoreText.text = value.ToString();
        }

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
