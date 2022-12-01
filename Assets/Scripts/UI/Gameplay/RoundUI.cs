using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _roundCounter;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Transform _ballUIParent;
    [SerializeField] private BallOwnedView _ballOwnedViewPrefab;
    [SerializeField] private Transform _clubUIParent;
    [SerializeField] private ClubView _clubViewPrefab;

    private List<BallOwnedView> _ballViews = new List<BallOwnedView>();
    private List<ClubView> _clubViews = new List<ClubView>();

    public void Initialize()
    {
        foreach (Transform child in _clubUIParent)
            Destroy(child.gameObject);
    }
    
    public void SetRounds(int round)
    {
        _roundCounter.text = $"Hole: {round}";
    }
    
    public void SetUpClubs(List<ClubConfig.ClubType> ownedClubs, Dictionary<string, int> stackedClubs, int shotCounter)
    {
        foreach (Transform child in _clubUIParent)
            Destroy(child.gameObject);
        _clubViews.Clear();
        
        foreach(KeyValuePair<string, int> stackedClub in stackedClubs)
        {
            ClubView clubView = Instantiate(_clubViewPrefab, _clubUIParent);
            clubView.SetUp(ownedClubs.First(x => x.Name == stackedClub.Key), stackedClub.Value, shotCounter);
            _clubViews.Add(clubView);
        }
    }
    
    public void SetUpBalls(int selectedBall, List<int> usedBalls, List<BallConfig.BallType> ownedBalls, Action<int> selectedBallCallback)
    {
        foreach (Transform child in _ballUIParent)
            Destroy(child.gameObject);
        _ballViews.Clear();
        for (int i = 0; i < ownedBalls.Count; i++)
        {
            BallOwnedView ballOwnedView = Instantiate(_ballOwnedViewPrefab, _ballUIParent);
            ballOwnedView.SetUp(i, ownedBalls[i], !usedBalls.Contains(i), i == selectedBall, selectedBallCallback);
            _ballViews.Add(ballOwnedView);
        }
    }

    public void AddBall(BallConfig.BallType ballType, Action<int> selectedBallCallback)
    {
        BallOwnedView ballOwnedView = Instantiate(_ballOwnedViewPrefab, _ballUIParent);
        ballOwnedView.SetUp(_ballViews.Count, ballType, true, false, selectedBallCallback);
        _ballViews.Add(ballOwnedView);
    }

    public void SelectBall(int selectedBall)
    {
        for (int i = 0; i < _ballViews.Count; i++)
            _ballViews[i].SetSelected(i == selectedBall);
    }

    public void UsedBall(int usedBall)
    {
        _ballViews[usedBall].SetInteractablePermanent(false);
    }
    
    private void OnEnable()
    {
        _restartButton.onClick.AddListener(OnRestartClicked);
    }

    private void OnDisable()
    {
        _restartButton.onClick.RemoveListener(OnRestartClicked);
    }

    private void OnRestartClicked()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
    
    public void SetBallPickerInteractable(bool interactable)
    {
        foreach (BallOwnedView ballView in _ballViews)
            ballView.SetInteractableTemporary(interactable);
    }
}