using UnityEngine;
using TMPro;
using ArabicSupport;
using UnityEngine.InputSystem;
using UnityEngine.UI; // added for masking components

[RequireComponent(typeof(TMP_InputField))]
public class ArabicManagedInput : MonoBehaviour
{
    [SerializeField] TMP_Text _placeholderText;
    [SerializeField] TMP_Text _displayText;

    private TMP_InputField _inputField;
    private string _rawText = "";
    private bool _isUpdating = false;
    // backspace repeat control
    [Header("Backspace Repeat Settings")] [SerializeField] private float backspaceFirstDelay = 0.35f; [SerializeField] private float backspaceRepeatDelay = 0.06f; private float _backspaceTimer = 0f;

    [SerializeField] private string _arabicPlaceholder = "اكتب هنا..";
    [SerializeField] private string _englishPlaceholder = "Type here..";

    private string _originalPlaceholder = ""; // legacy storage
    private float _cachedViewWidth = -1f;

    void Start()
    {
        _inputField = GetComponent<TMP_InputField>();

        if (_placeholderText != null)
        {
            _originalPlaceholder = _arabicPlaceholder;
            _placeholderText.text = ArabicFixer.Fix(_arabicPlaceholder);
        }

        if (_inputField.textComponent != null)
        {
            _inputField.textComponent.color = new Color(1, 1, 1, 0.01f);
            _inputField.textComponent.textWrappingMode = TextWrappingModes.NoWrap;
        }

        _inputField.lineType = TMP_InputField.LineType.SingleLine;
        _inputField.textViewport.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _inputField.textComponent.fontSize * 1.4f);

        if (_displayText != null)
        {
            _displayText.textWrappingMode = TextWrappingModes.NoWrap;
            _displayText.overflowMode = TextOverflowModes.Masking;
        }

        _inputField.onValueChanged.AddListener(OnTextChanged);
        _inputField.onSelect.AddListener(OnInputSelected);
        _inputField.onDeselect.AddListener(OnInputDeselected);

        if (ArabicEnglishManager.Instance != null)
            ArabicEnglishManager.Instance.RegisterInput(this);
    }

    void Update()
    {
        if (_inputField == null || !_inputField.isFocused) return;

        // Support both new Input System and legacy
        bool backDown = false; bool backHeld = false; bool ctrl = false;
#if ENABLE_INPUT_SYSTEM || UNITY_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            backDown = kb.backspaceKey.wasPressedThisFrame;
            backHeld = kb.backspaceKey.isPressed;
            ctrl = kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed;
        }
#endif
#if !ENABLE_INPUT_SYSTEM && !UNITY_INPUT_SYSTEM
        backDown = Input.GetKeyDown(KeyCode.Backspace);
        backHeld = Input.GetKey(KeyCode.Backspace);
        ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#endif

        _backspaceTimer -= Time.unscaledDeltaTime;

        // Backspace logic (first delay then faster repeat)
        if ((backDown || (backHeld && _backspaceTimer <= 0f)) && _rawText.Length > 0)
        {
            _rawText = _rawText.Remove(_rawText.Length - 1, 1);
            _inputField.SetTextWithoutNotify(_rawText);
            _inputField.stringPosition = _rawText.Length;
            _inputField.caretPosition = _rawText.Length;
            UpdateDisplayText();
            _backspaceTimer = backDown ? backspaceFirstDelay : backspaceRepeatDelay;
        }

        // CTRL + A select all
        if (ctrl && backDown == false && backHeld == false) // check each frame; more explicit selection on Ctrl+A key press using new system below
        {
#if ENABLE_INPUT_SYSTEM || UNITY_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame)
            {
                _inputField.selectionStringAnchorPosition = 0;
                _inputField.selectionStringFocusPosition = _rawText.Length;
            }
#else
            if (Input.GetKeyDown(KeyCode.A))
            {
                _inputField.selectionStringAnchorPosition = 0;
                _inputField.selectionStringFocusPosition = _rawText.Length;
            }
#endif
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
        if (_displayText == null) return;
        var mgr = ArabicEnglishManager.Instance;
        bool isArabic = mgr != null && mgr.CurrentLanguage == ArabicEnglishManager.Language.Arabic;
        string shaped = isArabic ? ArabicFixer.Fix(_rawText) : _rawText;
        _displayText.alignment = isArabic ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
        string fitted = FitToSingleLine(shaped, isArabic);
        _displayText.text = fitted;
    }

    /// <summary>
    /// Fit text to single line. For LTR (English) we keep the tail (newest at end) by trimming the start.
    /// For Arabic (plugin reverses visual order) newest glyphs are at the BEGINNING of the shaped string,
    /// so we keep the head and trim the tail.
    /// </summary>
    private string FitToSingleLine(string full, bool isArabic)
    {
        if (string.IsNullOrEmpty(full) || _displayText == null) return full;

        float viewWidth = GetViewWidth();
        if (viewWidth <= 0f) return full;

        if (GetTextWidth(full) <= viewWidth) return full; // fits already

        if (!isArabic)
        {
            // Existing logic: keep tail
            int lo = 0; int hi = full.Length;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                string candidate = full.Substring(mid);
                if (GetTextWidth(candidate) > viewWidth) lo = mid + 1; else hi = mid;
            }
            return full.Substring(lo);
        }
        else
        {
            // Arabic: keep head (start of shaped string), trim tail.
            // Binary search maximum length L such that substring(0, L) fits.
            int lo = 0; // fits
            int hi = full.Length; // exclusive
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2; // bias upward to find max
                string candidate = full.Substring(0, mid);
                if (GetTextWidth(candidate) <= viewWidth) lo = mid; else hi = mid - 1;
            }
            return full.Substring(0, lo);
        }
    }

    private float GetViewWidth()
    {
        if (_displayText == null) return 0f;
        var rt = _displayText.rectTransform;
        float w = rt.rect.width;
        if (w <= 0f && rt.hasChanged)
        {
            Canvas.ForceUpdateCanvases();
            w = rt.rect.width;
        }
        _cachedViewWidth = w;
        return w;
    }

    private float GetTextWidth(string s)
    {
        // Use TMP preferred values (x component is width)
        Vector2 pref = _displayText.GetPreferredValues(s, Mathf.Infinity, Mathf.Infinity);
        return pref.x;
    }

    void OnInputSelected(string text)
    {
        if (_placeholderText != null)
            _placeholderText.gameObject.SetActive(false);
    }

    void OnInputDeselected(string text)
    {
        if (_placeholderText != null)
        {
            bool isEmpty = string.IsNullOrEmpty(_rawText);
            _placeholderText.gameObject.SetActive(isEmpty);

            if (isEmpty && ArabicEnglishManager.Instance != null)
            {
                if (ArabicEnglishManager.Instance.CurrentLanguage == ArabicEnglishManager.Language.Arabic)
                {
                    _placeholderText.alignment = TextAlignmentOptions.Right;
                    _placeholderText.text = ArabicFixer.Fix(_arabicPlaceholder);
                }
                else
                {
                    _placeholderText.alignment = TextAlignmentOptions.Left;
                    _placeholderText.text = _englishPlaceholder;
                }
            }
        }
    }

    // called by manager when language changes
    public void ApplyLanguage(ArabicEnglishManager.Language lang)
    {
        if (_placeholderText != null)
        {
            if (lang == ArabicEnglishManager.Language.Arabic)
            {
                _placeholderText.alignment = TextAlignmentOptions.Right;
                _placeholderText.text = ArabicFixer.Fix(_arabicPlaceholder);
            }
            else
            {
                _placeholderText.alignment = TextAlignmentOptions.Left;
                _placeholderText.text = _englishPlaceholder;
            }
        }

        // update display text / alignment
        if (_displayText != null)
        {
            if (lang == ArabicEnglishManager.Language.Arabic)
            {
                _displayText.alignment = TMPro.TextAlignmentOptions.Right;
                _displayText.text = ArabicFixer.Fix(_rawText);
            }
            else
            {
                _displayText.alignment = TMPro.TextAlignmentOptions.Left;
                _displayText.text = _rawText;
            }
        }

        // keep caret visible: hide input text component in Arabic, show in English
        if (_inputField.textComponent != null)
        {
            if (lang == ArabicEnglishManager.Language.Arabic)
                _inputField.textComponent.color = new Color(1, 1, 1, 0.01f);
            else
                _inputField.textComponent.color = new Color(1, 1, 1, 1f);
        }
    }

    void OnDestroy()
    {
        if (_inputField != null)
        {
            _inputField.onValueChanged.RemoveListener(OnTextChanged);
            _inputField.onSelect.RemoveListener(OnInputSelected);
            _inputField.onDeselect.RemoveListener(OnInputDeselected);
        }

        if (ArabicEnglishManager.Instance != null)
            ArabicEnglishManager.Instance.UnregisterInput(this);
    }
}
