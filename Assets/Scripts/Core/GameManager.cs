using System;
using UnityEngine;
using BeachRunner.Data;

namespace BeachRunner.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Session")]
        [SerializeField] private float distanceMultiplier = 1.8f;
        [SerializeField] private int scorePerCoin = 10;

        [Header("Runtime")]
        [SerializeField] private CharacterDatabase characterDatabase;

        public GameState State { get; private set; } = GameState.Splash;
        public float Distance { get; private set; }
        public int Coins { get; private set; }
        public int Score { get; private set; }
        public int BestScore { get; private set; }
        public CharacterDefinition SelectedCharacter { get; private set; }

        public event Action<GameState> OnStateChanged;
        public event Action<int, float, int> OnHudChanged;

        private bool _isSessionActive;

        private const string BestScoreKey = "A_A_BEST_SCORE";
        private const string SelectedCharacterKey = "A_A_SELECTED_CHARACTER";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);

            var selectedId = PlayerPrefs.GetString(SelectedCharacterKey, string.Empty);
            SelectedCharacter = characterDatabase.GetById(selectedId) ?? characterDatabase.DefaultCharacter;
        }

        private void Update()
        {
            if (!_isSessionActive) return;

            Distance += Time.deltaTime * RunnerSpeedRuntime.CurrentSpeed;
            Score = Mathf.FloorToInt(Distance * distanceMultiplier) + Coins * scorePerCoin;
            OnHudChanged?.Invoke(Coins, Distance, Score);
        }

        public void ChangeState(GameState next)
        {
            State = next;
            _isSessionActive = next == GameState.Playing;
            OnStateChanged?.Invoke(State);
        }

        public void StartRun()
        {
            Distance = 0f;
            Coins = 0;
            Score = 0;
            RunnerSpeedRuntime.ResetRuntime();
            ChangeState(GameState.Playing);
            OnHudChanged?.Invoke(Coins, Distance, Score);
        }

        public void AddCoin(int amount = 1)
        {
            Coins += amount;
            OnHudChanged?.Invoke(Coins, Distance, Score);
        }

        public void GameOver()
        {
            _isSessionActive = false;
            if (Score > BestScore)
            {
                BestScore = Score;
                PlayerPrefs.SetInt(BestScoreKey, BestScore);
            }

            ChangeState(GameState.GameOver);
        }

        public void SetCharacter(string id)
        {
            var candidate = characterDatabase.GetById(id);
            if (candidate == null) return;
            SelectedCharacter = candidate;
            PlayerPrefs.SetString(SelectedCharacterKey, id);
        }
    }
}
