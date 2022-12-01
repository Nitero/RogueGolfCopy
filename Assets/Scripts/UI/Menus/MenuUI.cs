using Controllers;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private RoundUI _roundUI;
    [SerializeField] private GameObject _menuParent;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private SetPlayerNameUI _setPlayerNameUI;
    [SerializeField] private LeaderboardUI _leaderboard;

    private void Start()
    {
        _roundUI.gameObject.SetActive(false);
        if (PlayerPrefs.HasKey(Constants.RESTART_PLAYER_PREFS_KEY) && PlayerPrefs.GetInt(Constants.RESTART_PLAYER_PREFS_KEY) == 1)
        {
            OnStartButtonClicked();
            PlayerPrefs.SetInt(Constants.RESTART_PLAYER_PREFS_KEY, 0);
        }
        else
            AudioManager.Instance.PlayMusic(AudioManager.Sound.MenuMusic);
    }
    
    private void OnEnable()
    {
        _startButton.onClick.AddListener(OnStartButtonClicked);
        _quitButton.onClick.AddListener(OnQuitButtonClicked);
    }
    private void OnDisable()
    {
        _startButton.onClick.RemoveListener(OnStartButtonClicked);
        _quitButton.onClick.RemoveListener(OnQuitButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        _menuParent.SetActive(false);
        if (!PlayerPrefs.HasKey(Constants.PLAYER_NAME_PLAYER_PREFS_KEY))
            _setPlayerNameUI.gameObject.SetActive(true);
        else
            _roundUI.gameObject.SetActive(true);
            
        AudioManager.Instance.PlayMusic(AudioManager.Sound.GameplayMusic);
    }

    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }
}