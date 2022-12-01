using TMPro;
using UnityEngine;

public class LeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rank;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _score;

    public void SetUp(string name, int rank, int score)
    {
        _name.text = name;
        _rank.text = $"{rank}.";
        if (score >= 0)
            _score.text = score.ToString();
        else
            _score.text = "?";
    }
}