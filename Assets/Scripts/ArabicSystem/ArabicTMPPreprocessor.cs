using System;
using ArabicSupport;
using TMPro;

// Applies Arabic shaping before TMP parses/layouts text.
public class ArabicTMPPreprocessor : ITextPreprocessor
{
    private readonly Func<bool> _shouldFix;

    public ArabicTMPPreprocessor(Func<bool> shouldFix = null)
    {
        _shouldFix = shouldFix ?? (() => true);
    }

    public string PreprocessText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        try
        {
            return _shouldFix() ? ArabicFixer.Fix(text) : text;
        }
        catch
        {
            return text;
        }
    }
}
