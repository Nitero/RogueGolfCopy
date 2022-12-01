using System;
using UnityEngine;
using UnityEngine.UI;

public class BallOwnedView : MonoBehaviour
{
    [SerializeField] private Image _typeIcon;
    [SerializeField] private Image _lockedIcon;
    [SerializeField] private Image _selectedIcon;
    [SerializeField] private Button _selectButton;

    private bool _interactable;
    private int _ballIndex;
    private Action<int> _selectedBallCallback;
    
    public void SetUp(int ballIndex, BallConfig.BallType ballType, bool interactable, bool isSelected, Action<int> selectedBallCallback)
    {
        _ballIndex = ballIndex;
        _typeIcon.sprite = ballType.Icon;
        SetInteractablePermanent(interactable);
        SetSelected(isSelected);
        _selectedBallCallback = selectedBallCallback;
    }

    private void OnEnable()
    {
        _selectButton.onClick.AddListener(OnSelectButtonClicked);
    }

    private void OnDisable()
    {
        _selectButton.onClick.RemoveListener(OnSelectButtonClicked);
    }

    private void OnSelectButtonClicked()
    {
        _selectedBallCallback.Invoke(_ballIndex);
    }

    public void SetSelected(bool isSelected)
    {
        _selectedIcon.gameObject.SetActive(isSelected);
    }

    public void SetInteractablePermanent(bool interactable)
    {
        _interactable = interactable;
        _selectButton.interactable = interactable;
        
        _lockedIcon.gameObject.SetActive(!interactable);
    }
    public void SetInteractableTemporary(bool interactable)
    {
        if(interactable)
            _selectButton.interactable = _interactable;
        else
            _selectButton.interactable = false;
    }
}