using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivalGame.Core
{
    public enum RunState
    {
        Ready,
        Running,
        Paused,
        Won,
        Lost
    }

    /// <summary>
    /// Single owner of the game loop lifecycle (Ready -> Running -> Won/Lost/Paused).
    /// Dispatches state changes to UI, audio, and gameplay systems without tight coupling.
    /// </summary>
    public class GameLoop : MonoBehaviour
    {
        public static GameLoop Instance { get; private set; }

        [Header("Run Goals & Pressure")]
        [Tooltip("Number of days/nights the player must survive to achieve victory (0 for endless).")]
        [SerializeField] private int targetDaysToSurvive = 5;

        public RunState State { get; private set; } = RunState.Ready;
        public int DaysSurvived { get; private set; } = 0;
        public int TargetDays => targetDaysToSurvive;

        public static event Action<RunState> OnStateChanged;
        public static event Action<int, int> OnDayProgressChanged; // currentDay, targetDay

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // The main menu owns the start of a run when present.
            if (FindFirstObjectByType<SurvivalGame.UI.GameUI>() == null)
            {
                StartRun();
            }
        }

        public void StartRun()
        {
            DaysSurvived = 0;
            Time.timeScale = 1f;
            SetState(RunState.Running);
            OnDayProgressChanged?.Invoke(DaysSurvived, targetDaysToSurvive);
        }

        public void RegisterDayCompleted()
        {
            if (State != RunState.Running) return;

            DaysSurvived++;
            OnDayProgressChanged?.Invoke(DaysSurvived, targetDaysToSurvive);

            if (targetDaysToSurvive > 0 && DaysSurvived >= targetDaysToSurvive)
            {
                Win();
            }
        }

        public void Win()
        {
            if (State != RunState.Running) return;
            SetState(RunState.Won);
            Time.timeScale = 0.2f; // Dramatic slow down before win screen
        }

        public void Lose()
        {
            if (State != RunState.Running) return;
            SetState(RunState.Lost);
            Time.timeScale = 0.2f; // Dramatic slow down before death screen
        }

        public void TogglePause()
        {
            if (State == RunState.Running)
            {
                SetState(RunState.Paused);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (State == RunState.Paused)
            {
                SetState(RunState.Running);
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SetState(RunState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
