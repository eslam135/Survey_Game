using UnityEngine;
using TMPro;
using ArabicSupport;

[RequireComponent(typeof(TextMeshProUGUI))]
public class test : MonoBehaviour
{ private TMP_InputField inputField;
    private TextMeshProUGUI textComponent;
    private TextMeshProUGUI placeholderComponent;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        textComponent = inputField.textComponent as TextMeshProUGUI;
        placeholderComponent = inputField.placeholder as TextMeshProUGUI;

        // Listen for text changes
        inputField.onValueChanged.AddListener(OnValueChanged);

        // Fix placeholder if it exists
        if (placeholderComponent != null)
        {
            placeholderComponent.text = ArabicFixer.Fix(placeholderComponent.text);
        }
    }

    private void OnValueChanged(string newText)
    {
        // Keep caret position intact
        int caretPos = inputField.caretPosition;

        // Apply Arabic fixer
        string fixedText = ArabicFixer.Fix(newText);
        textComponent.text = fixedText;

        // Restore caret
        inputField.caretPosition = caretPos;
    }}
