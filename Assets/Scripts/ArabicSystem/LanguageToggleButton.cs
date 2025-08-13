using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArabicSupport;
using System.Collections; // added for coroutine

/// <summary>
/// Attach to a Unity UI Button (or any object with a Button component).
/// Clicking toggles Arabic/English via ArabicEnglishManager.
/// Optional TMP_Text label updates automatically.
/// </summary>
[RequireComponent(typeof(Button))]
public class LanguageToggleButton : MonoBehaviour
{
    [Header("UI Refs")] 
    [SerializeField] private Button _button;           // If left null, grabbed from same GameObject
    [SerializeField] private TMP_Text _label;          // Optional label to show language

    [Header("Labels")]
    [Tooltip("Label when showing English (current or next depending on mode).")]
    [SerializeField] private string englishLabel = "English";
    [Tooltip("Label (raw) for Arabic. Will be passed through ArabicFixer when shown.")]
    [SerializeField] private string arabicLabel = "العربية";

    [Header("Behavior")]
    [Tooltip("If true: label shows the NEXT language (what will happen on click). If false: shows CURRENT language.")]
    [SerializeField] private bool showNextLanguage = true;
    [Tooltip("What to assume for initial label BEFORE ArabicEnglishManager exists (first frame order issues).")]
    [SerializeField] private ArabicEnglishManager.Language defaultLanguageWhenManagerMissing = ArabicEnglishManager.Language.English;

    [Header("Alignment Override")]
    [Tooltip("If false, this script will NOT modify alignment.")]
    [SerializeField] private bool autoAlign = true;
    [SerializeField] private TextAlignmentOptions englishAlignment = TextAlignmentOptions.Center;
    [SerializeField] private TextAlignmentOptions arabicAlignment = TextAlignmentOptions.Right;

    // Track last displayed language to auto-refresh when manager changes elsewhere
    private ArabicEnglishManager.Language _lastDisplayedLang;
    private bool _hasLastDisplayed = false;

    private Coroutine _waitRoutine;

    private void Awake()
    {
        if (_button == null) _button = GetComponent<Button>();
        if (_label == null) _label = GetComponentInChildren<TMP_Text>(true);
        if (_button != null) _button.onClick.AddListener(OnClickToggle);
        UpdateLabel(); // will use fallback if manager not ready yet
    }

    private void OnEnable()
    {
        UpdateLabel();
        // If manager not ready yet (script execution order), wait until it appears then refresh once
        if (ArabicEnglishManager.Instance == null && _waitRoutine == null)
            _waitRoutine = StartCoroutine(WaitForManagerThenRefresh());
        _hasLastDisplayed = false; // force first Update() to refresh
    }

    private IEnumerator WaitForManagerThenRefresh()
    {
        // Wait only until instance becomes available
        while (ArabicEnglishManager.Instance == null)
            yield return null; // next frame
        _waitRoutine = null;
        UpdateLabel();
    }

    private void OnDisable()
    {
        if (_waitRoutine != null)
        {
            StopCoroutine(_waitRoutine);
            _waitRoutine = null;
        }
    }

    private void OnDestroy()
    {
        if (_button != null) _button.onClick.RemoveListener(OnClickToggle);
    }

    private void Update()
    {
        if (_label == null) return;
        var mgr = ArabicEnglishManager.Instance;
        var current = mgr != null ? mgr.CurrentLanguage : defaultLanguageWhenManagerMissing;
        var langToDisplay = showNextLanguage
            ? (current == ArabicEnglishManager.Language.English ? ArabicEnglishManager.Language.Arabic : ArabicEnglishManager.Language.English)
            : current;

        if (!_hasLastDisplayed || _lastDisplayedLang != langToDisplay)
        {
            UpdateLabel();
            _lastDisplayedLang = langToDisplay;
            _hasLastDisplayed = true;
        }
    }

    private void OnClickToggle()
    {
        if (ArabicEnglishManager.Instance == null) return;
        ArabicEnglishManager.Instance.ToggleLanguage();
        UpdateLabel();
    }

    public void UpdateLabel()
    {
        if (_label == null) return;

        // Determine current (or default) language
        var mgr = ArabicEnglishManager.Instance;
        var current = mgr != null ? mgr.CurrentLanguage : defaultLanguageWhenManagerMissing;

        // Decide which language the label should display
        ArabicEnglishManager.Language langToDisplay;
        if (showNextLanguage)
            langToDisplay = current == ArabicEnglishManager.Language.English ? ArabicEnglishManager.Language.Arabic : ArabicEnglishManager.Language.English;
        else
            langToDisplay = current;

        if (langToDisplay == ArabicEnglishManager.Language.Arabic)
        {
            if (autoAlign) _label.alignment = arabicAlignment;
            _label.text = ArabicFixer.Fix(arabicLabel);
        }
        else
        {
            if (autoAlign) _label.alignment = englishAlignment;
            _label.text = englishLabel;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (_label == null) _label = GetComponentInChildren<TMP_Text>(true);
            UpdateLabel();
        }
    }
#endif
}
