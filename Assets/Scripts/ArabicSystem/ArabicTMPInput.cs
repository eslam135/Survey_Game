using UnityEngine;
using TMPro;
using ArabicSupport;
using UnityEngine.InputSystem;

public class ArabicTMPInput : MonoBehaviour
{
    [SerializeField] TMP_Text _placeholderText;
    [SerializeField] TMP_Text _displayText;

    private TMP_InputField _inputField;
    private string _rawText = "";
    private bool _isUpdating = false;
    private float _backspaceCooldown = 0f;

    void Start()
    {
        _inputField = GetComponent<TMP_InputField>();
        _inputField.textComponent.color = new Color(1, 1, 1, 0.01f);

        if (_placeholderText != null)
            _placeholderText.text = ArabicFixer.Fix(_placeholderText.text);

        _inputField.onValueChanged.AddListener(OnTextChanged);
        _inputField.onSelect.AddListener(OnInputSelected);
        _inputField.onDeselect.AddListener(OnInputDeselected);
    }

    void Update()
    {
        if (!_inputField.isFocused || Keyboard.current == null) return;

        _backspaceCooldown -= Time.unscaledDeltaTime;

        // === BACKSPACE ===
        if (Keyboard.current.backspaceKey.isPressed && _rawText.Length > 0 && _backspaceCooldown <= 0f)
        {
            _rawText = _rawText.Remove(_rawText.Length - 1, 1);
            _inputField.SetTextWithoutNotify(_rawText);
            _inputField.stringPosition = _rawText.Length;
            _inputField.caretPosition = _rawText.Length;
            UpdateDisplayText();

            _backspaceCooldown = 0.08f; // adjust for hold sensitivity
        }

        // === CTRL+A ===
        if ((Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed) &&
            Keyboard.current.aKey.wasPressedThisFrame)
        {
            _inputField.selectionStringAnchorPosition = 0;
            _inputField.selectionStringFocusPosition = _rawText.Length;
        }
    }

    void OnTextChanged(string newText)
    {
        if (_isUpdating) return;
        _isUpdating = true;
        _rawText = newText;
        UpdateDisplayText();
        _isUpdating = false;
    }

    void UpdateDisplayText()
    {
        _displayText.text = ArabicFixer.Fix(_rawText);
    }

    void OnInputSelected(string text)
    {
        if (_placeholderText != null)
            _placeholderText.gameObject.SetActive(false);
    }

    void OnInputDeselected(string text)
    {
        if (_placeholderText != null)
            _placeholderText.gameObject.SetActive(string.IsNullOrEmpty(_rawText));
    }

    void OnDestroy()
    {
        _inputField.onValueChanged.RemoveListener(OnTextChanged);
        _inputField.onSelect.RemoveListener(OnInputSelected);
        _inputField.onDeselect.RemoveListener(OnInputDeselected);
    }
}
