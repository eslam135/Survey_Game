using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ArabicSupport;

public class ArabicEnglishManager : MonoBehaviour
{
    public enum Language { English, Arabic }
    public static ArabicEnglishManager Instance { get; private set; }

    [Header("Optional: drag Managed Inputs here (they also auto-register)")]
    public List<ArabicManagedInput> managedInputs = new List<ArabicManagedInput>();

    [Header("Display-only TMP Texts (labels, titles, etc.)")]
    public List<TMP_Text> displayOnlyTexts = new List<TMP_Text>();

    Dictionary<TMP_Text, string> _originalDisplayText = new Dictionary<TMP_Text, string>();

    public Language CurrentLanguage { get; private set; } = Language.Arabic; // default Arabic to match original behavior

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var t in displayOnlyTexts)
            if (t != null && !_originalDisplayText.ContainsKey(t))
                _originalDisplayText[t] = t.text;

        ApplyLanguageToAll();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterInput(ArabicManagedInput input)
    {
        if (input == null) return;
        if (!managedInputs.Contains(input)) managedInputs.Add(input);
        input.ApplyLanguage(CurrentLanguage);
    }

    public void UnregisterInput(ArabicManagedInput input)
    {
        if (input == null) return;
        managedInputs.Remove(input);
    }

    public void RegisterDisplayOnlyText(TMP_Text t)
    {
        if (t == null) return;
        if (!displayOnlyTexts.Contains(t)) displayOnlyTexts.Add(t);
        if (!_originalDisplayText.ContainsKey(t)) _originalDisplayText[t] = t.text;
        ApplyLanguageToDisplayOnly(t, CurrentLanguage);
    }

    public void UnregisterDisplayOnlyText(TMP_Text t)
    {
        if (t == null) return;
        displayOnlyTexts.Remove(t);
        if (_originalDisplayText.ContainsKey(t)) _originalDisplayText.Remove(t);
    }

    public void ToggleLanguage() => SetLanguage(CurrentLanguage == Language.English ? Language.Arabic : Language.English);

    public void SetLanguage(Language lang)
    {
        if (CurrentLanguage == lang) return;
        CurrentLanguage = lang;
        ApplyLanguageToAll();
    }

    void ApplyLanguageToAll()
    {
        for (int i = 0; i < managedInputs.Count; i++)
        {
            var m = managedInputs[i];
            if (m != null) m.ApplyLanguage(CurrentLanguage);
        }

        foreach (var t in displayOnlyTexts)
            ApplyLanguageToDisplayOnly(t, CurrentLanguage);
    }

    void ApplyLanguageToDisplayOnly(TMP_Text t, Language lang)
    {
        if (t == null) return;

        // NEW: If the text has a DisplayLocalizedText component, let that component fully control
        // content, shaping, and alignment. Manager does nothing.
        if (t.GetComponent<DisplayLocalizedText>() != null) return;

        string original = _originalDisplayText.ContainsKey(t) ? _originalDisplayText[t] : t.text;

        if (lang == Language.Arabic)
        {
            t.alignment = TextAlignmentOptions.Right;
            t.text = ArabicFixer.Fix(original);
        }
        else
        {
            t.alignment = TextAlignmentOptions.Left;
            t.text = original;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Refresh Original Display Texts")]
    void RefreshOriginals()
    {
        _originalDisplayText.Clear();
        foreach (var t in displayOnlyTexts)
            if (t != null && !_originalDisplayText.ContainsKey(t))
                _originalDisplayText[t] = t.text;
    }
#endif
}
