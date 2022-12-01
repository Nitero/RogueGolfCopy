using System;
using System.Collections.Generic;
using System.Linq;
using MackySoft.Choice;
using UnityEngine;

[CreateAssetMenu(fileName = "BallConfig", menuName = "Configs/BallConfig", order = 1)]
public class BallConfig : ScriptableObject
{
    [SerializeField] private BallType[] BallTypes;
    
    [Serializable]
    public class BallType
    {
        public string Name;
        public string Description;
        public float RewardWeight = 1;
        [Space]
        public Sprite Icon;
        public float SizeModifier = 1;
        public float SpeedModifier = 1;
        public float WeightModifier = 1;
        public float BounceModifier = 1;
        [Space]
        public float BreakWallAfterXHits;
        public float GoThroughXWalls;
        public bool CantFallOffEdge;
        public bool DoesntFallUntilStops;
    }
    
    public BallType[] GetRandomBalls(int amount)
    {
        //TODO: add seeds
        Dictionary<string, BallType> balls = new Dictionary<string, BallType>();
        for (int i = 0; i < amount; i++)
        {
            BallType ball = BallTypes.Where(x => !balls.ContainsKey(x.Name)).ToWeightedSelector(item => item.RewardWeight).SelectItemWithUnityRandom();
            balls.Add(ball.Name, ball);
        }
        return balls.Values.ToArray();
    }
    
    public BallType GetBallByName(string startBall)
    {
        return BallTypes.FirstOrDefault(x => x.Name == startBall);//TODO: instead fill a dictionary
    }
    
    [ContextMenu("TestGetRandomBalls")]
    public void TestGetRandomBalls()
    {
        for (int i = 0; i < 10; i++)
        {
            BallType[] balls = GetRandomBalls(3);
            string result = "Rolled: ";
            for (var j = 0; j < balls.Length; j++)
                result += balls[j].Name + (j < balls.Length-1 ? ", " : "");
            Debug.Log(result);
        }
    }
    [ContextMenu("TestGetRandomBallSums")]
    public void TestGetRandomBallsSums()
    {
        Dictionary<string, int> occurances = new Dictionary<string, int>();
        for (int i = 0; i < 100; i++)
        {
            BallType[] balls = GetRandomBalls(3);
            foreach (BallType ball in balls)
            {
                if (occurances.ContainsKey(ball.Name))
                    occurances[ball.Name]++;
                else
                    occurances.Add(ball.Name, 1);
            }
        }
        string result = "Rolled: ";
        foreach (KeyValuePair<string, int> occurance in occurances.OrderByDescending(x => x.Value))
            result += occurance.Key + " (" + occurance.Value + "), ";
        Debug.Log(result);
    }
}