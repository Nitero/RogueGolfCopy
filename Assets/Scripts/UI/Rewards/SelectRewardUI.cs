using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public abstract class SelectRewardUI : MonoBehaviour
{
    [SerializeField] private GameObject _selectionContainer;
    [SerializeField] protected Transform _optionsViewParent;
    [SerializeField] private Button _toggleShowSelectionButton;
    [SerializeField] protected Button _skipButton;
    [SerializeField] private TextMeshProUGUI _toggleShowSelectionButtonText;
    [SerializeField] protected RoundController _rounds;

    private void OnEnable()
    {
        _toggleShowSelectionButton.onClick.AddListener(OnToggleShowSelection);
        _skipButton.onClick.AddListener(OnSkipped);
    }

    private void OnDisable()
    {
        _toggleShowSelectionButton.onClick.RemoveListener(OnToggleShowSelection);
        _skipButton.onClick.RemoveListener(OnSkipped);
    }

    private void OnToggleShowSelection()
    {
        _selectionContainer.SetActive(!_selectionContainer.activeSelf);
        _toggleShowSelectionButtonText.text = _selectionContainer.activeSelf ? "Hide" : "Show";
    }
    
    public void Open()
    {
        gameObject.SetActive(true);

        foreach (Transform child in _optionsViewParent)
            Destroy(child.gameObject);

        InstantiateOptions();
    }

    protected abstract void InstantiateOptions();


    protected void OnSelectOption(Object selectedReward)
    {
        gameObject.SetActive(false);
        _rounds.OnSelectedOption(selectedReward);
    }

    private void OnSkipped()
    {
        gameObject.SetActive(false);
        _rounds.OnSkipped();
    }
}