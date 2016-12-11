using UnityEngine;
using UnityEngine.UI;

namespace MaxPostnikov.LD37
{
    public class GameOverPopup : MonoBehaviour
    {
        public Text scoreText;
        public Button playAgainButton;

        IUIController controller;

        public void Init(IUIController controller)
        {
            this.controller = controller;

            playAgainButton.onClick.AddListener(OnPlayAgain);
        }

        public void Show(int score)
        {
            scoreText.text = score.ToString();

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void OnPlayAgain()
        {
            Hide();

            controller.Restart();
        }
    }
}
