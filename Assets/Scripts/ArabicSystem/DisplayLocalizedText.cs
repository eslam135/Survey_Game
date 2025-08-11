using UnityEngine;
using TMPro;
using ArabicSupport;

/// <summary>
/// Attach to any TMP_Text you want auto localized & Arabic-shaped without manual manager list setup.
/// 1. Put the ENGLISH text (key) in the TMP_Text initially (e.g. "Login", "Settings").
/// 2. Optionally override Arabic translation in the inspector.
/// 3. When language is Arabic it will:
///    - Use explicit Arabic override if provided.
///    - Else look up a built‑in dictionary of common UI words.
///    - Else if the original text already contains Arabic letters, shape that directly.
///    - Then apply ArabicFixer.Fix to shape and set alignment Right.
/// 4. When language is English it restores the original English key & alignment Left.
/// Works even if the manager starts already in Arabic (no need to toggle first).
/// It polls language change cheaply (stores last value) to avoid needing an event.
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
        { "setting things up","إعداد الأمور" },
        {"loading","جار التحميل"},
        {"error","خطأ"},
        {"success","نجاح"},
        {"welcome","مرحبا"},
        {"goodbye","وداعا"},
        {"yes","نعم"},
        {"no","لا"},
        {"ok","حسنا"},
        {"cancel","إلغاء"},

    };

    void Awake(){ Cache(); ApplyIfNeeded(force:true); }
    void OnEnable(){ Cache(); ApplyIfNeeded(force:true); }
    void Update(){ ApplyIfNeeded(); }

    void Cache()
    {
        if (_initialized && _tmp != null) return;
        _tmp = GetComponent<TMP_Text>();
        if (_tmp == null) return;
        if (string.IsNullOrEmpty(_originalEnglish)) _originalEnglish = _tmp.text; // take whatever is there as the English key
        _initialized = true;
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
            string shaped = ArabicFixer.Fix(arabicRaw);
            _tmp.text = shaped;
            if (forceArabicRight) _tmp.alignment = TextAlignmentOptions.Right;
        }
        else
        {
            _tmp.text = _originalEnglish;
            if (forceEnglishLeft) _tmp.alignment = TextAlignmentOptions.Left;
        }
    }

    string GetArabicRaw()
    {
        if (!string.IsNullOrEmpty(explicitArabicTranslation)) return explicitArabicTranslation.Trim();
        if (autoLookupCommonWords && COMMON.TryGetValue(_originalEnglish.Trim(), out var found)) return found;
        if (autoDetectArabicInOriginal && ContainsArabic(_originalEnglish)) return _originalEnglish; // already Arabic text typed in inspector
        // Fallback: just use original English (will look odd but shaped anyway)
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
