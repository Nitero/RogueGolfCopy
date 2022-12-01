using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class BallRewardOption : MonoBehaviour
{
    [SerializeField] private Image _ballIcon;
    [SerializeField] private TextMeshProUGUI _ballName;
    [SerializeField] private TextMeshProUGUI _ballDescription;
    [SerializeField] private Button _selectButton;

    public void Initialize(BallConfig.BallType ballType, Action<Object> onSelect)
    {
        _ballIcon.sprite = ballType.Icon;
        _ballName.text = ballType.Name;
        _ballDescription.text = ballType.Description;
        _selectButton.onClick.AddListener(() => onSelect(ballType));
    }
}