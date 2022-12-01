using UnityEngine;

public class SelectClubRewardUI : SelectRewardUI
{
    [SerializeField] private ClubConfig _clubConfig;
    [SerializeField] private ClubRewardOption _clubRewardOptionPrefab;
    
    protected override void InstantiateOptions()
    {
        foreach (ClubConfig.ClubType clubConfig in _clubConfig.GetRandomClubs(3+_rounds.CombinedClub.ExtraRewardOptions, _rounds))
            Instantiate(_clubRewardOptionPrefab, _optionsViewParent).Initialize(clubConfig, OnSelectOption);
    }
}