using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class BoardTileRewardOption : MonoBehaviour
{
    [SerializeField] private GameObject[] _walls;
    [SerializeField] private GameObject _wallTunnelHor;
    [SerializeField] private GameObject _wallTunnelVert;
    [SerializeField] private GameObject _ground;
    [SerializeField] private GameObject _groundGap;
    [SerializeField] private Image _groundImage;
    [SerializeField] private Sprite _groundSpriteSand;
    [SerializeField] private Sprite[] _groundSpritesBoost;
    [SerializeField] private GameObject _obstacle;
    [SerializeField] private Button _selectButton;
    
    public void Initialize(BoardTileConfig.BoardTileData boardTile, Action<Object> onSelect)
    {
        for (int i = 0; i < _walls.Length; i++)
            _walls[i].SetActive(boardTile.WallDirections.Contains((BoardTileConfig.Direction) i));
        bool hasGap = boardTile.Modifiers.Any(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Gap);
        _groundGap.SetActive(hasGap);
        _ground.SetActive(!hasGap);
        _obstacle.SetActive(boardTile.Modifiers.Any(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Obstacle));
        
        if(boardTile.Modifiers.Any(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Sand))
            _groundImage.sprite = _groundSpriteSand;
        if(boardTile.Modifiers.Any(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Boost))
            _groundImage.sprite = _groundSpritesBoost[(int)boardTile.Modifiers.First(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Boost).ModifierDirections[0]];

        if (boardTile.Modifiers.Any(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Tunnel))
        {
            if (boardTile.WallDirections.Contains(BoardTileConfig.Direction.North))
                _wallTunnelVert.SetActive(true);
            else
                _wallTunnelHor.SetActive(true);
        }
        
        _selectButton.onClick.AddListener(() => onSelect(boardTile));
    }
}