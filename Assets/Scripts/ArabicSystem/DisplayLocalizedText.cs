using UnityEngine;
using TMPro;
using ArabicSupport;

/// <summary>
/// Attach to any TMP_Text you want auto localized & Arabic-shaped without manual manager list setup.
/// Handles runtime-created clones and texts that are assigned after Awake (e.g. player name set later).
/// </summary>
[ExecuteAlways]
public class DisplayLocalizedText : MonoBehaviour
{
    [TextArea] public string explicitArabicTranslation = ""; // Optional manual Arabic (unshaped) override
    public bool autoLookupCommonWords = true;
    public bool autoDetectArabicInOriginal = true;

    [Header("Alignment Overrides")] public bool forceEnglishLeft = true; public bool forceArabicRight = true;

    TMP_Text _tmp;
    string _originalEnglish; // stored key
    ArabicEnglishManager.Language _lastLang;
    bool _initialized;

    // registration state with manager
    bool _registeredWithManager = false;
    bool _pendingRegister = false;

    // Optional: enable extra debug logs for troubleshooting
    public bool enableDebugLogs = false;

    // Common UI translations (raw Arabic, unshaped)
    static readonly System.Collections.Generic.Dictionary<string,string> COMMON = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
    {
        {"login","تسجيل الدخول"},
        {"log in","تسجيل الدخول"},
        {"signup","إنشاء حساب"},
        {"sign up","إنشاء حساب"},
        {"create account","إنشاء حساب"},
        {"settings","الإعدادات"},
        {"back","رجوع"},
        {"resume","استمرار"},
        {"quit","خروج"},
        {"exit","خروج"},
        {"start","ابدأ"},
        {"play","العب"},
        {"continue","متابعة"},
        {"main menu","القائمة الرئيسية"},
        {"controls","عناصر التحكم"},
        {"how to play","كيفية اللعب"},
        {"create room","إنشاء غرفة"},
        {"join room","انضم إلى غرفة"},
        {"send","ارسل"},
        {"please choose your avatar","من فضلك اختر شخصيتك"},
        { "setting things up","جاري إعداد الأمور" },
        {"loading","جار التحميل"},
        {"error","خطأ"},
        {"success","نجاح"},
        {"welcome","مرحبا"},
        {"goodbye","وداعا"},
        {"yes","نعم"},
        {"no","لا"},
        {"ok","حسنا"},
        {"cancel","إلغاء"},
        { "Enter the room code","أدخل رمز الغرفة" },
    };

    void Awake()
    {
        Cache();
        ApplyIfNeeded(force:true);
    }

    void OnEnable()
    {
        Cache();

        // skip registration if this TMP_Text belongs to an ArabicManagedInput (same object or parent)
        if (_tmp != null && GetComponentInParent<ArabicManagedInput>() == null)
            TryRegister(); // will register as soon as Instance exists (no need to wait for mgr.enabled)

        ApplyIfNeeded(force:true);
    }

    void OnDisable()
    {
        // if we registered already, unregister; otherwise cancel pending
        if (_registeredWithManager)
        {
            if (ArabicEnglishManager.Instance != null)
                ArabicEnglishManager.Instance.UnregisterDisplayOnlyText(_tmp);
            _registeredWithManager = false;
        }
        _pendingRegister = false;
    }

    void OnDestroy()
    {
        if (_registeredWithManager && ArabicEnglishManager.Instance != null)
            ArabicEnglishManager.Instance.UnregisterDisplayOnlyText(_tmp);
        _registeredWithManager = false;
        _pendingRegister = false;
    }

    void Cache()
    {
        if (_initialized && _tmp != null) return;
        _tmp = GetComponent<TMP_Text>();
        if (_tmp == null) return;
        // If there's already text set in inspector/prefab, use it. If it's empty, we'll capture it later.
        if (!string.IsNullOrEmpty(_tmp.text) && string.IsNullOrEmpty(_originalEnglish))
            _originalEnglish = _tmp.text;
        _initialized = true;
    }

    void Update()
    {
        // language application logic
        ApplyIfNeeded();

        // cheap retry for registration when manager.Instance becomes available later
        if (_pendingRegister)
        {
            var mgr = ArabicEnglishManager.Instance;
            if (mgr != null)
            {
                mgr.RegisterDisplayOnlyText(_tmp);
                _registeredWithManager = true;
                _pendingRegister = false;
                if (enableDebugLogs) Debug.Log($"[DisplayLocalizedText] Registered (late) {_tmp.name}");
            }
        }

        // If original key was empty (prefab had empty text) but text is now populated, capture it and reapply language.
        if (string.IsNullOrEmpty(_originalEnglish) && _tmp != null && !string.IsNullOrEmpty(_tmp.text))
        {
            _originalEnglish = _tmp.text;
            if (enableDebugLogs) Debug.Log($"[DisplayLocalizedText] Captured original text for {_tmp.name}: '{_originalEnglish}'");
            ApplyIfNeeded(force:true);
        }
    }

    void TryRegister()
    {
        if (_tmp == null) return;
        var mgr = ArabicEnglishManager.Instance;
        if (mgr != null)
        {
            // Manager instance exists (register regardless of mgr.enabled)
            mgr.RegisterDisplayOnlyText(_tmp);
            _registeredWithManager = true;
            _pendingRegister = false;
            if (enableDebugLogs) Debug.Log($"[DisplayLocalizedText] Registered {_tmp.name}");
        }
        else
        {
            // Manager missing -> mark pending, Update will retry
            _pendingRegister = true;
            if (enableDebugLogs) Debug.Log($"[DisplayLocalizedText] Pending register for {_tmp.name}");
        }
    }

    void ApplyIfNeeded(bool force=false)
    {
        var mgr = ArabicEnglishManager.Instance; // may be null in edit mode
        var lang = mgr != null ? mgr.CurrentLanguage : ArabicEnglishManager.Language.English;
        if (!force && lang == _lastLang) return;
        _lastLang = lang;
        ApplyLanguage(lang);
    }

    void ApplyLanguage(ArabicEnglishManager.Language lang)
    {
        if (_tmp == null) return;

        if (lang == ArabicEnglishManager.Language.Arabic)
        {
            string arabicRaw = GetArabicRaw();
            // Apply Arabic shaping/fixing
            string shaped = ArabicFixer.Fix(arabicRaw ?? "");

            // Set text and force TMP to rebuild the mesh so it renders correctly right away
            _tmp.text = shaped;
            _tmp.ForceMeshUpdate();
            UnityEngine.Canvas.ForceUpdateCanvases();

            if (forceArabicRight) _tmp.alignment = TextAlignmentOptions.Right;

            if (enableDebugLogs)
                Debug.Log($"[DisplayLocalizedText] Applied Arabic to '{_tmp.name}': raw='{arabicRaw}' -> shaped='{shaped}' registered={_registeredWithManager}");
        }
        else
        {
            // Restore original (if it's still empty, leave the TMP's current text as-is)
            if (!string.IsNullOrEmpty(_originalEnglish))
                _tmp.text = _originalEnglish;

            _tmp.ForceMeshUpdate();
            UnityEngine.Canvas.ForceUpdateCanvases();

            if (forceEnglishLeft) _tmp.alignment = TextAlignmentOptions.Left;

            if (enableDebugLogs)
                Debug.Log($"[DisplayLocalizedText] Applied English to '{_tmp.name}': '{_originalEnglish}'");
        }
    }

    string GetArabicRaw()
    {
        if (!string.IsNullOrEmpty(explicitArabicTranslation)) return explicitArabicTranslation.Trim();
        if (autoLookupCommonWords && !string.IsNullOrEmpty(_originalEnglish) && COMMON.TryGetValue(_originalEnglish.Trim(), out var found)) return found;
        if (autoDetectArabicInOriginal && !string.IsNullOrEmpty(_originalEnglish) && ContainsArabic(_originalEnglish)) return _originalEnglish; // already Arabic text typed in inspector
        // If originalEnglish is empty, fallback to the TMP's current text (useful when names are assigned at runtime)
        if (string.IsNullOrEmpty(_originalEnglish) && _tmp != null && !string.IsNullOrEmpty(_tmp.text))
            return _tmp.text;
        // final fallback
        return _originalEnglish;
    }

    static bool ContainsArabic(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        foreach (char c in s)
        {
            if ((c >= '\u0600' && c <= '\u06FF') || (c >= '\u0750' && c <= '\u077F') || (c >= '\u08A0' && c <= '\u08FF'))
                return true;
        }
        return false;
    }
}
