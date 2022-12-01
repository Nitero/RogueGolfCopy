using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = System.Object;

public class RoundController : MonoBehaviour
{
    [SerializeField] private BallConfig _ballConfig;
    [SerializeField] private BoardTileConfig _tileConfig;
    [SerializeField] private RoundUI _ui;
    [SerializeField] private GameController _game;
    [SerializeField] private TileManager _tileManager;
    [SerializeField] private bool _canShootBeforeStop;
    [SerializeField] private List<string> _startBallNames;
    [SerializeField] private SelectTileRewardUI _selectTileRewardUI;
    [SerializeField] private SelectBallRewardUI _selectBallRewardUI;
    [SerializeField] private SelectClubRewardUI _selectClubRewardUI;
    [Space]
    [SerializeField] private int _clubRewardEachXRounds = 12;
    [SerializeField] private int _ballRewardEachXRounds = 6;
    
    private List<BallConfig.BallType> _ownedBalls = new List<BallConfig.BallType>();
    private List<ClubConfig.ClubType> _ownedClubs = new List<ClubConfig.ClubType>();
    private ClubConfig.ClubType _combinedClub = new ClubConfig.ClubType();
    public ClubConfig.ClubType CombinedClub => _combinedClub;
    private Dictionary<string, int> _stackedClubs = new Dictionary<string, int>();
    public Dictionary<string, int> StackedClubs => _stackedClubs;
    private List<int> _usedBalls = new List<int>();
    private int _selectedBall;
    private int _round;
    public int Round => _round;
    private List<BoardTileConfig.RewardType> _pendingRewards = new List<BoardTileConfig.RewardType>();
    private int _shotCounter = 1;
    
    private void OnEnable()
    {
        _game.BallShotEvent += OnBallShot;
        _game.BallStoppedEvent += OnBallStopped;
        _game.BallScoredEvent += OnBallScored;
        _game.BallFellOffEvent += OnBallFellOff;
    }
    private void OnDisable()
    {
        _game.BallShotEvent -= OnBallShot;
        _game.BallStoppedEvent -= OnBallStopped;
        _game.BallScoredEvent -= OnBallScored;
        _game.BallFellOffEvent -= OnBallFellOff;
    }
    
    private void Awake()
    {
        foreach (string startBall in _startBallNames)
            _ownedBalls.Add(_ballConfig.GetBallByName(startBall));
        _ui.Initialize();
        _ui.SetRounds(_round);
        _ui.SetUpBalls(_selectedBall, _usedBalls, _ownedBalls, OnSelectedBall);
    }

    private void OnBallShot()
    {
        bool usedBall = true;
        if (_combinedClub.CanShootEveryXBallTwice > 0)
        {
            _shotCounter++;
            if (_shotCounter > _combinedClub.CanShootEveryXBallTwice)
            {
                _shotCounter = 1;
                usedBall = false;
            }
            _ui.SetUpClubs(_ownedClubs, _stackedClubs, _shotCounter);
        }

        if (usedBall)
        {
            _usedBalls.Add(_selectedBall);
            _ui.UsedBall(_selectedBall);
        }
        
        if (!_combinedClub.CanShootBeforeStop)
            _ui.SetBallPickerInteractable(false);
    }

    private void OnBallStopped()
    {
        if(_usedBalls.Contains(_selectedBall))
            TryToSelectNewBall(1);
        _ui.SetBallPickerInteractable(true);
    }
    
    private void OnBallFellOff()
    {
        if(_usedBalls.Contains(_selectedBall))
            TryToSelectNewBall(1);
        _ui.SetBallPickerInteractable(true);
    }

    private void OnBallScored()
    {
        StartNewRound();
        _ui.SetBallPickerInteractable(true);
    }

    public void StartNewRound(bool wonLastRound = true)
    {
        if (wonLastRound)
        {
            _round++;
            _ui.SetRounds(_round);
        }
        
        _usedBalls.Clear();
        _selectedBall = 0;
        _ui.SetUpBalls(_selectedBall, _usedBalls, _ownedBalls, OnSelectedBall);
    }

    private void OnSelectedBall(int selectedBall)
    {
        _selectedBall = selectedBall;
        _ui.SelectBall(_selectedBall);
        _game.SelectBall(_ownedBalls[_selectedBall]);
    }

    private void TryToSelectNewBall(int direction)
    {
        SelectNewBall(direction);
        if (_usedBalls.Contains(_selectedBall)) //couldnt select any new ball bcz all were used
        {
            if (_combinedClub.ExtraLives > 0)
            {
                RemoveClub(_ownedClubs.First(x => x.ExtraLives > 0));
                _game.SpawnBall();
                StartNewRound(false);
            }
            else
                _game.GameOver();
        }
        else
        {
            _ui.SelectBall(_selectedBall);
            _game.SelectBall(_ownedBalls[_selectedBall]);
        }
    }
    private void SelectNewBall(int direction)
    {
        _selectedBall += direction;
        _selectedBall.Wrap(_ownedBalls.Count);
            
        //skip used balls so cant choose them
        for(int i = 0; i < _ownedBalls.Count; i++)
        {
            if (!_usedBalls.Contains(_selectedBall))
                break;
            _selectedBall += direction;
            _selectedBall.Wrap(_ownedBalls.Count);
        }
    }

    private void AddBall(BallConfig.BallType ballType)
    {
        _ownedBalls.Add(ballType);
        _ui.AddBall(ballType, OnSelectedBall);
    }
    private void AddClub(ClubConfig.ClubType clubType)
    {
        _ownedClubs.Add(clubType);
        _combinedClub.AddAllRewards(clubType);
        UpdateStackedClubs();
        _ui.SetUpClubs(_ownedClubs, _stackedClubs, _shotCounter);
    }
    private void RemoveClub(ClubConfig.ClubType clubType)
    {
        _ownedClubs.Remove(clubType);
        _combinedClub.RemoveAllRewards(clubType);
        UpdateStackedClubs();
        _ui.SetUpClubs(_ownedClubs, _stackedClubs, _shotCounter);
    }
    private void UpdateStackedClubs()
    {
        _stackedClubs.Clear();
        foreach (ClubConfig.ClubType clubType in _ownedClubs)
        {
            if (_stackedClubs.ContainsKey(clubType.Name))
                _stackedClubs[clubType.Name]++;
            else
                _stackedClubs.Add(clubType.Name, 1);
        }
    }

    public BallConfig.BallType GetSelectedBallType()
    {
        return _ownedBalls[_selectedBall];
    }

    public bool HasBallsLeft()
    {
        return _usedBalls.Count < _ownedBalls.Count;
    }

    public void AddReward(string rewardTag)
    {
        _pendingRewards.Add(_tileConfig.Rewards.First(x => x.Tag == rewardTag).RewardType);
    }
    public void OpenRewardOptions()
    {
        if (_round % _clubRewardEachXRounds == 0)
            _pendingRewards.Add(BoardTileConfig.RewardType.Club);
        if (_round % _ballRewardEachXRounds == 0)
            _pendingRewards.Add(BoardTileConfig.RewardType.Ball);
        _pendingRewards.Add(BoardTileConfig.RewardType.Tile);

        TryToOpenNextRewardOption();
    }
    private void TryToOpenNextRewardOption()
    {
        if (_pendingRewards.Count == 0)
            return;

        if(_pendingRewards[0] == BoardTileConfig.RewardType.Mystery)
            OpenRewardUI(_tileConfig.GetRandomMysteryRewardType());
        else
            OpenRewardUI(_pendingRewards[0]);
            
        _pendingRewards.RemoveAt(0);
    }

    private void OpenRewardUI(BoardTileConfig.RewardType rewardType)
    {
        if(rewardType == BoardTileConfig.RewardType.Tile)
            _selectTileRewardUI.Open();
        if(rewardType == BoardTileConfig.RewardType.Ball)
            _selectBallRewardUI.Open();
        if(rewardType == BoardTileConfig.RewardType.Club)
            _selectClubRewardUI.Open();
    }

    public void OnSelectedOption(Object selectedReward)
    {
        if(selectedReward.GetType() == typeof(BallConfig.BallType))
            AddBall((BallConfig.BallType)selectedReward);
        if(selectedReward.GetType() == typeof(ClubConfig.ClubType))
            AddClub((ClubConfig.ClubType)selectedReward);
        if(selectedReward.GetType() == typeof(BoardTileConfig.BoardTileData))
            _tileManager.StartPlacing((BoardTileConfig.BoardTileData) selectedReward);
        TryToOpenNextRewardOption();
    }

    public void OnSkipped()
    {
        TryToOpenNextRewardOption();
    }
}