using System.Linq;
using UnityEngine;

public class BoardTile : MonoBehaviour
{
    [SerializeField] private BoardTileConfig _tileConfig;
    [SerializeField] private GameObject[] _walls;
    [SerializeField] private GameObject _wallTunnelHor;
    [SerializeField] private GameObject _wallTunnelVert;
    [SerializeField] private GameObject[] _wallsInvis;
    [SerializeField] private GameObject _wallsInvisCenter;
    [SerializeField] private GameObject _ground;
    [SerializeField] private GameObject _groundGap;
    [SerializeField] private SpriteRenderer _groundRenderer;
    [SerializeField] private Sprite _groundSpriteSand;
    [SerializeField] private Sprite[] _groundSpritesBoost;
    [SerializeField] private GameObject _obstacle;
    [SerializeField] private GameObject _goal;
    [SerializeField] private GameObject _start;
    [SerializeField] private SpriteRenderer _rewardPickup;
    [SerializeField] private SpriteRenderer[] _sprites;
    [SerializeField] private SpriteRenderer[] _spritesDisableDuringPlacement;

    private TileManager _tileManager;
    
    private BoardTileConfig.BoardTileData _boardTile;
    public Vector2Int GridPosition { get; set; }

    public void DeleteGoalAndTryAddModifier()
    {
        _goal.gameObject.SetActive(false);

        if (_boardTile == null)//one of the tiles already existing in the scene
            return;
        
        if (_boardTile.Modifiers.Any(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Obstacle))
            _obstacle.SetActive(true);
        if (_boardTile.Modifiers.Any(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Tunnel))
        {
            if (_boardTile.WallDirections.Contains(BoardTileConfig.Direction.North))
                _wallTunnelVert.SetActive(true);
            else
                _wallTunnelHor.SetActive(true);
        }
        if (_boardTile.Modifiers.Any(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Gap))
        {
            _ground.SetActive(false);
            _groundGap.SetActive(true);
            _wallsInvisCenter.SetActive(true);
        }

        if (_boardTile.Modifiers.Any(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Sand))
        {
            _groundRenderer.sprite = _groundSpriteSand;
            _ground.tag = "Sand";
        }

        if (_boardTile.Modifiers.Any(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Boost))
        {
            int direction = (int) _boardTile.Modifiers.First(x => x.Type == BoardTileConfig.BoardTileData.Modifier.ModifierType.Boost).ModifierDirections[0];
            _groundRenderer.sprite = _groundSpritesBoost[direction];
            _ground.tag = "Boost"+direction;
        }

    }
    
    public BoardTile Initialize(BoardTileConfig.BoardTileData boardTile, TileManager tileManager)
    {
        _boardTile = boardTile;
        _tileManager = tileManager;
        
        for (int i = 0; i < _walls.Length; i++)
            _walls[i].SetActive(boardTile.WallDirections.Contains((BoardTileConfig.Direction) i));
        _ground.SetActive(true);
        _goal.gameObject.SetActive(boardTile.HasGoal);
        _start.gameObject.SetActive(boardTile.HasStart);
        _rewardPickup.gameObject.SetActive(boardTile.HasReward);
        if (boardTile.HasReward)
        {
            BoardTileConfig.RewardData reward = _tileConfig.Rewards.First(x => x.RewardType == boardTile.RewardType);
            _rewardPickup.sprite = reward.Sprite;
            _rewardPickup.tag = reward.Tag;
        }
        
        return this;
    }

    public void Place(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
        SetAlpha(1f);
        UpdateInvisibleWalls(true);
    }

    private void UpdateInvisibleWalls(bool firstCall)
    {
        for (int i = 0; i < _walls.Length; i++)
        {
            bool neighborExists = !_tileManager.CanPlaceTile(transform.position, i);
            if (neighborExists && firstCall)
                _tileManager.GetTile(transform.position, i).UpdateInvisibleWalls(false);
            if (_walls[i] == null || !_walls[i].activeSelf)
                _wallsInvis[i].SetActive(!neighborExists);
            else
                _wallsInvis[i].SetActive(false);
        }
    }

    public void SetAlpha(float alpha)
    {
        foreach (SpriteRenderer sprite in _sprites)
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);
        foreach (SpriteRenderer sprite in _spritesDisableDuringPlacement)
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha < 1f ? 0f : alpha);
    }
}