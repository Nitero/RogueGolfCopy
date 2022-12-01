using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private LeaderboardEntryUI _leaderboardEntryPrefab;
    [SerializeField] private Transform _entryParent;
    [SerializeField] private GameObject _loadingPlaceholder;
    [SerializeField] private int _shownEntries = 10;
    
    private dreamloLeaderBoard _leaderboard;
    private bool _playerHasScore;
    private bool _playerHasLocalScore;
    private dreamloLeaderBoard.Score _playerScore;
    
    private void Start()
    {
	    _leaderboard = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
	    
        foreach (Transform child in _entryParent)
            Destroy(child.gameObject);
        
        _leaderboard.GetSingleScore(SystemInfo.deviceUniqueIdentifier);
        
        _leaderboard.EventRequestFinished += OnSingleRequestFinished;
        _loadingPlaceholder.SetActive(true);
    }
    
    private void OnSingleRequestFinished()
    {
        _leaderboard.EventRequestFinished -= OnSingleRequestFinished;
        
        List<dreamloLeaderBoard.Score> scores = _leaderboard.ToListHighToLow();
        _playerHasScore = scores.Count > 0;
        if (_playerHasScore)
            _playerScore = scores[0];
        _playerHasLocalScore = PlayerPrefs.HasKey(Constants.PLAYER_HIGHSCORE_PLAYER_PREFS_KEY);
        
        _leaderboard.GetScores();
        _leaderboard.EventRequestFinished += OnAllRequestFinished;
    }
    private void OnAllRequestFinished()
    {
        _leaderboard.EventRequestFinished -= OnAllRequestFinished;
        _loadingPlaceholder.SetActive(false);
        
        List<dreamloLeaderBoard.Score> scores = _leaderboard.ToListHighToLow();
        
        int playerRank = -1;
        bool playerHasShownScore = false;
        if (_playerHasScore)
        {
            dreamloLeaderBoard.Score scoreRef = scores.First(x => x.playerName == SystemInfo.deviceUniqueIdentifier);
            playerRank = scores.IndexOf(scoreRef) + 1;
            playerHasShownScore = playerRank <= _shownEntries;
        }
        
        int rank = 1;
        foreach (dreamloLeaderBoard.Score currentScore in scores)
        {
            // if player isnt on leaderboard, show as last entry instead
            if (rank == _shownEntries && _playerHasScore && !playerHasShownScore)
            {
                Instantiate(_leaderboardEntryPrefab, _entryParent).SetUp(_playerScore.shortText, playerRank, _playerScore.score);
            }
            else if (rank == _shownEntries && !_playerHasScore && _playerHasLocalScore)
                Instantiate(_leaderboardEntryPrefab, _entryParent).SetUp(PlayerPrefs.GetString(Constants.PLAYER_NAME_PLAYER_PREFS_KEY), -1, PlayerPrefs.GetInt(Constants.PLAYER_HIGHSCORE_PLAYER_PREFS_KEY));
            else
                Instantiate(_leaderboardEntryPrefab, _entryParent).SetUp(currentScore.shortText, rank, currentScore.score);
    
            rank++;
            if (rank > _shownEntries)
                break;
        }
    }
}