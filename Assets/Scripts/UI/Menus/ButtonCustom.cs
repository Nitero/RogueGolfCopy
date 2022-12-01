using Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonCustom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Button _button;
    [SerializeField] private float _textMoveDistance;

    private RectTransform _textRect;
    private Vector3 _originalTextPosition;
    private bool _pressed;

    private void Start()
    {
        if (_text == null)
            return;
        _textRect = _text.transform as RectTransform;
        _originalTextPosition = _textRect.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!_button.interactable)
            return;
		
        _pressed = true;

        if (_text != null)
            _textRect.anchoredPosition = _originalTextPosition + Vector3.down * _textMoveDistance;
        AudioManager.Instance.Play(AudioManager.Sound.ClickUIButton);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(_pressed)
        {
            _pressed = false;
            if (_text != null)
                _textRect.anchoredPosition = _originalTextPosition;
        }
    }
}