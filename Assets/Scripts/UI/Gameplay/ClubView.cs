using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClubView : MonoBehaviour
{
    [SerializeField] private Image _typeIcon;
    [SerializeField] private GameObject _counterParent;
    [SerializeField] private TextMeshProUGUI _counterText;
    
    public void SetUp(ClubConfig.ClubType clubType, int amount, int shotCounter)
    {
        _typeIcon.sprite = clubType.Icon;

        _counterParent.SetActive(amount > 1);
        _counterText.text = amount.ToString();

        if (clubType.CanShootEveryXBallTwice > 0)
        {
            _counterParent.SetActive(true);
            _counterText.color = Color.red;
            _counterText.text = shotCounter.ToString();
        }
    }
}