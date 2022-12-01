using System;
using System.Collections.Generic;
using System.Linq;
using MackySoft.Choice;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "BoardTileConfig", menuName = "Configs/BoardTileConfig", order = 1)]
public class BoardTileConfig : ScriptableObject
{
    public int SpawnProtectionRadius = 32;
    public int GenerationRadius = 32;
    [SerializeField] private GenerationData _defaultGenerationData;
    public GenerationData DefaultGenerationData => _defaultGenerationData;
    [SerializeField] private GenerationData _rewardGenerationData;
    public GenerationData RewardGenerationData => _rewardGenerationData;
    
    public int Seed;
    public int MaxSeed;
    public bool RandomSeed;
    
    [Serializable]
    public class GenerationData
    {
        public float NoiseThreshold = 0.75f;
        public float NoiseScaleX = 5f;
        public float NoiseScaleY = 5f;
    }
    
    [SerializeField] private WeightedWallSpawnChances[] GeneratedWallSpawnChances;
    [SerializeField] private WeightedWallSpawnChances[] RewardWallSpawnChances;
    [SerializeField] private WeightedModifierChances[] ModifierChances;
    
    [Serializable]
    public class WeightedWallSpawnChances
    {
        public int WallCount;
        public float Weight;
    }
    [Serializable]
    public class WeightedModifierChances
    {
        public BoardTileData.Modifier.ModifierType ModifierType;
        public float Weight;
    }
    
    public enum Direction {North, East, South, West};
    private Vector2Int[] _directionVectors = new[] {Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left};
    public int GridDimensions = 4;

    public Vector2Int GetGridOffset(int directionIndex)
    {
        return _directionVectors[directionIndex];
    }
    
    
    [SerializeField] private RewardData[] _rewards;
    public RewardData[] Rewards => _rewards;
    [SerializeField] private RewardData[] _mysteryRewards;

    [Serializable]
    public class RewardData
    {
        public RewardType RewardType;
        public float Weight;
        public Sprite Sprite;
        public string Tag;
    }

    public enum RewardType {Tile, Ball, Club, Mystery};

    public RewardType GetRandomMysteryRewardType()
    {
        var rewardSelector = _mysteryRewards.ToWeightedSelector(item => item.Weight);
        return rewardSelector.SelectItemWithUnityRandom().RewardType;
    }
    
    
    public class BoardTileData
    {
        public Direction[] WallDirections;
        public Modifier[] Modifiers;
        public bool HasStart;
        public bool HasGoal;
        public bool HasReward;
        public RewardType RewardType;

        public class Modifier
        {
            public Direction[] ModifierDirections;
            public ModifierType Type;
            public enum ModifierType {None, Gap, Obstacle, Boost, Sand, Tunnel, Spinner, Thin };

            public Modifier(ModifierType modifierType)
            {
                Type = modifierType;
                if (modifierType == ModifierType.Boost)
                    ModifierDirections = new Direction[] {(Direction) Random.Range(0, 4)};
                // if(modifierType == ModifierType.Thin)
                //TODO: add rand directions like for walls
            }
        }

        public BoardTileData(List<int> walls, Modifier[] modifiers, bool hasReward)
        {
            WallDirections = IntsToDirections(walls);
            Modifiers = modifiers;
        }
        public BoardTileData(List<int> walls, Modifier[] modifiers, bool hasGoal, bool hasStart)
        {
            WallDirections = IntsToDirections(walls);
            Modifiers = modifiers;
            HasGoal = hasGoal;
            HasStart = hasStart;
        }
        public BoardTileData(IWeightedSelector<WeightedWallSpawnChances> generatedWallSpawnChancesWeighted,
            IWeightedSelector<WeightedModifierChances> modifierChancesWeighted,
            IWeightedSelector<WeightedModifierChances> modifierNonTunnelChancesWeighted,
            IWeightedSelector<RewardData> rewardChancesWeighted,
            bool hasReward)
        {
            int wallCount = generatedWallSpawnChancesWeighted.SelectItemWithUnityRandom().WallCount;

            List<int> walls = new List<int>(wallCount);
            for (int i = 0; i < wallCount; i++)
                walls.Add(RandomDirectionIntExcept(walls));
            
            WallDirections = IntsToDirections(walls);
            
            Modifier.ModifierType modifierType;
            if (HasOnlyOppositeWalls(walls))
                modifierType = modifierChancesWeighted.SelectItemWithUnityRandom().ModifierType;
            else
                modifierType = modifierNonTunnelChancesWeighted.SelectItemWithUnityRandom().ModifierType;
            
            if(modifierType != Modifier.ModifierType.None)
                Modifiers = new[] { new Modifier(modifierType) };
            else
                Modifiers = Array.Empty<Modifier>();
            
            HasReward = hasReward;
            if (hasReward)
                RewardType = rewardChancesWeighted.SelectItemWithUnityRandom().RewardType;
        }

        private int RandomDirectionIntExcept(List<int> exclude)
        {
            IEnumerable<int> range = Enumerable.Range(0, 4).Where(i => !exclude.Contains(i));
            return range.ElementAt(Random.Range(0, 4 - exclude.Count));
        }

        private Direction[] IntsToDirections(List<int> ints)
        {
            Direction[] directions = new Direction[ints.Count];
            for (int i = 0; i < ints.Count; i++)
                directions[i] = (Direction)ints[i];
            return directions;
        }

        private bool HasOnlyOppositeWalls(List<int> walls)
        {
            if (walls.Count != 2)
                return false;
            if (walls.Contains(0) && walls.Contains(2))
                return true;
            if (walls.Contains(1) && walls.Contains(3))
                return true;
            return false;
        }
    }
    
    public BoardTileData[] GetRandomTiles(int amount)
    {
        //TODO: add seeds
        BoardTileData[] tiles = new BoardTileData[amount];
        for (int i = 0; i < amount; i++)
            tiles[i] = GetRandomTile(false, false);
        return tiles;
    }
    public BoardTileData GetRandomTile(bool generated, bool hasReward)
    {
        return new BoardTileData(generated ? _generatedWallSpawnChancesWeighted : _rewardWallSpawnChancesWeighted, _modifierChancesWeighted, _modifierNonTunnelChancesWeighted, _rewardChancesWeighted, hasReward);
    }
    
    private IWeightedSelector<WeightedWallSpawnChances> _generatedWallSpawnChancesWeighted;
    private IWeightedSelector<WeightedWallSpawnChances> _rewardWallSpawnChancesWeighted;
    private IWeightedSelector<WeightedModifierChances> _modifierChancesWeighted;
    private IWeightedSelector<WeightedModifierChances> _modifierNonTunnelChancesWeighted;
    private IWeightedSelector<RewardData> _rewardChancesWeighted;

    private void OnEnable()
    {
        PrepareWeightedRandoms();
    }
    private void OnValidate()
    {
        PrepareWeightedRandoms();
    }
    private void PrepareWeightedRandoms()
    {
        _generatedWallSpawnChancesWeighted = GeneratedWallSpawnChances.ToWeightedSelector(item => item.Weight);
        _rewardWallSpawnChancesWeighted = RewardWallSpawnChances.ToWeightedSelector(item => item.Weight);
        _modifierChancesWeighted = ModifierChances.ToWeightedSelector(item => item.Weight);
        _modifierNonTunnelChancesWeighted = ModifierChances.Where(item => item.ModifierType != BoardTileData.Modifier.ModifierType.Tunnel).ToWeightedSelector(item => item.Weight);
        _rewardChancesWeighted = Rewards.ToWeightedSelector(item => item.Weight);
    }
}