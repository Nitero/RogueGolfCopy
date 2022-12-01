using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private RoundController _rounds;
    [SerializeField] private GameObject _gameOverParent;
    [SerializeField] private GameObject _gameOverToggledPanel;
    [SerializeField] private TextMeshProUGUI _gameOverRoundsText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _toggleHideButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private TextMeshProUGUI _toggleHideButtonText;
    
    private dreamloLeaderBoard _leaderboard;

    public void Open()
    {
        _gameOverParent.SetActive(true);
        
        int score = _rounds.Round - 1;
        _gameOverRoundsText.text = $"Scored {_rounds.Round-1} Holes";
        
        if (PlayerPrefs.HasKey(Constants.PLAYER_HIGHSCORE_PLAYER_PREFS_KEY))
        {
            if (PlayerPrefs.GetInt(Constants.PLAYER_HIGHSCORE_PLAYER_PREFS_KEY) < score)
                SetHighScore(score);
        }
        else
            SetHighScore(score);
    }

    private void SetHighScore(int score)
    {
        PlayerPrefs.SetInt(Constants.PLAYER_HIGHSCORE_PLAYER_PREFS_KEY, score);
        
        _leaderboard = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
        _leaderboard.AddScore(SystemInfo.deviceUniqueIdentifier, score, 0, PlayerPrefs.GetString(Constants.PLAYER_NAME_PLAYER_PREFS_KEY));
    }

    private void OnEnable()
    {
        _restartButton.onClick.AddListener(OnRestartButtonClicked);
        _toggleHideButton.onClick.AddListener(OnHideButtonClicked);
        _menuButton.onClick.AddListener(OnMenuButtonClicked);
    }

    private void OnDisable()
    {
        _restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        _toggleHideButton.onClick.RemoveListener(OnHideButtonClicked);
        _menuButton.onClick.RemoveListener(OnMenuButtonClicked);
    }

    private void OnRestartButtonClicked()
    {
        PlayerPrefs.SetInt(Constants.RESTART_PLAYER_PREFS_KEY, 1);
        Application.LoadLevel(Application.loadedLevel);
    }

    private void OnHideButtonClicked()
    {
        _gameOverToggledPanel.SetActive(!_gameOverToggledPanel.activeSelf);
        _toggleHideButtonText.text = _gameOverToggledPanel.activeSelf ? "Hide" : "Show";
    }

    private void OnMenuButtonClicked()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
}