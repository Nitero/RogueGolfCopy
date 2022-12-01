using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private RoundController _rounds;
    [SerializeField] private TileManager _tileManager;
    [SerializeField] private CameraController _camera;
    [SerializeField] private ShootInput _shootInput;
    [SerializeField] private BallController _ballPrefab;
    [SerializeField] private GameOverUI _gameOverUI;
    
    private BallController _currentBall;
    public BallController CurrentBall => _currentBall;
    
    public Dictionary<Collider2D, int> HitWalls = new Dictionary<Collider2D, int>();
    
    public Action BallShotEvent;
    public Action BallStoppedEvent;
    public Action BallScoredEvent;
    public Action BallFellOffEvent;

    private void Start()
    {
        SpawnBall();
        _rounds.StartNewRound();
    }

    public void ScoredGoal()
    {
        _shootInput.enabled = false;
        Destroy(_currentBall.gameObject);
        _camera.FocusBoard();
        
        _tileManager.DeleteGoalAndTryAddModifier();
        StartCoroutine(ScoredGoalDelayed());
        AudioManager.Instance.Play(AudioManager.Sound.BallScore);
    }
    private IEnumerator ScoredGoalDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        _rounds.OpenRewardOptions();
        BallScoredEvent?.Invoke();
    }

    public void FellOff()
    {
        Destroy(_currentBall.gameObject);
        if (_rounds.HasBallsLeft())
            SpawnBall();
        else
            _camera.FocusBoard();//game over
        
        BallFellOffEvent?.Invoke();
    }

    public void PlacedGoal()
    {
        _shootInput.Initialize();
        _shootInput.enabled = true;
        SpawnBall();
        AudioManager.Instance.Play(AudioManager.Sound.TilePlaced);
    }

    public void SpawnBall()
    {
        _currentBall = Instantiate(_ballPrefab, _tileManager.StartTile.transform.position, Quaternion.identity).Initialize(this, _rounds.GetSelectedBallType());
        _camera.AddBall(_currentBall.transform);
        _camera.FocusBall();
        AudioManager.Instance.Play(AudioManager.Sound.BallRespawn);
    }

    public void ShotBall()
    {
        _currentBall.Shoot();
        if(!_rounds.HasBallsLeft() && !_rounds.CombinedClub.CanShootBeforeStop)
            _shootInput.enabled = false;
        
        BallShotEvent?.Invoke();
        AudioManager.Instance.Play(AudioManager.Sound.BallShoot);
    }
    
    public void StoppedBall()
    {
        BallStoppedEvent.Invoke();
        AudioManager.Instance.Play(AudioManager.Sound.BallReady);
    }
    
    public bool CanShootBall()
    {
        return _currentBall != null
           && _currentBall.CanBeShot
           && _rounds.HasBallsLeft();
    }

    public void SelectBall(BallConfig.BallType ballType)
    {
        _currentBall.SetBallType(ballType);
        AudioManager.Instance.Play(AudioManager.Sound.BallSelect);
    }

    public void AddReward(string rewardTag)
    {
        _rounds.AddReward(rewardTag);
    }

    public void GameOver()
    {
        _gameOverUI.Open();
        AudioManager.Instance.Play(AudioManager.Sound.GameOver);
    }
}