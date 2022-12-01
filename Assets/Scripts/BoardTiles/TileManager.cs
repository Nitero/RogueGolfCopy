using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] private GameController _game;
    [SerializeField] private CameraController _camera;
    [SerializeField] private BoardTileConfig _tileConfig;
    [SerializeField] private BoardTile _startTile;
    [SerializeField] private BoardTile _goalTile;
    [SerializeField] private Transform _tileParent;
    [SerializeField] private BoardTile _boardTilePrefab;

    public BoardTile StartTile => _startTile;
    private bool _inPlacement;

    private List<BoardTile> _tiles = new List<BoardTile>();
    private List<Vector2Int> _checkedGridPositions = new List<Vector2Int>();

    private void Start()
    {
        if (_tileConfig.RandomSeed)
            _tileConfig.Seed = (int) System.DateTime.Now.Ticks;
        AddStartTiles();
    }

    private void AddStartTiles()
    {
        BoardTile startTile = AddTile(0, 0,
            new BoardTileConfig.BoardTileData(new List<int>() {1, 2, 3},
                new BoardTileConfig.BoardTileData.Modifier[] { }, false, true));
        
        _goalTile = AddTile(0, 1,
            new BoardTileConfig.BoardTileData(new List<int>(),
                new BoardTileConfig.BoardTileData.Modifier[] { }, true, false));
        
        _camera.AddBoardTile(startTile.transform);
        _camera.AddBoardTile(_goalTile.transform);
        
        GenerateTilesAround(_goalTile);
    }

    private BoardTile AddTile(int x, int y, BoardTileConfig.BoardTileData tileData, bool randomReward = false)
    {
        BoardTile tile = Instantiate(_boardTilePrefab, _tileParent).Initialize(tileData, this);
        tile.transform.position = new Vector2(x, y) * _tileConfig.GridDimensions;
        _tiles.Add(tile);
        tile.Place(ToGridPosition(tile.transform.position));
        return tile;
    }

    public void StartPlacing(BoardTileConfig.BoardTileData boardTile)
    {
        boardTile.HasGoal = true;
        _goalTile = Instantiate(_boardTilePrefab, _tileParent).Initialize(boardTile, this);
        _inPlacement = true;
        _goalTile.SetAlpha(0.5f);
    }
    
    private void Update()
    {
        if (_inPlacement)
        {
            Vector2 desiredPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            desiredPosition = new Vector2(Mathf.Round(desiredPosition.x / _tileConfig.GridDimensions) * _tileConfig.GridDimensions,
                                        Mathf.Round(desiredPosition.y / _tileConfig.GridDimensions) * _tileConfig.GridDimensions);
            _goalTile.transform.position = desiredPosition;
            
            bool canPlace = CanPlaceTile(ToGridPosition(desiredPosition));
            //TODO: add feedback if cant place like making it red or shaking when try to place anyways
            
            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                _inPlacement = false;
                _tiles.Add(_goalTile);
                _goalTile.Place(ToGridPosition(_goalTile.transform.position));
                _camera.AddBoardTile(_goalTile.transform);
                GenerateTilesAround(_goalTile);
                _game.PlacedGoal();
            }
        }
    }

    public void DeleteGoalAndTryAddModifier()
    {
        _goalTile.DeleteGoalAndTryAddModifier();
    }
    
    private void GenerateTilesAround(BoardTile boardTile)
    {
        //TODO: instead circle shape? or 16:9?
        for (int x = -_tileConfig.GenerationRadius; x < _tileConfig.GenerationRadius+1; x++)
        {
            for (int y = -_tileConfig.GenerationRadius; y < _tileConfig.GenerationRadius+1; y++)
            {
                Vector2Int gridPosition = boardTile.GridPosition + new Vector2Int(x, y);
                if (_checkedGridPositions.Contains(gridPosition))
                    continue;
                _checkedGridPositions.Add(gridPosition);

                if(gridPosition.x < _tileConfig.SpawnProtectionRadius && gridPosition.x > -_tileConfig.SpawnProtectionRadius
                && gridPosition.y < _tileConfig.SpawnProtectionRadius+1 && gridPosition.y > -_tileConfig.SpawnProtectionRadius+1)
                    continue;

                int sampleX = gridPosition.x + (_tileConfig.Seed % _tileConfig.MaxSeed);
                int sampleY = gridPosition.y + (_tileConfig.Seed % _tileConfig.MaxSeed);
                if (Mathf.PerlinNoise(sampleX / _tileConfig.RewardGenerationData.NoiseScaleX,
                                    sampleY / _tileConfig.RewardGenerationData.NoiseScaleY) <= _tileConfig.RewardGenerationData.NoiseThreshold
                    && CanPlaceTile(gridPosition))
                    AddTile(gridPosition.x, gridPosition.y, _tileConfig.GetRandomTile(true, true));
                
                if (Mathf.PerlinNoise(sampleX / _tileConfig.DefaultGenerationData.NoiseScaleX, 
                                    sampleY / _tileConfig.DefaultGenerationData.NoiseScaleY) <= _tileConfig.DefaultGenerationData.NoiseThreshold
                    && CanPlaceTile(gridPosition))
                    AddTile(gridPosition.x, gridPosition.y, _tileConfig.GetRandomTile(true, false));
            }
        }
    }

    private Vector2Int ToGridPosition(Vector2 value)
    {
        return value.ToVec2IntPosition() / _tileConfig.GridDimensions;
    }
    private Vector2Int ToGridPosition(Vector3 value)
    {
        return value.ToVec2IntPosition() / _tileConfig.GridDimensions;
    }
    
    public bool CanPlaceTile(Vector2Int gridPosition)
    {
        return _tiles.All(x => x.GridPosition != gridPosition);
    }
    public bool CanPlaceTile(Vector2 transformPosition)
    {
        return _tiles.All(x => x.GridPosition != ToGridPosition(transformPosition));
    }
    public bool CanPlaceTile(Vector2 transformPosition, int directionIndex)
    {
        return _tiles.All(x => x.GridPosition != ToGridPosition(transformPosition)+_tileConfig.GetGridOffset(directionIndex));
    }
    public BoardTile GetTile(Vector3 transformPosition, int directionIndex)
    {
        return _tiles.First(x => x.GridPosition == ToGridPosition(transformPosition)+_tileConfig.GetGridOffset(directionIndex));
    }
}