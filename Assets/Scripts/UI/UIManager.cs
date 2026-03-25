using TMPro;
using UnityEngine;
using BeachRunner.Core;
using BeachRunner.Audio;

namespace BeachRunner.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private CanvasGroup splashPanel;
        [SerializeField] private CanvasGroup mainMenuPanel;
        [SerializeField] private CanvasGroup characterSelectPanel;
        [SerializeField] private CanvasGroup hudPanel;
        [SerializeField] private CanvasGroup pausePanel;
        [SerializeField] private CanvasGroup gameOverPanel;

        [Header("HUD")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text distanceText;
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private TMP_Text gameOverScoreText;
        [SerializeField] private TMP_Text bestScoreText;

        private void OnEnable()
        {
            GameManager.Instance.OnStateChanged += HandleState;
            GameManager.Instance.OnHudChanged += UpdateHud;
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnStateChanged -= HandleState;
            GameManager.Instance.OnHudChanged -= UpdateHud;
        }

        private void Start()
        {
            HandleState(GameManager.Instance.State);
            UpdateHud(0, 0f, 0);
        }

        public void OnTapPlay()
        {
            AudioBus.Instance.PlayButton();
            GameManager.Instance.ChangeState(GameState.CharacterSelect);
        }

        public void OnTapStartRun()
        {
            AudioBus.Instance.PlayButton();
            GameManager.Instance.StartRun();
        }

        public void OnTapPause()
        {
            AudioBus.Instance.PlayButton();
            Time.timeScale = 0f;
            GameManager.Instance.ChangeState(GameState.Paused);
        }

        public void OnTapResume()
        {
            AudioBus.Instance.PlayButton();
            Time.timeScale = 1f;
            GameManager.Instance.ChangeState(GameState.Playing);
        }

        public void OnTapRestart()
        {
            AudioBus.Instance.PlayButton();
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
        }

        public void OnTapMainMenu()
        {
            AudioBus.Instance.PlayButton();
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void HandleState(GameState state)
        {
            SetVisible(splashPanel, state == GameState.Splash);
            SetVisible(mainMenuPanel, state == GameState.MainMenu);
            SetVisible(characterSelectPanel, state == GameState.CharacterSelect);
            SetVisible(hudPanel, state == GameState.Playing);
            SetVisible(pausePanel, state == GameState.Paused);
            SetVisible(gameOverPanel, state == GameState.GameOver);

            if (state == GameState.GameOver)
            {
                AudioBus.Instance.PlayGameOver();
                gameOverScoreText.text = $"SCORE  {GameManager.Instance.Score}";
                bestScoreText.text = $"BEST  {GameManager.Instance.BestScore}";
            }
        }

        private void UpdateHud(int coins, float distance, int score)
        {
            coinText.text = coins.ToString("000");
            distanceText.text = $"{distance:0}m";
            scoreText.text = score.ToString("00000");
        }

        private static void SetVisible(CanvasGroup group, bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
