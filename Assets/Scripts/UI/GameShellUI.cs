using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SurvivalGame.Core;

namespace SurvivalGame.UI
{
    public class GameShellUI : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

        [Header("Game Over / Victory Labels")]
        [SerializeField] private TextMeshProUGUI gameOverStatsText;
        [SerializeField] private TextMeshProUGUI victoryStatsText;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButtonGameOver;
        [SerializeField] private Button restartButtonVictory;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            GameLoop.OnStateChanged += HandleStateChanged;

            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (restartButtonGameOver != null) restartButtonGameOver.onClick.AddListener(Restart);
            if (restartButtonVictory != null) restartButtonVictory.onClick.AddListener(Restart);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);

            HideAllPanels();
        }

        private void OnDestroy()
        {
            GameLoop.OnStateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            if (GameInput.PausePressed)
            {
                if (GameLoop.Instance != null && (GameLoop.Instance.State == RunState.Running || GameLoop.Instance.State == RunState.Paused))
                {
                    GameLoop.Instance.TogglePause();
                }
            }
        }

        private void HandleStateChanged(RunState newState)
        {
            HideAllPanels();

            switch (newState)
            {
                case RunState.Paused:
                    if (pausePanel != null) pausePanel.SetActive(true);
                    break;
                case RunState.Lost:
                    ShowGameOver();
                    break;
                case RunState.Won:
                    ShowVictory();
                    break;
                case RunState.Running:
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;
            }
        }

        private void ShowGameOver()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (gameOverStatsText != null && GameLoop.Instance != null)
            {
                gameOverStatsText.text = $"You succumbed to the elements.\nDays Survived: {GameLoop.Instance.DaysSurvived}";
            }
        }

        private void ShowVictory()
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (victoryStatsText != null && GameLoop.Instance != null)
            {
                victoryStatsText.text = $"You conquered the wilderness!\nDays Survived: {GameLoop.Instance.DaysSurvived}";
            }
        }

        private void HideAllPanels()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
        }

        private void Resume()
        {
            GameLoop.Instance?.TogglePause();
        }

        private void Restart()
        {
            GameLoop.Instance?.RestartGame();
        }

        private void Quit()
        {
            GameLoop.Instance?.QuitGame();
        }
    }
}
