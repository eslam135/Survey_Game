using ArabicSupport;
using TMPro;
using UnityEngine;

// Attach to any TextMeshProUGUI or TMP_Text element to apply ArabicFixer automatically.
[RequireComponent(typeof(TMP_Text))]
public class ArabicTMPAutoFix : MonoBehaviour
{
    [Tooltip("Force Arabic fixing on this text. If you have a global language manager, you can toggle this at runtime.")]
    public bool enableArabicFix = true;

    [Tooltip("When true, set alignment to Right for Arabic.")]
    public bool autoAlignment = true;

    [Tooltip("Optionally read language from ArabicEnglishManager if present.")]
    public bool useGlobalLanguage = true;

    private TMP_Text _tmp;
    private ITextPreprocessor _originalPreprocessor;
    private ArabicTMPPreprocessor _arabicPreprocessor;

    void Awake()
    {
        _tmp = GetComponent<TMP_Text>();
        _originalPreprocessor = _tmp.textPreprocessor;
        _arabicPreprocessor = new ArabicTMPPreprocessor(() => ShouldFix());
        ApplyPreprocessor();
    }

    void OnEnable()
    {
        ApplyPreprocessor();
        ApplyAlignment();
        ForceRefresh();
    }

    void OnDisable()
    {
        // Restore original preprocessor to avoid side effects
        if (_tmp != null)
            _tmp.textPreprocessor = _originalPreprocessor;
    }

    void OnDestroy()
    {
        if (_tmp != null)
            _tmp.textPreprocessor = _originalPreprocessor;
    }

    private bool ShouldFix()
    {
        if (!enableArabicFix) return false;
        if (!useGlobalLanguage) return true;

        var mgr = ArabicEnglishManager.Instance;
        return mgr == null || mgr.CurrentLanguage == ArabicEnglishManager.Language.Arabic;
    }

    private void ApplyPreprocessor()
    {
        if (_tmp == null) return;
        _tmp.textPreprocessor = _arabicPreprocessor;
    }

    private void ApplyAlignment()
    {
        if (!autoAlignment || _tmp == null) return;
        bool isArabic = ShouldFix();
        _tmp.alignment = isArabic ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
    }

    public void Refresh()
    {
        ApplyPreprocessor();
        ApplyAlignment();
        ForceRefresh();
    }

    private void ForceRefresh()
    {
        if (_tmp == null) return;
        // Force TMP to re-generate
        var t = _tmp.text;
        _tmp.text = t;
        _tmp.ForceMeshUpdate();
    }
}
