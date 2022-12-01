using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class ClubRewardOption : MonoBehaviour
{
    [SerializeField] private Image _clubIcon;
    [SerializeField] private TextMeshProUGUI _clubName;
    [SerializeField] private TextMeshProUGUI _clubDescription;
    [SerializeField] private Button _selectButton;

    public void Initialize(ClubConfig.ClubType clubType, Action<Object> onSelect)
    {
        _clubIcon.sprite = clubType.Icon;
        _clubName.text = clubType.Name;
        _clubDescription.text = clubType.Description;
        _selectButton.onClick.AddListener(() => onSelect(clubType));
    }
}