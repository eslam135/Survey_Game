// AdvancedTMPInputForceRefresh_HandlesDisabled.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ArabicSupport;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AdvancedTMPInputForceRefresh : MonoBehaviour
{
    [Header("Auto refresh settings")]
    public bool autoRefreshOnLanguageChange = true;
    public bool pollIfNoEvent = true;
    public float pollInterval = 0.25f;
    public bool allowAggressiveFallback = true;

    // static registry so we can call RefreshAllNow() from anywhere (even if objects are disabled)
    private static readonly List<AdvancedTMPInputForceRefresh> s_instances = new List<AdvancedTMPInputForceRefresh>();

    // runtime refs
    TMP_InputField tmpInput;
    TMP_Text inputTextComp;
    TMP_Text placeholderText;
    Coroutine pollCoroutine;
    bool lastIsArabic;

    // if a language-change happens while this component is disabled/inactive, mark pending
    bool pendingRefresh = false;

    void Awake()
    {
        // add to registry (Awake runs even if component is disabled)
        s_instances.Add(this);

        tmpInput = GetComponent<TMP_InputField>() ?? GetComponentInChildren<TMP_InputField>(true);
        if (tmpInput != null)
        {
            inputTextComp = tmpInput.textComponent;
            if (tmpInput.placeholder != null)
                placeholderText = tmpInput.placeholder.GetComponent<TMP_Text>();
        }

        // subscribe to manager event if possible; else start poll coroutine if allowed
        if (!TrySubscribeLanguageEvent() && pollIfNoEvent)
        {
            lastIsArabic = IsArabicMode();
            pollCoroutine = StartCoroutine(LanguagePoller());
        }
    }

    void OnEnable()
    {
        // If a language-change occurred while disabled, refresh now
        if (pendingRefresh && autoRefreshOnLanguageChange)
        {
            pendingRefresh = false;
            RefreshNow();
        }
        else if (autoRefreshOnLanguageChange)
        {
            // optional: ensure visible state in case mode changed while active
            RefreshNow();
        }
    }

    void OnDisable()
    {
        // don't stop coroutines here because Awake may have started a poll coroutine
        if (pollCoroutine != null)
        {
            StopCoroutine(pollCoroutine);
            pollCoroutine = null;
        }
    }

    void OnDestroy()
    {
        s_instances.Remove(this);
    }

    /// <summary>
    /// Public API: refresh this instance. If the component is disabled/inactive we mark it pending so it will refresh when enabled.
    /// </summary>
    public void RefreshNow()
    {
        // If component isn't active & enabled we can't start coroutines or rely on Unity UI updates.
        // Mark pending so OnEnable will refresh it later.
        if (!isActiveAndEnabled)
        {
            pendingRefresh = true;
            return;
        }

        StopAllCoroutines();
        StartCoroutine(RefreshSequence());
    }

    /// <summary>
    /// Call this to force all registered refreshers to refresh (works for active and inactive objects).
    /// </summary>
    public static void RefreshAllNow()
    {
        // iterate a copy to be safe if collection changes while iterating
        var copy = s_instances.ToArray();
        foreach (var inst in copy)
        {
            inst.RefreshNow();
        }
    }

    private IEnumerator LanguagePoller()
    {
        while (true)
        {
            bool nowArabic = IsArabicMode();
            if (nowArabic != lastIsArabic)
            {
                lastIsArabic = nowArabic;
                if (autoRefreshOnLanguageChange) RefreshNow();
            }
            yield return new WaitForSeconds(Mathf.Max(0.05f, pollInterval));
        }
    }

    private bool IsArabicMode()
    {
        if (ArabicEnglishManager.Instance == null) return true;
        try { return ArabicEnglishManager.Instance.CurrentLanguage == ArabicEnglishManager.Language.Arabic; }
        catch { return true; }
    }

    // Try to hook manager event names (best-effort)
    private bool TrySubscribeLanguageEvent()
    {
        var mgr = ArabicEnglishManager.Instance;
        if (mgr == null) return false;

        var type = mgr.GetType();
        string[] candidate = { "OnLanguageChanged", "LanguageChanged", "LanguageChange", "OnLanguageChange" };
        foreach (var name in candidate)
        {
            var ev = type.GetEvent(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (ev == null) continue;
            try
            {
                var d = Delegate.CreateDelegate(ev.EventHandlerType, this, nameof(OnManagerLanguageChanged));
                ev.AddEventHandler(mgr, d);
                return true;
            }
            catch
            {
                try { ev.AddEventHandler(mgr, (Action)OnManagerLanguageChanged); return true; }
                catch { /* ignore */ }
            }
        }
        return false;
    }

    private void OnManagerLanguageChanged() 
    {
        if (autoRefreshOnLanguageChange) RefreshNow();
    }

    /// <summary>
    /// The main refresh coroutine: updates alignments, forces TMP/Internal updates and does safe nudges.
    /// </summary>
    private IEnumerator RefreshSequence()
    {
        bool isArabic = IsArabicMode();

        // 1) Update alignment + mark graphics dirty
        if (inputTextComp != null)
        {
            inputTextComp.alignment = isArabic ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
            // Enable TMP RTL rendering so caret/selection behave correctly
            try { inputTextComp.isRightToLeftText = isArabic; } catch { }
            SafeSetGraphicDirty(inputTextComp);
            inputTextComp.ForceMeshUpdate();
        }
        if (placeholderText != null)
        {
            placeholderText.alignment = isArabic ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
            try { placeholderText.isRightToLeftText = isArabic; } catch { }
            SafeSetGraphicDirty(placeholderText);
            placeholderText.ForceMeshUpdate();
        }

        // Try to set RTL on the TMP_InputField itself (property name can vary per TMP version)
        if (tmpInput != null)
        {
            try
            {
                var pi = tmpInput.GetType().GetProperty("isRightToLeftText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (pi != null && pi.CanWrite)
                {
                    pi.SetValue(tmpInput, isArabic, null);
                }
                else
                {
                    var fi = tmpInput.GetType().GetField("m_IsRightToLeft", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                             ?? tmpInput.GetType().GetField("m_isRightToLeft", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (fi != null) fi.SetValue(tmpInput, isArabic);
                }
            }
            catch { /* ignore reflection failures */ }
        }

        // 2) Reassign text without notify to force TMP internals
        if (tmpInput != null)
            tmpInput.SetTextWithoutNotify(tmpInput.text ?? "");

        // 3) Try calling internal UpdateLabel() on TMP_InputField via reflection
        if (tmpInput != null)
        {
            var mi = tmpInput.GetType().GetMethod("UpdateLabel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (mi != null)
            {
                try { mi.Invoke(tmpInput, null); }
                catch { /* ignore */ }
            }
            try { tmpInput.ForceLabelUpdate(); } catch { }
        }

        // 4) Force layout rebuild on the input rect
        var r = tmpInput != null ? tmpInput.GetComponent<RectTransform>() : GetComponent<RectTransform>();
        if (r != null) LayoutRebuilder.ForceRebuildLayoutImmediate(r);

        // 5) Force canvas update & wait a frame
        Canvas.ForceUpdateCanvases();
        yield return null;

        // 6) nudge interactable
        if (tmpInput != null)
        {
            bool old = tmpInput.interactable;
            tmpInput.interactable = !old;
            yield return null;
            tmpInput.interactable = old;
            yield return null;
        }

        // 7) briefly disable/enable the TMP_InputField component (safe)
        if (tmpInput != null)
        {
            var comp = tmpInput as Behaviour;
            if (comp != null)
            {
                bool oldE = comp.enabled;
                comp.enabled = false;
                yield return null;
                comp.enabled = oldE;
                yield return null;
            }
        }

        // 8) EventSystem deselect/select to nudge visuals
        if (EventSystem.current != null)
        {
            var prev = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(null);
            yield return null;
            EventSystem.current.SetSelectedGameObject(prev);
            yield return null;
        }

        // 9) Aggressive fallback: toggle placeholder off/on if allowed
        if (allowAggressiveFallback && placeholderText != null && placeholderText.gameObject.activeSelf)
        {
            placeholderText.gameObject.SetActive(false);
            yield return null;
            placeholderText.gameObject.SetActive(true);
            yield return null;
        }

        // final layout/canvas update
        if (r != null) LayoutRebuilder.ForceRebuildLayoutImmediate(r);
        Canvas.ForceUpdateCanvases();
    }

    // helper: call protected Graphic.SetAllDirty / SetVerticesDirty via reflection (some TMP versions require this)
    void SafeSetGraphicDirty(Graphic g)
    {
        if (g == null) return;
        try
        {
            var mi = typeof(Graphic).GetMethod("SetAllDirty", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (mi != null) mi.Invoke(g, null);
            else
            {
                var sv = typeof(Graphic).GetMethod("SetVerticesDirty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                sv?.Invoke(g, null);
                var sl = typeof(Graphic).GetMethod("SetLayoutDirty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                sl?.Invoke(g, null);
            }
        }
        catch { /* ignore reflection failures */ }
    }
}
