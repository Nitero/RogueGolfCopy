using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetPlayerNameUI : MonoBehaviour
{
    [SerializeField] private RoundUI _roundUI;
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private Button _submitButton;
    // [SerializeField] private TextMeshProUGUI _invalidInputText;
    
    private void OnEnable()
    {
        _submitButton.onClick.AddListener(OnSubmitButtonClicked);
        _nameInput.onValueChanged.AddListener(CheckInput);
        CheckInput("");
    }

    private void OnDisable()
    {
        _submitButton.onClick.RemoveListener(OnSubmitButtonClicked);
        _nameInput.onValueChanged.RemoveListener(CheckInput);
    }
    
    private void CheckInput(string name)
    {
        _submitButton.interactable = name.Length > 0 && name.Length < 13;
        // _invalidInputText.gameObject.SetActive(!_submitButton.interactable);
    }
    
    private void OnSubmitButtonClicked()
    {
        PlayerPrefs.SetString(Constants.PLAYER_NAME_PLAYER_PREFS_KEY, _nameInput.text);
        gameObject.SetActive(false);
        _roundUI.gameObject.SetActive(true);
    }
}