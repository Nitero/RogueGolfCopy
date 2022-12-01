using System;
using System.Collections.Generic;
using System.Linq;
using MackySoft.Choice;
using UnityEngine;

[CreateAssetMenu(fileName = "ClubConfig", menuName = "Configs/ClubConfig", order = 1)]
public class ClubConfig : ScriptableObject
{
    [SerializeField] private ClubType[] ClubTypes;
    
    [Serializable]
    public class ClubType
    {
        public string Name;
        public string Description;
        public float RewardWeight;
        public Sprite Icon;
        public int MaxAmountOfType;
        [Space]
        public int ShowXTrajectoryBounces;
        public float AddedTrajectoryDistance;
        public float AddedShootStrength;
        public int ExtraLives;
        public int ExtraRewardOptions;
        public bool CanShootBeforeStop;
        public int CanShootEveryXBallTwice;

        public void AddAllRewards(ClubType clubType)
        {
            ShowXTrajectoryBounces += clubType.ShowXTrajectoryBounces;
            AddedTrajectoryDistance += clubType.AddedTrajectoryDistance;
            AddedShootStrength += clubType.AddedShootStrength;
            ExtraLives += clubType.ExtraLives;
            ExtraRewardOptions += clubType.ExtraRewardOptions;
            if(clubType.CanShootBeforeStop)
                CanShootBeforeStop = true;
            CanShootEveryXBallTwice += clubType.CanShootEveryXBallTwice;
        }
        public void RemoveAllRewards(ClubType clubType)
        {
            ShowXTrajectoryBounces -= clubType.ShowXTrajectoryBounces;
            AddedTrajectoryDistance -= clubType.AddedTrajectoryDistance;
            AddedShootStrength -= clubType.AddedShootStrength;
            ExtraLives -= clubType.ExtraLives;
            ExtraRewardOptions -= clubType.ExtraRewardOptions;
            if(clubType.CanShootBeforeStop)
                CanShootBeforeStop = false;
            CanShootEveryXBallTwice -= clubType.CanShootEveryXBallTwice;
        }
    }
    
    public ClubType[] GetRandomClubs(int amount, RoundController rounds)
    {
        //TODO: add seeds
        List<ClubType> possibleClubTypes = new List<ClubType>();
        foreach (ClubType clubType in ClubTypes)
        {
            if(clubType.MaxAmountOfType == 0 || rounds == null || !rounds.StackedClubs.ContainsKey(clubType.Name) || rounds.StackedClubs[clubType.Name] < clubType.MaxAmountOfType)
                possibleClubTypes.Add(clubType);
        }
        
        Dictionary<string, ClubType> clubs = new Dictionary<string, ClubType>();
        for (int i = 0; i < amount; i++)
        {
            ClubType club = possibleClubTypes.Where(x => !clubs.ContainsKey(x.Name)).ToWeightedSelector(item => item.RewardWeight).SelectItemWithUnityRandom();
            clubs.Add(club.Name, club);
        }
        return clubs.Values.ToArray();
    }
    
    [ContextMenu("TestGetRandomClubs")]
    public void TestGetRandomClubs()
    {
        for (int i = 0; i < 100; i++)
        {
            ClubType[] clubs = GetRandomClubs(3, null);
            string result = "Rolled: ";
            for (var j = 0; j < clubs.Length; j++)
                result += clubs[j].Name + (j < clubs.Length-1 ? ", " : "");
            Debug.Log(result);
        }
    }
    [ContextMenu("TestGetRandomClubSums")]
    public void TestGetRandomClubsSums()
    {
        Dictionary<string, int> occurances = new Dictionary<string, int>();
        for (int i = 0; i < 100; i++)
        {
            ClubType[] clubs = GetRandomClubs(3, null);
            foreach (ClubType club in clubs)
            {
                if (occurances.ContainsKey(club.Name))
                    occurances[club.Name]++;
                else
                    occurances.Add(club.Name, 1);
            }
        }
        string result = "Rolled: ";
        foreach (KeyValuePair<string, int> occurance in occurances.OrderByDescending(x => x.Value))
            result += occurance.Key + " (" + occurance.Value + "), ";
        Debug.Log(result);
    }
}