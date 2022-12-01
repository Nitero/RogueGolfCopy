using UnityEngine;

public class SelectBallRewardUI : SelectRewardUI
{
    [SerializeField] private BallConfig _ballConfig;
    [SerializeField] private BallRewardOption _ballRewardOptionPrefab;
    
    protected override void InstantiateOptions()
    {
        foreach (BallConfig.BallType ballConfig in _ballConfig.GetRandomBalls(3+_rounds.CombinedClub.ExtraRewardOptions))
            Instantiate(_ballRewardOptionPrefab, _optionsViewParent).Initialize(ballConfig, OnSelectOption);
    }
}