using System;
using System.Reflection;
using ArabicSupport;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class PlaceholderLanguageSwitcher : MonoBehaviour
{
    [Tooltip("Placeholder to show when language is English. Leave empty to use 'Type here...'.")]
    public string englishPlaceholder = "Type here...";

    [Tooltip("Placeholder to show when language is Arabic. If empty, the current placeholder text will be captured as Arabic.")]
    [TextArea]
    public string arabicPlaceholder = "";

    [Tooltip("If true the script will check ArabicEnglishManager.Instance.CurrentLanguage. If false you must call Refresh() manually.")]
    public bool useGlobalLanguage = true;

    private TMP_Text _placeholderText;
    private TMP_InputField _inputField;

    void Awake()
    {
        _placeholderText = GetComponent<TMP_Text>();
        // find input field in parents in case script is on placeholder child
        _inputField = GetComponentInParent<TMP_InputField>();

        if (_placeholderText == null)
        {
            Debug.LogError("[PlaceholderLanguageSwitcher] No TMP_Text found on this GameObject.");
            enabled = false;
            return;
        }

        // capture existing placeholder as arabic default if not set
        if (string.IsNullOrEmpty(arabicPlaceholder))
            arabicPlaceholder = _placeholderText.text;

        TrySubscribeToLanguageChanged();
    }

    void OnEnable()
    {
        ApplyPlaceholder();
    }

    /// <summary>
    /// Set placeholder based on current language. Call this if you change language manually.
    /// </summary>
    public void Refresh()
    {
        ApplyPlaceholder();
    }

    private bool IsArabicMode()
    {
        if (!useGlobalLanguage) return true; // default to Arabic behavior when not using manager
        var mgr = ArabicEnglishManager.Instance;
        if (mgr == null) return true;
        return mgr.CurrentLanguage == ArabicEnglishManager.Language.Arabic;
    }

    private void ApplyPlaceholder()
    {
        if (_placeholderText == null) return;

        bool isArabic = IsArabicMode();

        if (isArabic)
        {
            // keep captured arabic placeholder if available, else keep current
            if (!string.IsNullOrEmpty(arabicPlaceholder))
                _placeholderText.text = arabicPlaceholder;
        }
        else
        {
            _placeholderText.text = string.IsNullOrEmpty(englishPlaceholder) ? "Type here..." : englishPlaceholder;
        }

        _placeholderText.ForceMeshUpdate();
    }

    // Best-effort: subscribe to event names common in language managers
    private void TrySubscribeToLanguageChanged()
    {
        var mgr = ArabicEnglishManager.Instance;
        if (mgr == null) return;

        var mgrType = mgr.GetType();
        string[] candidateEventNames = { "OnLanguageChanged", "LanguageChanged", "LanguageChange", "OnLanguageChange" };

        foreach (var evName in candidateEventNames)
        {
            var ev = mgrType.GetEvent(evName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (ev == null) continue;

            try
            {
                var handler = Delegate.CreateDelegate(ev.EventHandlerType, this, nameof(OnExternalLanguageChanged));
                ev.AddEventHandler(mgr, handler);
                return;
            }
            catch
            {
                // fallback: try to add parameterless Action
                try
                {
                    var action = (Action)OnExternalLanguageChanged;
                    ev.AddEventHandler(mgr, action);
                    return;
                }
                catch { /* ignore and continue */ }
            }
        }
    }

    private void OnExternalLanguageChanged()
    {
        ApplyPlaceholder();
    }
}
