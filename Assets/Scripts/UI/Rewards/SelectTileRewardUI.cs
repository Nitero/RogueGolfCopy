using UnityEngine;

public class SelectTileRewardUI : SelectRewardUI
{
    [SerializeField] private BoardTileConfig _tileConfig;
    [SerializeField] private BoardTileRewardOption _boardTileRewardOptionPrefab;

    
    protected override void InstantiateOptions()
    {
        foreach (BoardTileConfig.BoardTileData tileConfig in _tileConfig.GetRandomTiles(3+_rounds.CombinedClub.ExtraRewardOptions))
            Instantiate(_boardTileRewardOptionPrefab, _optionsViewParent).Initialize(tileConfig, OnSelectOption);
        
        _skipButton.gameObject.SetActive(false);
    }
}